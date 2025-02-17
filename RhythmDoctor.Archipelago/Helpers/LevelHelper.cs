namespace RhythmDoctor.Archipelago.Helpers;

internal static class LevelHelper
{
  internal static readonly Dictionary<Level, LevelStage> InternalToFriendlyNameDictionary =
    new()
    {
      #region Act 1 - Main Ward
      { Level.OrientalTechno, LevelStage.SamuraiTechno },
      { Level.OrientalDubstep, LevelStage.SamuraiDubstep },
      { Level.Intimate, LevelStage.Intimate },
      { Level.IntimateH, LevelStage.IntimateNight },
      { Level.OrientalInsomniac, LevelStage.BattlewornInsomniac },
      { Level.GongXi, LevelStage.ChineseNewYear },
      { Level.Halloween, LevelStage.ThemeOfReallySpookyBird },
      #endregion

      #region Act 2 - SVT Ward
      { Level.Lofi, LevelStage.LofiHipHopBeatsToTreatPatientsTo },
      { Level.CareLess, LevelStage.WishICouldCareLess },
      { Level.SVT, LevelStage.SupraventricularTachycardia },
      { Level.Unreachable, LevelStage.Unreachable },
      { Level.Smokin, LevelStage.PuffPiece },
      { Level.Pomeranian, LevelStage.BombSniffingPomeranian },
      { Level.SongOfTheSea, LevelStage.SongOfTheSea },
      { Level.SongOfTheSeaH, LevelStage.SongOfTheSeaNight },
      { Level.Boss2, LevelStage.AllTheTimes },
      #endregion

      #region Act 3 - Main Ward
      { Level.Garden, LevelStage.SleepyGarden },
      { Level.Lounge, LevelStage.Lounge },
      { Level.Classy, LevelStage.Classy },
      { Level.ClassyH, LevelStage.ClassyNight },
      { Level.DistantDuet, LevelStage.DistantDuet },
      { Level.DistantDuetH, LevelStage.DistantDuetNight },
      { Level.Lesmis, LevelStage.OneShiftMore },
      #endregion

      #region Act 4 - Train
      { Level.Heldbeats, LevelStage.TrainingDoctorsTrainRidePerformance },
      { Level.Rollerdisco, LevelStage.RollerdiscoRumble },
      { Level.Invisible, LevelStage.Invisible },
      { Level.InvisibleH, LevelStage.InvisibleNight },
      { Level.Steinway, LevelStage.Steinway },
      { Level.SteinwayH, LevelStage.SteinwayReprise },
      { Level.KnowYou, LevelStage.KnowYou },
      { Level.Murmurs, LevelStage.Murmurs },
      { Level.InsomniacHard, LevelStage.SuperBattlewornInsomniac },
      #endregion

      #region Act 5 - Physiotherapy Ward
      { Level.LuckyBreak, LevelStage.LuckyBreak },
      { Level.Injury, LevelStage.OneSlipTooLate },
      { Level.Freezeshot, LevelStage.LofiBeatsForPatientsToChillTo },
      { Level.FreezeshotH, LevelStage.UnsustainableInconsolable },
      { Level.AthleteTherapy, LevelStage.SeventhInningStretch },
      { Level.RhythmWeightlifter, LevelStage.RhythmWeightlifter },
      { Level.AthleteFinale, LevelStage.DreamsDontStop },
      #endregion

      #region Bonus - Basement
      { Level.VividStasis, LevelStage.FixationsTowardTheStars },
      { Level.SparkLine, LevelStage.KingdomOfBalloons },
      { Level.Unbeatable, LevelStage.WornOutTapes },
      { Level.MeetAndTweet, LevelStage.MeetAndTweet },
      { Level.BlackestLuxuryCar, LevelStage.BlackestLuxuryCar },
      { Level.TapeStopNight, LevelStage.TapeStopNight },
      { Level.The90sDecision, LevelStage.The90sDecision },
      { Level.ArtExercise, LevelStage.ArtExercise },
      #endregion

      #region Art Room
      { Level.HelpingHands, LevelStage.HelpingHands },
      #endregion
    };

