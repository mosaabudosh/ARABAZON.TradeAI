import MetaTrader5 as mt5
import structlog
from app.models import OpenTradeRequest, CloseTradeRequest, ModifyTradeRequest, TradeResultDto
from app.connection_manager import connection_manager

logger = structlog.get_logger()

# In-memory idempotency registry (use Redis in production)
_executed_keys: dict[str, TradeResultDto] = {}

SLIPPAGE_LIMITS = {
    "XAUUSD": 10,  # points
    "USOIL": 5,
}

async def open_trade(request: OpenTradeRequest) -> TradeResultDto:
    # Idempotency check
    if request.idempotency_key in _executed_keys:
        logger.warning("Duplicate execution detected", idempotency_key=request.idempotency_key)
        return _executed_keys[request.idempotency_key]

    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    trade_type = mt5.ORDER_TYPE_BUY if request.trade_type == "Buy" else mt5.ORDER_TYPE_SELL
    symbol_info = mt5.symbol_info(request.symbol)

    if symbol_info is None:
        raise ValueError(f"Symbol {request.symbol} not found")

    if not symbol_info.visible:
        mt5.symbol_select(request.symbol, True)

    slippage = SLIPPAGE_LIMITS.get(request.symbol, 10)

    order_request = {
        "action": mt5.TRADE_ACTION_DEAL,
        "symbol": request.symbol,
        "volume": request.volume,
        "type": trade_type,
        "price": mt5.symbol_info_tick(request.symbol).ask if trade_type == mt5.ORDER_TYPE_BUY
                 else mt5.symbol_info_tick(request.symbol).bid,
        "sl": request.stop_loss,
        "tp": request.take_profit,
        "deviation": slippage,
        "magic": 20260101,
        "comment": request.comment,
        "type_time": mt5.ORDER_TIME_GTC,
        "type_filling": mt5.ORDER_FILLING_IOC,
    }

    logger.info("Sending trade order", request_id=request.request_id, symbol=request.symbol)
    result = mt5.order_send(order_request)

    if result is None or result.retcode != mt5.TRADE_RETCODE_DONE:
        error_msg = f"Order failed: retcode={result.retcode if result else 'None'}"
        logger.error("Trade execution failed", error=error_msg, request_id=request.request_id)
        return TradeResultDto(
            broker_ticket="",
            execution_status="Failed",
            executed_price=0,
            slippage=0,
            error_message=error_msg
        )

    actual_slippage = abs(result.price - request.entry_price)
    trade_result = TradeResultDto(
        broker_ticket=str(result.order),
        execution_status="Success",
        executed_price=float(result.price),
        slippage=round(actual_slippage, 5),
    )

    _executed_keys[request.idempotency_key] = trade_result
    logger.info("Trade executed successfully", ticket=result.order, request_id=request.request_id)
    return trade_result

async def close_trade(request: CloseTradeRequest) -> TradeResultDto:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    positions = mt5.positions_get(ticket=int(request.ticket))
    if not positions:
        raise ValueError(f"Position {request.ticket} not found")

    position = positions[0]
    close_type = mt5.ORDER_TYPE_SELL if position.type == mt5.ORDER_TYPE_BUY else mt5.ORDER_TYPE_BUY
    tick = mt5.symbol_info_tick(position.symbol)
    price = tick.bid if close_type == mt5.ORDER_TYPE_SELL else tick.ask

    order_request = {
        "action": mt5.TRADE_ACTION_DEAL,
        "symbol": position.symbol,
        "volume": position.volume,
        "type": close_type,
        "position": position.ticket,
        "price": price,
        "deviation": 10,
        "magic": 20260101,
        "comment": "ARABAZON_CLOSE",
        "type_time": mt5.ORDER_TIME_GTC,
        "type_filling": mt5.ORDER_FILLING_IOC,
    }

    result = mt5.order_send(order_request)

    if result is None or result.retcode != mt5.TRADE_RETCODE_DONE:
        error_msg = f"Close failed: retcode={result.retcode if result else 'None'}"
        logger.error("Close trade failed", error=error_msg, ticket=request.ticket)
        return TradeResultDto(
            broker_ticket=request.ticket,
            execution_status="Failed",
            executed_price=0,
            slippage=0,
            error_message=error_msg
        )

    logger.info("Trade closed successfully", ticket=request.ticket)
    return TradeResultDto(
        broker_ticket=request.ticket,
        execution_status="Success",
        executed_price=float(result.price),
        slippage=0,
    )

async def modify_trade(request: ModifyTradeRequest) -> bool:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    order_request = {
        "action": mt5.TRADE_ACTION_SLTP,
        "position": int(request.ticket),
        "sl": request.stop_loss,
        "tp": request.take_profit,
    }

    result = mt5.order_send(order_request)
    success = result is not None and result.retcode == mt5.TRADE_RETCODE_DONE

    if not success:
        logger.error("Modify trade failed", ticket=request.ticket,
                    retcode=result.retcode if result else None)

    return success

async def get_positions() -> list[dict]:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    positions = mt5.positions_get()
    if positions is None:
        return []

    return [
        {
            "ticket": str(p.ticket),
            "symbol": p.symbol,
            "trade_type": "Buy" if p.type == mt5.ORDER_TYPE_BUY else "Sell",
            "volume": float(p.volume),
            "open_price": float(p.price_open),
            "current_price": float(p.price_current),
            "stop_loss": float(p.sl),
            "take_profit": float(p.tp),
            "profit": float(p.profit),
            "open_time": p.time,
        }
        for p in positions
    ]

async def get_account_info() -> dict:
    if not await connection_manager.ensure_connected():
        raise ConnectionError("MT5 not connected")

    info = mt5.account_info()
    if info is None:
        raise RuntimeError("Failed to get account info")

    return {
        "balance": float(info.balance),
        "equity": float(info.equity),
        "margin": float(info.margin),
        "free_margin": float(info.margin_free),
        "margin_level": float(info.margin_level) if info.margin_level else 0,
        "currency": info.currency,
    }