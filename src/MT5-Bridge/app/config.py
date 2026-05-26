from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    mt5_login: int = 0
    mt5_password: str = ""
    mt5_server: str = ""
    mt5_terminal_path: str = ""
    api_host: str = "0.0.0.0"
    api_port: int = 8001
    max_retries: int = 3
    retry_delay_seconds: float = 2.0
    heartbeat_interval_seconds: float = 5.0

    class Config:
        env_file = ".env"

settings = Settings()