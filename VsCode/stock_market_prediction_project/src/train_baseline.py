
from pathlib import Path
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestRegressor
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
from sklearn.model_selection import TimeSeriesSplit

from indicators import build_features
from data_loader import load_or_download_data
from backtest import long_flat_strategy, sharpe_ratio

def train_baseline(
    ticker: str = "AAPL",
    start: str = "2015-01-01",
    end: str = "2024-12-31",
    cache_dir: str = "data",
    output_dir: str = "outputs",
    tc_bps: float = 5.0,
):
    out_path = Path(output_dir)
    out_path.mkdir(parents=True, exist_ok=True)
    df = load_or_download_data(ticker, start, end, cache_dir)
    data = build_features(df)
    feature_cols = [c for c in data.columns if c not in ["Target_NextRet", "Return"]]
    X = data[feature_cols]
    y = data["Target_NextRet"]
    tscv = TimeSeriesSplit(n_splits=5)
    preds = pd.Series(index=y.index, dtype=float)
    for train_idx, test_idx in tscv.split(X):
        X_train, X_test = X.iloc[train_idx], X.iloc[test_idx]
        y_train, y_test = y.iloc[train_idx], y.iloc[test_idx]
        model = RandomForestRegressor(
            n_estimators=400,
            max_depth=None,
            min_samples_leaf=3,
            random_state=42,
            n_jobs=-1,
        )
        model.fit(X_train, y_train)
        preds.iloc[test_idx] = model.predict(X_test)
    valid = ~preds.isna()
    mae = mean_absolute_error(y[valid], preds[valid])
    rmse = mean_squared_error(y[valid], preds[valid], squared=False)
    r2 = r2_score(y[valid], preds[valid])
    bt = long_flat_strategy(preds, y, tc_bps=tc_bps)
    strat_ret = bt["StratRet"]
    cum_ret = bt["CumRet"]
    buy_hold = bt["BuyHold"]
    sharpe = sharpe_ratio(strat_ret)
    metrics = {
        "Ticker": ticker,
        "Start": start,
        "End": end,
        "MAE": float(mae),
        "RMSE": float(rmse),
        "R2": float(r2),
        "Sharpe": float(sharpe),
        "Final Cumulative Return (Strategy)": float(cum_ret.iloc[-1]),
        "Final Cumulative Return (Buy&Hold)": float(buy_hold.iloc[-1]),
        "TransactionCost_bps": float(tc_bps),
    }
    pd.Series(metrics).to_json(out_path / "metrics_baseline.json", indent=2)
    out_df = pd.DataFrame({
        "ActualNextRet": y,
        "PredNextRet": preds,
        "Signal": bt["Signal"],
        "StratRet": strat_ret,
        "CumRet": cum_ret,
        "BuyHold": buy_hold,
    })
    out_df.to_csv(out_path / "predictions_and_backtest_baseline.csv")
    plt.figure(figsize=(10, 5))
    cum_ret.plot(label="Strategy (after TC)")
    buy_hold.plot(label="Buy & Hold")
    plt.title(f"RandomForest Strategy vs Buy & Hold ({ticker})")
    plt.xlabel("Date")
    plt.ylabel("Cumulative Return")
    plt.legend()
    plt.tight_layout()
    plt.savefig(out_path / "cumulative_returns_baseline.png")
    plt.close()
    
    df = load_or_download_data(ticker, start, end, cache_dir)
    print("Raw data shape:", df.shape)   # <-- add this

    data = build_features(df)
    print("Feature data shape:", data.shape)  # <-- add this


if __name__ == "__main__":
    train_baseline()
