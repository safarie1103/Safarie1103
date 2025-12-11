
import numpy as np
import pandas as pd

def long_flat_strategy(pred_next_ret: pd.Series, actual_next_ret: pd.Series, tc_bps: float = 0.0):
    """Long/flat strategy with optional transaction costs in basis points."""
    signal = (pred_next_ret > 0).astype(int)
    signal_shifted = signal.shift(1).fillna(0)
    strat_ret = signal_shifted * actual_next_ret
    position_change = (signal_shifted != signal_shifted.shift(1)).astype(int).fillna(0)
    tc = (tc_bps / 10000.0) * position_change
    strat_ret_tc = strat_ret - tc
    cum_ret = (1 + strat_ret_tc).cumprod()
    buy_hold = (1 + actual_next_ret).cumprod()
    return {
        "Signal": signal_shifted,
        "StratRet": strat_ret_tc,
        "CumRet": cum_ret,
        "BuyHold": buy_hold,
    }

def sharpe_ratio(returns: pd.Series, periods_per_year: int = 252):
    if returns.std() == 0:
        return 0.0
    return (returns.mean() / returns.std()) * np.sqrt(periods_per_year)
