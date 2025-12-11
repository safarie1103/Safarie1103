
from pathlib import Path
import pandas as pd
import yfinance as yf

def load_or_download_data(ticker: str, start: str, end: str, cache_dir: str = "data") -> pd.DataFrame:
    """Load cached OHLCV data if available; otherwise download from Yahoo Finance."""
    cache_dir_path = Path(cache_dir)
    cache_dir_path.mkdir(parents=True, exist_ok=True)
    cache_path = cache_dir_path / f"{ticker}_{start}_{end}.csv"

    if cache_path.exists():
        df = pd.read_csv(cache_path, parse_dates=["Date"], index_col="Date")
        return df

    df = yf.download(ticker, start=start, end=end)
    df = df.rename_axis("Date").reset_index().set_index("Date")
    df.to_csv(cache_path)
    return df
