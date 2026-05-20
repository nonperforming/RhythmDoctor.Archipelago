namespace RhythmDoctor.Archipelago.Client.Components;

internal sealed class DeathLinkComponent
{
  private readonly ArchipelagoSession Session;
  private DeathLinkService Service = null!;

  private CancellationTokenSource _cancellationTokenSource;

  // TODO: would be nice to localize these.
  private static readonly string[] Messages =
  [
    " couldn't defibrillate well enough",
    " was defeated by Connectifia abortus",
    " couldn't keep the beat",
    " had to go back to med school",
    " lost their ranked match",
    "'s been waiting for so long",
    " woof woof woof woof woof woof woof", // has been waiting for so long
    " is living with regrets",
    "'s dreams stopped",
    " played Falcon",
    " couldn't jump over the box of beans",
    " hit that \"Don't Save Changes\" again",
    " wishes they could write more, and care less",
    " lost connection",
    "'s the one that needs some help",
    " was fired",
    " wasn't ready yet!", // Beans Hopper achievement
    " couldn't count to 7",
  ];

  internal DeathLinkComponent(ArchipelagoSession session, CancellationTokenSource cancellationTokenSource)
  {
    Session = session;
    _cancellationTokenSource = cancellationTokenSource;
  }

  internal async Task Enable()
  {
    Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Enabling DeathLink...");
    await Task.Run(() =>
    {
      Service = Session.CreateDeathLinkService();
      Service.EnableDeathLink();
      Service.OnDeathLinkReceived += DeathLinkReceived;
    });
    Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Enabled DeathLink!");
  }

  internal void SendDeathLink()
  {
    // ReSharper disable once NullableWarningSuppressionIsUsed
    PlayerInfo player = Session!.Players.ActivePlayer;

    string message = player.Alias + Messages[Plugin.Random.Next(Messages.Length)];

    DeathLink deathLink = new(player.Alias, message);
    SendDeathLink(deathLink);
  }

  private void SendDeathLink(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Sending death link: \"{deathLink.Cause}\"...");
    Service.SendDeathLink(deathLink);
  }

  private void DeathLinkReceived(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo(
      $"[{nameof(DeathLinkComponent)}] DeathLink from {deathLink.Source} by \"{deathLink.Cause}\" at {deathLink.Timestamp}"
    );

    if (!DeathLinkPatch.enabled)
      return;

    string text = string.IsNullOrWhiteSpace(deathLink.Cause) ? $"{deathLink.Source} died" : deathLink.Cause;

    if (scnGame.instance is not null)
    {
      if (scnGame.instance.levelIdentifier == nameof(Level.BeansHopper))
      {
        Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Running tag 'miss'");

        // While Beans Hopper does technically have hearts, they're not visible/relevant in this minigame.
        DeathLinkPatch.enabled = false;
        scnGame.instance.currentLevel.RunTag("miss");

        // FIXME: Doesn't show - gets overwritten by score text.
        //scnGame.instance.statusText.SetStatusText(text, Color.red, narrate: true);
      }
      else
      {
        // Normal/boss level.
        Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Breaking all hearts");

        scrConductor.PlayFeedback(GameSoundType.BigMistake, group: RDUtils.GetMixerGroup("MistakesParent"));
        scnGame.instance.FlashBorderFeedbackWithDuration(scnGame.BorderFeedbackType.Incorrect, 5f);
        scnGame.instance.ShakeAllHearts(duration: 1f, 8);
        scnGame.instance.ShakeAllCharacters(duration: 1f, 8);
        scnGame.instance.currentLevel.CrackAllHearts();
        scnGame.instance.mistakesManager.mistakesCountP1 += 500;
        scnGame.instance.mistakesManager.mistakesCountP2 += 500;
        DeathLinkPatch.enabled = false;

        if (scnGame.instance.currentLevel.shouldMakeHealthBar)
        {
          // TODO: If possible, use the last entity interacted with (missed/hit note)

          scnGame.instance.UpdatePlayerHealthBars();
          if (!scnGame.instance.currentLevel.noBossFail)
          {
            scnGame.instance.FailLevel(scnGame.instance.rows[0].ent);
          }
        }

        // We only show the status text after we (potentially game over) as it can overwrite its text
        scnGame.instance.statusText.SetStatusText(text, Color.red, 10f, true, true);
      }
    }
    else if (scnBase.instance is scnIanDesktop desktop)
    {
      if (desktop.state != scnIanDesktop.ComputerState.Desktop)
        return;

      switch (desktop.currentProgramIndex)
      {
        // Rhythm Stacker
        case 0:
          Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Killing stacker");
          // from AddBlock()
          desktop.stackerManager.gameoverContainer.SetActive(true);
          desktop.stackerManager.hasLost = true;
          desktop.stackerManager.hiScoreText.text = RDString
            .Get("rhythmStacker.hiScore")
            .Replace("[score]", desktop.stackerManager.highestScore.ToString(), StringComparison.Ordinal);
          RDStringToUIText.Apply(desktop.stackerManager.hiScoreText);
          // TODO: Use game over text instead of high score text
          //  - this will require a patch to reset the text after Restart().
          desktop.stackerManager.hiScoreText.text = text + "\n" + desktop.stackerManager.hiScoreText.text;
          // TODO: Maybe sync high score with DataStorage?
          desktop.stackerManager.PlaySound("sndDesktopJingleNeutral");
          break;
        // tempres
        case 1:
          Plugin.Logger.LogInfo($"[{nameof(DeathLinkComponent)}] Killing tempres");
          // TODO: Show person who killed them
          // FIXME: Technically this works but its a bit buggy, doesn't tween bars.
          //  Also doesn't account for login minigame.
          // Use a reverse transpiler to pull out the `if (freeplay)` block and invoke it here.
          desktop.tempresManager.currentGameHasFinished = true;
          break;
      }
    }
  }
}
