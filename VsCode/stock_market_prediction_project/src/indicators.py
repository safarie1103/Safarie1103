
import numpy as np
import pandas as pd

def sma(series: pd.Series, window: int) -> pd.Series:
    return series.rolling(window).mean()

def ema(series: pd.Series, span: int) -> pd.Series:
    return series.ewm(span=span, adjust=False).mean()

def rsi(series: pd.Series, period: int = 14) -> pd.Series:
    delta = series.diff()
    up = delta.clip(lower=0).rolling(period).mean()
    down = -delta.clip(upper=0).rolling(period).mean()
    rs = up / (down + 1e-12)
    return 100 - (100 / (1 + rs))

def macd(series: pd.Series, fast: int = 12, slow: int = 26, signal: int = 9):
    macd_line = ema(series, fast) - ema(series, slow)
    signal_line = ema(macd_line, signal)
    hist = macd_line - signal_line
    return macd_line, signal_line, hist

def bollinger_bands(series: pd.Series, window: int = 20, num_std: float = 2.0):
    ma = sma(series, window)
    std = series.rolling(window).std()
    upper = ma + num_std * std
    lower = ma - num_std * std
    return upper, ma, lower

def realized_vol(returns: pd.Series, window: int = 20) -> pd.Series:
    return returns.rolling(window).std() * np.sqrt(252)

def build_features(df: pd.DataFrame) -> pd.DataFrame:
    data = df.copy()
    if "Adj Close" not in data.columns and "Close" in data.columns:
        data["Adj Close"] = data["Close"]
    data["Return"] = data["Adj Close"].pct_change()
    data["LogReturn"] = np.log1p(data["Return"]).fillna(0.0)
    data["SMA_10"] = sma(data["Adj Close"], 10)
    data["SMA_50"] = sma(data["Adj Close"], 50)
    data["EMA_12"] = ema(data["Adj Close"], 12)
    data["EMA_26"] = ema(data["Adj Close"], 26)
    data["RSI_14"] = rsi(data["Adj Close"], 14)
    macd_line, signal_line, hist = macd(data["Adj Close"])
    data["MACD"] = macd_line
    data["MACD_Signal"] = signal_line
    data["MACD_Hist"] = hist
    up, mid, low = bollinger_bands(data["Adj Close"])
    data["BB_Upper"] = up
    data["BB_Mid"] = mid
    data["BB_Lower"] = low
    data["Vol_20"] = realized_vol(data["Return"], 20)
    for lag in [1, 2, 5, 10]:
        data[f"Ret_Lag_{lag}"] = data["Return"].shift(lag)
    data["Target_NextRet"] = data["Return"].shift(-1)
    data = data.dropna()
    return data