  internal static readonly Dictionary<LevelStage, Level> FriendlyToInternalNameDictionary =
    new()
    {
      #region Act 1 - Main Ward
      { LevelStage.SamuraiTechno, Level.OrientalTechno },
      { LevelStage.SamuraiDubstep, Level.OrientalDubstep },
      { LevelStage.Intimate, Level.Intimate },
      { LevelStage.IntimateNight, Level.IntimateH },
      { LevelStage.BattlewornInsomniac, Level.OrientalInsomniac },
      { LevelStage.ChineseNewYear, Level.GongXi },
      { LevelStage.ThemeOfReallySpookyBird, Level.Halloween },
      #endregion

      #region Act 2 - SVT Ward
      { LevelStage.LofiHipHopBeatsToTreatPatientsTo, Level.Lofi },
      { LevelStage.WishICouldCareLess, Level.CareLess },
      { LevelStage.SupraventricularTachycardia, Level.SVT },
      { LevelStage.Unreachable, Level.Unreachable },
      { LevelStage.PuffPiece, Level.Smokin },
      { LevelStage.BombSniffingPomeranian, Level.Pomeranian },
      { LevelStage.SongOfTheSea, Level.SongOfTheSea },
      { LevelStage.SongOfTheSeaNight, Level.SongOfTheSeaH },
      { LevelStage.AllTheTimes, Level.Boss2 },
      #endregion

      #region Act 3 - Main Ward
      { LevelStage.SleepyGarden, Level.Garden },
      { LevelStage.Lounge, Level.Lounge },
      { LevelStage.Classy, Level.Classy },
      { LevelStage.ClassyNight, Level.ClassyH },
      { LevelStage.DistantDuet, Level.DistantDuet },
      { LevelStage.DistantDuetNight, Level.DistantDuetH },
      { LevelStage.OneShiftMore, Level.Lesmis },
      #endregion

      #region Act 4 - Train
      { LevelStage.TrainingDoctorsTrainRidePerformance, Level.Heldbeats },
      { LevelStage.RollerdiscoRumble, Level.Rollerdisco },
      { LevelStage.Invisible, Level.Invisible },
      { LevelStage.InvisibleNight, Level.InvisibleH },
      { LevelStage.Steinway, Level.Steinway },
      { LevelStage.SteinwayReprise, Level.SteinwayH },
      { LevelStage.KnowYou, Level.KnowYou },
      { LevelStage.Murmurs, Level.Murmurs },
      { LevelStage.SuperBattlewornInsomniac, Level.InsomniacHard },
      #endregion

      #region Act 5 - Physiotherapy Ward
      { LevelStage.LuckyBreak, Level.LuckyBreak },
      { LevelStage.OneSlipTooLate, Level.Injury },
      { LevelStage.LofiBeatsForPatientsToChillTo, Level.Freezeshot },
      { LevelStage.UnsustainableInconsolable, Level.FreezeshotH },
      { LevelStage.SeventhInningStretch, Level.AthleteTherapy },
      { LevelStage.RhythmWeightlifter, Level.RhythmWeightlifter },
      { LevelStage.DreamsDontStop, Level.AthleteFinale },
      #endregion

      #region Bonus - Basement
      { LevelStage.FixationsTowardTheStars, Level.VividStasis },
      { LevelStage.KingdomOfBalloons, Level.SparkLine },
      { LevelStage.WornOutTapes, Level.Unbeatable },
      { LevelStage.MeetAndTweet, Level.MeetAndTweet },
      { LevelStage.BlackestLuxuryCar, Level.BlackestLuxuryCar },
      { LevelStage.TapeStopNight, Level.TapeStopNight },
      { LevelStage.The90sDecision, Level.The90sDecision },
      { LevelStage.ArtExercise, Level.ArtExercise },
      #endregion

      #region Art Room
      { LevelStage.HelpingHands, Level.HelpingHands },
      #endregion
    };

