from pydantic import BaseModel
from typing import Optional, Union
from datetime import datetime
from enum import Enum


class ConnectionStatus(str, Enum):
    CONNECTED = "Connected"
    DISCONNECTED = "Disconnected"
    RECONNECTING = "Reconnecting"


class TradeType(str, Enum):
    BUY = "Buy"
    SELL = "Sell"


class Timeframe(str, Enum):
    M1 = "M1"
    M5 = "M5"
    M15 = "M15"
    M30 = "M30"
    H1 = "H1"


class ApiResponse(BaseModel):
    success: bool
    request_id: str
    timestamp: datetime
    data: Optional[Union[dict, list]] = None
    errors: list[str] = []


class CandleDto(BaseModel):
    symbol: str
    timeframe: str
    open_price: float
    high_price: float
    low_price: float
    close_price: float
    volume: float
    open_time: datetime
    close_time: datetime


class TickDto(BaseModel):
    symbol: str
    bid: float
    ask: float
    spread: float
    timestamp: datetime


class AccountInfoDto(BaseModel):
    balance: float
    equity: float
    margin: float
    free_margin: float
    margin_level: float
    currency: str


class PositionDto(BaseModel):
    ticket: str
    symbol: str
    trade_type: str
    volume: float
    open_price: float
    current_price: float
    stop_loss: float
    take_profit: float
    profit: float
    open_time: datetime


class OpenTradeRequest(BaseModel):
    request_id: str
    correlation_id: str
    idempotency_key: str
    symbol: str
    trade_type: TradeType
    volume: float
    entry_price: float
    stop_loss: float
    take_profit: float
    comment: str = "ARABAZON"


class CloseTradeRequest(BaseModel):
    request_id: str
    correlation_id: str
    ticket: str


class ModifyTradeRequest(BaseModel):
    ticket: str
    stop_loss: float
    take_profit: float


class TradeResultDto(BaseModel):
    broker_ticket: str
    execution_status: str
    executed_price: float
    slippage: float
    error_message: Optional[str] = None