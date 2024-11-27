namespace RhythmDoctor.Archipelago.World.Dictionaries;

internal static class InternalToFriendlyName
{
  internal static Dictionary<Level, LevelStage> InternalNameDictionary =
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
}
