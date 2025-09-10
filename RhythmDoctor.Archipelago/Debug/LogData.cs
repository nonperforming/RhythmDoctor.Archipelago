#if DEBUG
using RDLevelEditor;
using System.Text;

namespace RhythmDoctor.Archipelago.Debug;

internal static class LogData
{
  internal static void Level(RDLevelData data)
  {
    StringBuilder sb = new("----- Loaded Level Data\n");
    sb.AppendLine("--- Settings")
      .AppendLine($"Version: {data.settings.version}")
      .AppendLine($"Artist: {data.settings.artist}")
      .AppendLine($"Song: {data.settings.song}")
      .AppendLine($"Special Artist Type: {data.settings.specialArtistType}")
      .AppendLine($"Artist Permission File Name: {data.settings.artistPermissionFileName}")
      .AppendLine($"Artist Links: {data.settings.artistLinks}")
      .AppendLine($"Author: {data.settings.author}")
      .AppendLine($"Difficulty: {data.settings.difficulty}")
      .AppendLine($"Seizure Warning: {data.settings.seizureWarning}")
      .AppendLine($"Preview Image Name: {data.settings.previewImageName}")
      .AppendLine($"Syringe Icon Name: {data.settings.syringeIconName}")
      .AppendLine($"Preview Song Name: {data.settings.previewSongName}")
      .AppendLine($"Preview Song Start Time: {data.settings.previewSongStartTime}")
      .AppendLine($"Preview Song Duration: {data.settings.previewSongDuration}")
      .AppendLine($"Description: {data.settings.description}")
      .AppendLine($"Tags: {data.settings.tags}")
      .AppendLine($"Song Label Hue: {data.settings.songLabelHue}")
      .AppendLine($"Song Label Grayscale: {data.settings.songLabelGrayscale}")
      .AppendLine($"Can Be Played On: {data.settings.canBePlayedOn}")
      .AppendLine($"Separate 2P Level Filename: {data.settings.separate2PLevelFilename}")
      .AppendLine($"Rank Max Mistakes: {string.Join(", ", data.settings.rankMaxMistakes ?? [])}")
      .AppendLine($"Rank Description: {string.Join(", ", data.settings.rankDescription ?? [])}")
      .AppendLine($"Custom Class: {data.settings.customClass}")
      .AppendLine($"Multiplayer Appearance: {data.settings.multiplayerAppearance}")
      .AppendLine($"First Beat Behavior: {data.settings.firstBeatBehavior}")
      .AppendLine($"Mods: {string.Join(", ", data.settings.mods ?? [])}")
      .AppendLine($"Ink File: {data.settings.inkFile}")
      .AppendLine($"Level Volume: {data.settings.levelVolume}")
      .AppendLine($"Create Rows Manually: {data.settings.createRowsManually}")
      .AppendLine($"Uses Window Dance: {data.settings.usesWindowDance}")
      .AppendLine($"Resizes Window: {data.settings.resizesWindow}")
      .AppendLine($"Last Modified Time: {data.settings.lastModifiedTime}")
      .AppendLine($"First Song File Name: {data.settings.firstSongFileName}")
      .AppendLine($"First Song Offset: {data.settings.firstSongoffset}")
      .AppendLine($"BPM: {data.settings.bpm}")
      .AppendLine($"Main RD Level Relative Path: {data.settings.mainRDLevelRelativePath}")
      .AppendLine();

    sb.AppendLine("--- Rows");
    foreach (LevelEvent_MakeRow row in data.rows)
    {
      sb.AppendLine("-")
        .AppendLine($"Pulse Sound: {row.pulseSound.filename}")
        .AppendLine($"Mimics Row: {row.mimicsRow}")
        .AppendLine($"Custom Character Name: {row.customCharacterName}")
        .AppendLine($"Failed Loading Custom Character: {row.failedLoadingCustomCharacter}")
        .AppendLine($"Row Type: {row.rowType}")
        .AppendLine($"Player: {row.player}")
        .AppendLine($"Character: {row.character}")
        .AppendLine($"CPU Marker: {row.cpuMarker}")
        .AppendLine($"Hide At Start: {row.hideAtStart}")
        .AppendLine($"Row To Mimic: {row.rowToMimic}")
        .AppendLine($"Mute Beats: {row.muteBeats}")
        .AppendLine($"Mute In 1P: {row.muteIn1P}");
    }
    sb.AppendLine();

    sb.AppendLine("--- Sprites");
    foreach (LevelEvent_MakeSprite sprite in data.sprites)
    {
      sb.AppendLine("-")
        .AppendLine($"Failed Loading Custom Character: {sprite.failedLoadingCustomCharacter}")
        .AppendLine($"Visible: {sprite.visible}")
        .AppendLine($"ID: {sprite.spriteId}")
        .AppendLine($"Filename: {sprite.filename}")
        .AppendLine($"Depth: {sprite.depth}")
        .AppendLine($"Texture Filtering: {sprite.filter}");
    }
    sb.AppendLine();

    sb.AppendLine("--- Level Events");
    foreach (LevelEvent_Base levelEvent in data.levelEvents)
    {
      sb.AppendLine($"- {levelEvent.name}").AppendLine($"Bar: {levelEvent.bar}").AppendLine($"Beat: {levelEvent.beat}");

      // In alphabetical order...
      switch (levelEvent)
      {
        case LevelEvent_CallCustomMethod callCustomMethodEvent:
          // FIXME: Broken for some reason. Throws NRE
          // sb.AppendLine()
          //  .AppendLine($"Method: {callCustomMethodEvent.method.Name}")
          //  .AppendLine($"Field: {callCustomMethodEvent.field.Name}")
          //  .AppendLine($"Arguments: {string.Join(" ,", callCustomMethodEvent.argString ?? [])}");
          break;
        case LevelEvent_ChangeCharacter changeCharacterEvent:
          sb.AppendLine()
            .AppendLine($"Change To Character: {changeCharacterEvent.character}")
            .AppendLine($"Transition type: {changeCharacterEvent.transition}");
          break;
        case LevelEvent_SetBeatSound setBeatSoundEvent:
          sb.AppendLine().AppendLine($"Sound: {setBeatSoundEvent.sound.LocalizedName()}");
          break;
        case LevelEvent_SetClapSounds setClapSoundsEvent:
          sb.AppendLine()
            .AppendLine($"Row Type: {setClapSoundsEvent.rowType}")
            .AppendLine($"P1 Sound: {setClapSoundsEvent.p1Sound?.filename}")
            .AppendLine($"P2 Sound: {setClapSoundsEvent.p2Sound?.filename}")
            .AppendLine($"CPU Sound: {setClapSoundsEvent.cpuSound?.filename}");
          break;
      }
    }
    sb.AppendLine();

    sb.AppendLine("--- Conditionals");
    foreach (Conditional conditional in data.conditionals)
    {
      sb.AppendLine("-")
        .AppendLine($"Tag: {conditional.tag}")
        .AppendLine($"Description: {conditional.description}")
        .AppendLine($"ID: {conditional.id}")
        .AppendLine($"Type: {conditional.type}")
        .AppendLine($"Info: {conditional.info.name}");
    }
    sb.AppendLine();

    sb.AppendLine("--- Bookmarks");
    foreach (BookmarkData bookmark in data.bookmarks)
    {
      sb.AppendLine("-")
        .AppendLine($"Bar: {bookmark.barAndBeat.bar}")
        .AppendLine($"Beat: {bookmark.barAndBeat.beat}")
        .AppendLine($"Color Index: {bookmark.colorIndex}");
    }
    sb.AppendLine();

    sb.AppendLine("--- Color Palette").AppendLine(string.Join('\n', data.colorPalette));
    sb.AppendLine("-----");

    Plugin.Logger.LogDebug(sb.ToString());
  }
}
#endif
