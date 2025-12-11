
from pathlib import Path
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.neural_network import MLPRegressor
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
from sklearn.model_selection import TimeSeriesSplit

from indicators import build_features
from data_loader import load_or_download_data
from backtest import long_flat_strategy, sharpe_ratio

def train_mlp(
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
    X = data[feature_cols].values
    y = data["Target_NextRet"].values
    tscv = TimeSeriesSplit(n_splits=5)
    preds = np.full_like(y, np.nan, dtype=float)
    for train_idx, test_idx in tscv.split(X):
        X_train, X_test = X[train_idx], X[test_idx]
        y_train, y_test = y[train_idx], y[test_idx]
        model = MLPRegressor(
            hidden_layer_sizes=(64, 64),
            activation="relu",
            solver="adam",
            alpha=1e-4,
            learning_rate="adaptive",
            max_iter=500,
            random_state=42,
        )
        model.fit(X_train, y_train)
        preds[test_idx] = model.predict(X_test)
    valid = ~np.isnan(preds)
    mae = mean_absolute_error(y[valid], preds[valid])
    rmse = mean_squared_error(y[valid], preds[valid], squared=False)
    r2 = r2_score(y[valid], preds[valid])
    idx = data.index
    preds_series = pd.Series(preds, index=idx)
    y_series = pd.Series(y, index=idx)
    bt = long_flat_strategy(preds_series, y_series, tc_bps=tc_bps)
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
    pd.Series(metrics).to_json(out_path / "metrics_mlp.json", indent=2)
    out_df = pd.DataFrame({
        "ActualNextRet": y_series,
        "PredNextRet": preds_series,
        "Signal": bt["Signal"],
        "StratRet": strat_ret,
        "CumRet": cum_ret,
        "BuyHold": buy_hold,
    })
    out_df.to_csv(out_path / "predictions_and_backtest_mlp.csv")
    plt.figure(figsize=(10, 5))
    cum_ret.plot(label="Strategy (after TC)")
    buy_hold.plot(label="Buy & Hold")
    plt.title(f"MLP Strategy vs Buy & Hold ({ticker})")
    plt.xlabel("Date")
    plt.ylabel("Cumulative Return")
    plt.legend()
    plt.tight_layout()
    plt.savefig(out_path / "cumulative_returns_mlp.png")
    plt.close()

if __name__ == "__main__":
    train_mlp()
