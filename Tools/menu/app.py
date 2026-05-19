"""Rhythm Doctor Archipelago Developer Menu."""

from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING, ClassVar

import textual.containers as container
import textual.widgets as widget
from textual import app

import paths
import processes as proc
from settings import Settings

if TYPE_CHECKING:
    from collections.abc import Callable, Iterator


SETTINGS_FILENAME = "settings.json"
RHYTHM_DOCTOR_APPID = 774181
"""Rhythm Doctor's Steam AppID."""


@dataclass
class Process:
    id: str
    function: Callable[[RDAPMenu], Iterator[str]]


class Classes:
    """CSS classes."""

    class Navbar:
        BUTTON = "navbar-button"

    MENU_CATEGORY = "menu-category"


class Processes:
    """CSS IDs and associated functions."""

    class Main:
        TEST = Process("test", proc.test)
        BUILD = Process("build", proc.build)
        RUN_RESTART = Process("run-restart", proc.run_restart)

    class Tools:
        FORMAT = Process("format", proc.format)
        UPDATE_STRIPPED_DLLS = Process("update-stripped-dlls", proc.update_stripped_dlls)

    ID_TO_FUNCTION: ClassVar[dict[str, Callable[[RDAPMenu], Iterator[str]]]] = {
        Main.TEST.id: Main.TEST.function,
        Main.BUILD.id: Main.BUILD.function,
        Main.RUN_RESTART.id: Main.RUN_RESTART.function,
        Tools.FORMAT.id: Tools.FORMAT.function,
        Tools.UPDATE_STRIPPED_DLLS.id: Tools.UPDATE_STRIPPED_DLLS.function,
    }

    @classmethod
    def run_process(cls, button_id: str, app: RDAPMenu) -> Iterator[str]:
        yield from cls.ID_TO_FUNCTION[button_id](app)


class RDAPMenu(app.App):
    """Rhythm Doctor Archipelago Developer Menu."""

    CSS_PATH = "app.tcss"

    settings: Settings

    def compose(self) -> app.ComposeResult:
        yield widget.Header(True)

        with container.Horizontal(id="navbar"):
            yield widget.Button("Main", id="main", classes=Classes.Navbar.BUTTON)
            yield widget.Button("Tools", id="tools", classes=Classes.Navbar.BUTTON)
            yield widget.Button("Log", id="log", classes=Classes.Navbar.BUTTON)
            yield widget.Button("Settings", id="settings", classes=Classes.Navbar.BUTTON)

        with widget.ContentSwitcher(initial="main"):
            with container.Vertical(id="main", classes=Classes.MENU_CATEGORY):
                yield widget.Button("Build and Run/Restart", id=Processes.Main.TEST.id)
                yield widget.Button("Build", id=Processes.Main.BUILD.id)
                yield widget.Button("Run/Restart", id=Processes.Main.RUN_RESTART.id)
            with container.Vertical(id="tools", classes=Classes.MENU_CATEGORY):
                yield widget.Button("Format", id=Processes.Tools.FORMAT.id)
                yield widget.Button("Update stripped DLLs", id=Processes.Tools.UPDATE_STRIPPED_DLLS.id)
            with container.Vertical(id="log", classes=Classes.MENU_CATEGORY):
                yield widget.TextArea()
                yield widget.Button("Clear", id="clear")
            with container.Vertical(id="settings", classes=Classes.MENU_CATEGORY):
                yield widget.Button("Clear")

        # TODO: show status here
        # a label only shows for one paint, then disappears
        yield widget.Footer()

    def on_mount(self) -> None:
        self.theme = "catppuccin-mocha"
        self.title = "Rhythm Doctor Archipelago Developer Menu"
        self.sub_title = "Version ?"

    def on_button_pressed(self, event: widget.Button.Pressed) -> None:
        if Classes.Navbar.BUTTON in event.button.classes:
            # navbar button
            self.query_one(widget.ContentSwitcher).current = event.button.id
        else:
            # menu item
            [self.notify(msg) for msg in Processes.run_process(event.button.id, self)]

    def __init__(self) -> None:
        super().__init__()
        # get settings file if it exists
        try:
            with Path(paths.ROOT, SETTINGS_FILENAME).open("rt") as file:
                self.settings = Settings.deserialize_from_json(file.read())
        except:
            # no file, or it is corrupted
            with Path(paths.ROOT, SETTINGS_FILENAME).open("wt+") as file:
                self.settings = Settings()
                file.write(self.settings.serialize_to_json())


if __name__ == "__main__":
    RDAPMenu().run()
