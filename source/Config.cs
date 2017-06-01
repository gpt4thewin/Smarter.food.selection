using System;
using HugsLib.Settings;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Config
	{
		internal const float CONFIG_DEFAULT_COST_FACTOR = 250f;
		internal const float CONFIG_DEFAULT_ROTTING_SCORE_FACTOR = 30f;
		internal const float CONFIG_DEFAULT_NEVER_ROTS_SCORE_FACTOR = 180f;

		public static SettingHandle<bool> useCorpsesForTaming { get; set; }

		public static SettingHandle<bool> useHumanlikeCorpsesForTaming { get; set; }

		public static SettingHandle<bool> useMealsForTaming { get; set; }

		// Somehow obsolete but user friendly
		//public static SettingHandle<bool> privilegedPrisoners { get; set; }
		//public static SettingHandle<IncapFeedMode> IncapColonistsFeedMode { get; set; }
		//public static SettingHandle<bool> petsPreferHunt { get; set; }

		public static SettingHandle<bool> SeparatedNutrientPaste { get; set; }

		//TODO: make custom UI ajustment
		public static SettingHandle<float> petsSafeHuntMaxStrengthRatio { get; set; }

		//public static SettingHandle<bool> extendedFoodOptimality { get; set; }
		public static SettingHandle<float> NeedsTabUIHeight { get; set; }

		public static SettingHandle<bool> ShowAdvancedOptions { get; set; }

#if DEBUG
		public static SettingHandle<bool> debugFoodPrefConstant { get; set; }
		public static SettingHandle<bool> debugNoPawnsRestricted { get; set; }
		public static SettingHandle<bool> debugNoWasteFactor { get; set; }
#endif

		public static SettingHandle<bool> controlPets { get; set; }

		public static SettingHandle<bool> controlPrisoners { get; set; }

		public static SettingHandle<bool> controlColonists { get; set; }

		public static SettingHandle<bool> controlVisitors { get; set; }

		internal static bool ControlDisabledForPawn(Pawn eater)
		{
			if (eater.IsColonist && !controlColonists)
				return true;
			if (eater.IsPrisonerOfColony && !controlPrisoners)
				return true;
			if (eater.IsPetOfColony() && !controlPets)
				return true;
			if (eater.isFriendly() && !controlVisitors)
				return true;

			return false;
		}

		// now default true
		//public static SettingHandle<bool> ColonistsPrefPasteOverTasty { get; set; }

		public static SettingHandle<bool> PrintPreferencesCommand { get; set; }


		public static SettingHandle<bool> CostFactorMatters { get; set; }
		public static SettingHandle<float> CostFactor { get; set; }

		public static SettingHandle<float> RottingScoreFactor { get; internal set; }
		public static SettingHandle<float> UnrottableFoodScoreOffset { get; internal set; }

		public static SettingHandle<int> FoodSearchMaxItemsCount { get; internal set; }

		//public static List<SettingHandle<PolicySetting>> ActivePolicies { get; set; } = new List<SettingHandle<PolicySetting>>();
	}
}
