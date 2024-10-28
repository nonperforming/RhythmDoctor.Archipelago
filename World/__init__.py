# Archipelago imports
from BaseClasses import Region, Location, Item, Tutorial, ItemClassification
from worlds.AutoWorld import World, WebWorld
# Local imports

class RhythmDoctorWorld(World):
    ...

class RhythmDoctorWebWorld(WebWorld):
    rich_text_options_doc = True
    theme = "partyTime"
    bug_report_page = "https://github.com/nonperforming/RhythmDoctor.Archipelago/issues"
    tutorials = [Tutorial( # TODO
        "tutorial name",
        "description",
        "language",
        "file_name",
        "link",
        ["authors"]
    )]