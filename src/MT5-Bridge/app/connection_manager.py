import MetaTrader5 as mt5
import structlog
import asyncio
from app.config import settings
from app.models import ConnectionStatus

logger = structlog.get_logger()

class MT5ConnectionManager:
    def __init__(self):
        self.status = ConnectionStatus.DISCONNECTED
        self._retry_count = 0
        self._lock = asyncio.Lock()

    async def initialize(self) -> bool:
        async with self._lock:
            return await self._connect()

    async def _connect(self) -> bool:
        try:
            logger.info("Initializing MT5 connection...")

            init_kwargs = {}
            if settings.mt5_terminal_path:
                init_kwargs["path"] = settings.mt5_terminal_path

            if not mt5.initialize(**init_kwargs):
                error = mt5.last_error()
                logger.error("MT5 initialize failed", error=error)
                self.status = ConnectionStatus.DISCONNECTED
                return False

            if settings.mt5_login:
                authorized = mt5.login(
                    login=settings.mt5_login,
                    password=settings.mt5_password,
                    server=settings.mt5_server
                )
                if not authorized:
                    error = mt5.last_error()
                    logger.error("MT5 login failed", error=error)
                    self.status = ConnectionStatus.DISCONNECTED
                    return False

            self.status = ConnectionStatus.CONNECTED
            self._retry_count = 0
            info = mt5.terminal_info()
            logger.info("MT5 connected", 
                       connected=info.connected,
                       build=info.build)
            return True

        except Exception as e:
            logger.error("MT5 connection exception", error=str(e))
            self.status = ConnectionStatus.DISCONNECTED
            return False

    async def ensure_connected(self) -> bool:
        if self.status == ConnectionStatus.CONNECTED:
            if mt5.terminal_info() is not None:
                return True

        self.status = ConnectionStatus.RECONNECTING
        logger.warning("MT5 reconnecting...")

        for attempt in range(settings.max_retries):
            delay = settings.retry_delay_seconds * (2 ** attempt)
            await asyncio.sleep(delay)
            if await self._connect():
                return True
            logger.warning("Reconnect attempt failed", attempt=attempt + 1)

        self.status = ConnectionStatus.DISCONNECTED
        logger.error("MT5 reconnection failed after all retries")
        return False

    def shutdown(self):
        mt5.shutdown()
        self.status = ConnectionStatus.DISCONNECTED
        logger.info("MT5 connection closed")

    @property
    def is_connected(self) -> bool:
        return self.status == ConnectionStatus.CONNECTED

connection_manager = MT5ConnectionManager()