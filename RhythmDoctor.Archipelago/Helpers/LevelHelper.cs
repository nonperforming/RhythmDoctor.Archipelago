namespace RhythmDoctor.Archipelago.Helpers;

internal static class LevelHelper
{
  internal static Dictionary<Level, LevelStage> InternalToFriendlyNameDictionary =
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

  internal static Dictionary<LevelStage, Area> LevelToAreaDictionary =
    new()
    {
      #region Act 1 - Main Ward
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

  /// <summary>
  /// Check if a level is a boss stage.
  /// </summary>
  /// <param name="stage">Stage to check</param>
  /// <returns>True if boss stage, otherwise false</returns>
  internal static bool IsBoss(LevelStage stage) =>
    stage == LevelStage.BattlewornInsomniac ||
    stage == LevelStage.AllTheTimes ||
    stage == LevelStage.OneShiftMore ||
    stage == LevelStage.SuperBattlewornInsomniac ||
    stage == LevelStage.DreamsDontStop;

  /// <summary>
  /// Check if a level has checkpoints
  /// </summary>
  /// <param name="stage">Stage to check</param>
  /// <returns>True if has checkpoints, otherwise false</returns>
  internal static bool HasCheckpoints(LevelStage stage) =>
    stage == LevelStage.DreamsDontStop;
}
