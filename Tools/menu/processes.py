import subprocess
from pathlib import Path
from typing import TYPE_CHECKING

import psutil

import paths
import settings

if TYPE_CHECKING:
    from collections.abc import Iterator

    from app import RDAPMenu


# region Main
def test(app: RDAPMenu) -> Iterator[str]:
    yield from build(app)
    yield from move(app)
    yield from run_restart()


def build(app: RDAPMenu) -> Iterator[str]:
    # yield "Building..."
    try:
        # steals render
        subprocess.run(  # noqa: S603
            ["dotnet", "build", "--configuration", settings.BuildConfiguration.DEBUG_BEPINEX_5],  # noqa: S607
            shell=False,
            cwd=paths.REPOSITORY,
            check=True,
        )
        app.refresh()
        yield "[green][b]Successful build[/b][/green]"
    except subprocess.CalledProcessError:
        yield "[red][b]Failed to build[/b][/red]"


def move(app: RDAPMenu) -> Iterator[str]:
    yield "Moving plugin files..."
    # FIXME: assumes this exists and is good
    built_path = Path(
        paths.REPOSITORY, "RhythmDoctor.Archipelago", "bin", app.settings.build_configuration, "netstandard-2.1"
    )
    paths_to_move = (
        "Archipelago.MultiClient.Net.dll",
        "Assets",
        "RhythmDoctor.Archipelago.dll",
        "RhythmDoctor.Archipelago.pdb",
        "io.github.nonperforming.pulse.dll",
        "io.github.nonperforming.pulse.pdb",
    )
    built_path.move_into


def run_restart(_: RDAPMenu | None = None) -> Iterator[str]:
    # Check if Rhythm Doctor is running
    yield "Closing Rhythm Doctor..."

    # Start Rhythm Doctor
    yield "Starting Rhythm Doctor..."
    yield "2"


# endregion


# region Tools
def format(_: RDAPMenu) -> Iterator[str]:
    yield "Format is not implemented yet"


def update_stripped_dlls(_: RDAPMenu) -> Iterator[str]:
    yield "Asdf"
    yield "2"


# endregion
