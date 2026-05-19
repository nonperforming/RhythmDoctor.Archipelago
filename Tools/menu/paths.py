from pathlib import Path
from sys import platform

ROOT = Path(__file__).resolve(strict=True).parent
REPOSITORY = ROOT.parent.parent

# https://github.com/SubnauticaNitrox/Nitrox.Discovery/blob/a1459fc3057280ef7567556033cc616c7bdaf99e/Nitrox.Discovery/InstallationFinders/SteamFinder.cs:L93
match platform:
    case "win32":
        # TODO: testme
        # TODO: implement
        import winreg

        winreg.Open
        STEAM_ROOT = ""
        PLAYNITE_PATH = ""
    case "linux":
        # TODO: testme
        PLAYNITE_PATH = None
        pass
    case "darwin":
        # TODO: implement
        # as BIE is broken currently don't really need to do this yet.
        STEAM_ROOT = ""
        PLAYNITE_PATH = None
        # raise NotImplementedError("macOS not supported yet")
    case _:
        raise NotImplementedError
