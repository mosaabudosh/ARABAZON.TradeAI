import MetaTrader5 as mt5
import structlog
from datetime import datetime
from app.models import CandleDto, TickDto
from app.connection_manager import connection_manager

logger = structlog.get_logger()

TIMEFRAME_MAP = {
    "M1": mt5.TIMEFRAME_M1,
    "M5": mt5.TIMEFRAME_M5,
    "M15": mt5.TIMEFRAME_M15,
    "M30": mt5.TIMEFRAME_M30,
    "H1": mt5.TIMEFRAME_H1,
}

async def get_candles(symbol: str, timeframe: str, count: int = 100) -> list[CandleDto]:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    tf = TIMEFRAME_MAP.get(timeframe.upper())
    if tf is None:
        raise ValueError(f"Invalid timeframe: {timeframe}")

    rates = mt5.copy_rates_from_pos(symbol, tf, 0, count)
    if rates is None:
        error = mt5.last_error()
        logger.error("Failed to fetch candles", symbol=symbol, timeframe=timeframe, error=error)
        raise RuntimeError(f"Failed to fetch candles: {error}")

    candles = []
    for rate in rates:
        open_time = datetime.utcfromtimestamp(rate['time'])

        # Calculate close time based on timeframe
        tf_seconds = {"M1": 60, "M5": 300, "M15": 900, "M30": 1800, "H1": 3600}
        close_time = datetime.utcfromtimestamp(rate['time'] + tf_seconds.get(timeframe.upper(), 300))

        candles.append(CandleDto(
            symbol=symbol,
            timeframe=timeframe,
            open_price=float(rate['open']),
            high_price=float(rate['high']),
            low_price=float(rate['low']),
            close_price=float(rate['close']),
            volume=float(rate['tick_volume']),
            open_time=open_time,
            close_time=close_time,
        ))

    return candles

async def get_tick(symbol: str) -> TickDto:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    tick = mt5.symbol_info_tick(symbol)
    if tick is None:
        raise RuntimeError(f"Failed to get tick for {symbol}")

    return TickDto(
        symbol=symbol,
        bid=float(tick.bid),
        ask=float(tick.ask),
        spread=round(float(tick.ask - tick.bid), 5),
        timestamp=datetime.utcfromtimestamp(tick.time),
    )

async def get_symbols() -> list[str]:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    symbols = mt5.symbols_get()
    if symbols is None:
        return []
    return [s.name for s in symbols if s.visible]