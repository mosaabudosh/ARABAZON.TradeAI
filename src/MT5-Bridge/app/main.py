import uuid
import structlog
from contextlib import asynccontextmanager
from datetime import datetime
from fastapi import FastAPI, HTTPException, Query
from app.connection_manager import connection_manager
from app.models import (
    OpenTradeRequest, CloseTradeRequest, ModifyTradeRequest
)
from app import market_data, execution

logger = structlog.get_logger()

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting MT5 Bridge...")
    connected = await connection_manager.initialize()
    if not connected:
        logger.warning("MT5 started without connection - will retry on demand")
    yield
    logger.info("Shutting down MT5 Bridge...")
    connection_manager.shutdown()

app = FastAPI(title="ARABAZON MT5 Bridge", version="1.0.0", lifespan=lifespan)


def ok(data) -> dict:
    return {
        "success": True,
        "request_id": str(uuid.uuid4()),
        "timestamp": datetime.utcnow().isoformat(),
        "data": data,
        "errors": []
    }

def err(message: str) -> dict:
    return {
        "success": False,
        "request_id": str(uuid.uuid4()),
        "timestamp": datetime.utcnow().isoformat(),
        "data": None,
        "errors": [message]
    }

# ?? Health ??????????????????????????????????????????????
@app.get("/api/v1/mt5/health")
async def health():
    return ok({
        "connection_status": connection_manager.status,
        "is_connected": connection_manager.is_connected,
    })

# ?? Market Data ?????????????????????????????????????????
@app.get("/api/v1/mt5/symbols")
async def get_symbols():
    try:
        symbols = await market_data.get_symbols()
        return ok(symbols)
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

@app.get("/api/v1/mt5/candles")
async def get_candles(
    symbol: str = Query(...),
    timeframe: str = Query("M5"),
    count: int = Query(100, ge=1, le=5000)
):
    try:
        candles = await market_data.get_candles(symbol, timeframe, count)
        return ok([c.model_dump() for c in candles])
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

@app.get("/api/v1/mt5/tick")
async def get_tick(symbol: str = Query(...)):
    try:
        tick = await market_data.get_tick(symbol)
        return ok(tick.model_dump())
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

@app.get("/api/v1/mt5/account-info")
async def get_account_info():
    try:
        info = await execution.get_account_info()
        return ok(info)
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

@app.get("/api/v1/mt5/positions")
async def get_positions():
    try:
        positions = await execution.get_positions()
        return ok(positions)
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

# ?? Trading ?????????????????????????????????????????????
@app.post("/api/v1/mt5/trade/open")
async def open_trade(request: OpenTradeRequest):
    try:
        result = await execution.open_trade(request)
        return ok(result.model_dump())
    except Exception as e:
        logger.error("Open trade error", error=str(e))
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/v1/mt5/trade/close")
async def close_trade(request: CloseTradeRequest):
    try:
        result = await execution.close_trade(request)
        return ok(result.model_dump())
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/v1/mt5/trade/modify")
async def modify_trade(request: ModifyTradeRequest):
    try:
        success = await execution.modify_trade(request)
        return ok({"modified": success})
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/v1/mt5/sync")
async def sync_positions():
    try:
        positions = await execution.get_positions()
        return ok({"synced_count": len(positions), "positions": positions})
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))