  internal static readonly Dictionary<LevelStage, Area> LevelToActDictionary =
    new()
    {
      #region Main Ward
      { LevelStage.SamuraiTechno, Area.Act1 },
      { LevelStage.SamuraiDubstep, Area.Act1 },
      { LevelStage.Intimate, Area.Act1 },
      { LevelStage.IntimateNight, Area.Act1 },
      { LevelStage.BattlewornInsomniac, Area.Act1 },
      { LevelStage.ChineseNewYear, Area.Act1 },
      { LevelStage.ThemeOfReallySpookyBird, Area.Act1 },
      #endregion

      #region Act 2 - SVT Ward
      { LevelStage.LofiHipHopBeatsToTreatPatientsTo, Area.Act2 },
      { LevelStage.WishICouldCareLess, Area.Act2 },
      { LevelStage.SupraventricularTachycardia, Area.Act2 },
      { LevelStage.Unreachable, Area.Act2 },
      { LevelStage.PuffPiece, Area.Act2 },
      { LevelStage.BombSniffingPomeranian, Area.Act2 },
      { LevelStage.SongOfTheSea, Area.Act2 },
      { LevelStage.SongOfTheSeaNight, Area.Act2 },
      { LevelStage.AllTheTimes, Area.Act2 },
      #endregion

      #region Act 3 - Main Ward
      { LevelStage.SleepyGarden, Area.Act3 },
      { LevelStage.Lounge, Area.Act3 },
      { LevelStage.Classy, Area.Act3 },
      { LevelStage.ClassyNight, Area.Act3 },
      { LevelStage.DistantDuet, Area.Act3 },
      { LevelStage.DistantDuetNight, Area.Act3 },
      { LevelStage.OneShiftMore, Area.Act3 },
      #endregion

      #region Act 4 - Train
      { LevelStage.TrainingDoctorsTrainRidePerformance, Area.Act4 },
      { LevelStage.RollerdiscoRumble, Area.Act4 },
      { LevelStage.Invisible, Area.Act4 },
      { LevelStage.InvisibleNight, Area.Act4 },
      { LevelStage.Steinway, Area.Act4 },
      { LevelStage.SteinwayReprise, Area.Act4 },
      { LevelStage.KnowYou, Area.Act4 },
      { LevelStage.Murmurs, Area.Act4 },
      { LevelStage.SuperBattlewornInsomniac, Area.Act4 },
      #endregion

      #region Act 5 - Physiotherapy Ward
      { LevelStage.LuckyBreak, Area.Act5 },
      { LevelStage.OneSlipTooLate, Area.Act5 },
      { LevelStage.LofiBeatsForPatientsToChillTo, Area.Act5 },
      { LevelStage.UnsustainableInconsolable, Area.Act5 },
      { LevelStage.SeventhInningStretch, Area.Act5 },
      { LevelStage.RhythmWeightlifter, Area.Act5 },
      { LevelStage.DreamsDontStop, Area.Act5 },
      #endregion

      #region Bonus - Basement
      { LevelStage.FixationsTowardTheStars, Area.Basement },
      { LevelStage.KingdomOfBalloons, Area.Basement },
      { LevelStage.WornOutTapes, Area.Basement },
      { LevelStage.MeetAndTweet, Area.Basement },
      { LevelStage.BlackestLuxuryCar, Area.Basement },
      { LevelStage.TapeStopNight, Area.Basement },
      { LevelStage.The90sDecision, Area.Basement },
      { LevelStage.ArtExercise, Area.Basement },
      #endregion

      #region Art Room
      { LevelStage.HelpingHands, Area.ArtRoom },
      #endregion
    };

  internal static readonly Dictionary<Area, Region> ActToRegionDictionary =
    new()
    {
      { Area.Act1, Region.MainWard },
      { Area.Act2, Region.SVTWard },
      { Area.Act3, Region.MainWard },
      { Area.Act4, Region.Train },
      { Area.Act5, Region.PhysiotherapyWard },
    };

  /// <summary>
  /// Check if a level is a boss stage.
  /// </summary>
  /// <param name="stage">Stage to check</param>
  /// <returns>True if the stage is a boss stage, otherwise false</returns>
  internal static bool IsBoss(LevelStage stage) =>
    stage == LevelStage.BattlewornInsomniac
    || stage == LevelStage.AllTheTimes
    || stage == LevelStage.OneShiftMore
    || stage == LevelStage.SuperBattlewornInsomniac
    || stage == LevelStage.DreamsDontStop;

  /// <summary>
  /// Check if a level has checkpoints
  /// </summary>
  /// <param name="stage">Stage to check</param>
  /// <returns>True if the stage has checkpoints, otherwise false</returns>
  internal static bool HasCheckpoints(LevelStage stage) => stage == LevelStage.DreamsDontStop;

  internal static void UnlockEntrance(Ward wardToUnlock)
  {
    string name;
    switch (wardToUnlock)
    {
      case Ward.SVTWard:
        name = "GoToSVTWard";
        break;
      case Ward.Train:
        name = "GoToTrain";
        break;
      case Ward.PhysiotherapyWard:
        name = "GoToAthleteWard";
        break;
      case Ward.Basement:
        name = "GoToBasement";
        break;
      case Ward.ArtRoom:
        throw new NotImplementedException();
      default:
        Plugin.Logger?.LogWarning(
          $"Trying to unlock {wardToUnlock} but it doesn't have an implementation/it is the Main Ward"
        );
        return;
    }

    scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity(name));
  }
}
