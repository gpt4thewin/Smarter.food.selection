using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using HugsLib.Core;
using UnityEngine;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Config
	{
		internal const float CONFIG_DEFAULT_COST_FACTOR = 250f;

		//TODO: discard obsolette settings

		public static SettingHandle<bool> useCorpsesForTaming { get; set; }

		//TODO: implement setting
		public static SettingHandle<bool> useHumanlikeCorpsesForTaming { get; set; }

		// Somehow obsolete but user friendly
		//public static SettingHandle<bool> privilegedPrisoners { get; set; }
		//public static SettingHandle<IncapFeedMode> IncapColonistsFeedMode { get; set; }
		//public static SettingHandle<bool> petsPreferHunt { get; set; }

		//TODO: make custom UI ajustment
		//TODO: implement hunting safe ratio
		public static SettingHandle<float> petsSafeHuntMaxStrengthRatio { get; set; }

		//public static SettingHandle<bool> extendedFoodOptimality { get; set; }

		public static SettingHandle<bool> ShowAdvancedOptions { get; set; }

#if DEBUG
		public static SettingHandle<bool> debugFoodPrefConstant { get; set; }
		public static SettingHandle<bool> debugNoPawnsRestricted { get; set; }
		public static SettingHandle<bool> debugNoWasteFactor { get; set; }
#endif

		public static SettingHandle<bool> controlPets { get; set; }

		public static SettingHandle<bool> controlPrisoners { get; set; }

		public static SettingHandle<bool> controlColonists { get; set; }

		// now default true
		//public static SettingHandle<bool> ColonistsPrefPasteOverTasty { get; set; }

		public static SettingHandle<bool> PrintPreferencesCommand { get; set; }

		public static SettingHandle<bool> SeparatedNutrientPaste { get; set; }

		public static SettingHandle<bool> CostFactorMatters { get; set; }
		public static SettingHandle<float> CostFactor { get; set; }

		public static SettingHandle<int> FoodSearchMaxItemsCount { get; internal set; }

		//public static List<SettingHandle<PolicySetting>> ActivePolicies { get; set; } = new List<SettingHandle<PolicySetting>>();
	}
}
