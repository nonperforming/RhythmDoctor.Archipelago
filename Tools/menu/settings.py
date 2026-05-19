from dataclasses import dataclass
from enum import StrEnum
from json import dumps, loads
from typing import Any, Self


class BuildConfiguration(StrEnum):
    DEBUG_BEPINEX_5 = "Debug-BepInEx5"
    RELEASE_BEPINEX_5 = "Release-BepInEx5"
    DEBUG_BEPINEX_6 = "Debug-BepInEx6"
    RELEASE_BEPINEX_6 = "Release-BepInEx6"


class SettingsKey(StrEnum):
    BUILD_CONFIGURATION = "build_configuration"
    STEAM_OVERRIDE_PATH = "steam_override_path"
    PLAYNITE_OVERRIDE_PATH = "playnite_override_path"


@dataclass
class Settings:
    build_configuration: BuildConfiguration = BuildConfiguration.DEBUG_BEPINEX_5

    steam_override_path: str | None = None
    playnite_override_path: str | None = None

    def serialize_to_json(self) -> str:
        return dumps(self.__dict__, indent=None, separators=(",", ":"))

    @classmethod
    def deserialize_from_dict(cls, data: dict[str, Any]) -> Self:
        return cls(
            data[SettingsKey.BUILD_CONFIGURATION],
            data[SettingsKey.STEAM_OVERRIDE_PATH],
            data[SettingsKey.PLAYNITE_OVERRIDE_PATH],
        )

    @classmethod
    def deserialize_from_json(cls, data: str) -> Self:
        return cls.deserialize_from_dict(loads(data))
