using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;


/*
 * That might not be the best way to have a config file...
 */

namespace WM.SmarterFoodSelection
{
	internal static class Log
	{
		internal static void Message(string text)
		{
			ModCore.Log.Message(text);
		}
		internal static void Warning(string text)
		{
			ModCore.Log.Warning(text);
		}
		internal static void Error(string text)
		{
			ModCore.Log.Error(text);
		}
	}

	public class ModCore : HugsLib.ModBase
	{
		// ducktapestan
		private static ModCore running;

		public readonly static string modname = "Smarter_Food_Selection";

		internal static List<CompabilityDef> patches = new List<CompabilityDef>();

		public override string ModIdentifier
		{
			get
			{
				return modname;
			}
		}

		internal static class Log
		{
			internal static void Message(string text)
			{
				running.Logger.Message(text);
			}
			internal static void Warning(string text)
			{
				running.Logger.Warning(text);
			}
			internal static void Error(string text)
			{
				running.Logger.Error(text);
			}
		}


		public override void Initialize()
		{
			// ducktape
			running = this;
		}
		public override void WorldLoaded()
		{
			FoodSearchCache.ClearAll();

			var obj = UtilityWorldObjectManager.GetUtilityWorldObject<WorldDataStore_PawnPolicies>();

#if DEBUG
			Log.Message("World loaded. assigned policies: " + WorldDataStore_PawnPolicies.AssignedPoliciesCount);
#endif

			//if (!SeparatedNutrientPaste)
			//	Utils.ShowRevertAllWorldNonVanillaThingsDialog();
		}

		internal enum DrawFoodSearchMode
		{
			Off = 0,
			Simple = 1,
			AdvancedForBest = 2,
			Advanced = 3
		}

		static DrawFoodSearchMode drawFoodSearchMode_int = DrawFoodSearchMode.Off;
		internal static DrawFoodSearchMode drawFoodSearchMode
		{
			get
			{
				return drawFoodSearchMode_int;
			}
			set
			{
				if (value == DrawFoodSearchMode.Off)
					DebugViewSettings.drawFoodSearchFromMouse = false;
				else
					DebugViewSettings.drawFoodSearchFromMouse = true;

				drawFoodSearchMode_int = value;
			}
		}

		public override void OnGUI()
		{
			if (KeysBinding.ToggleFoodScore.KeyDownEvent)
			{
				drawFoodSearchMode++;
				if (Convert.ToInt32(drawFoodSearchMode) >= Enum.GetValues(typeof(DrawFoodSearchMode)).Length)
					drawFoodSearchMode = 0;

#if DEBUG
				Log.Message("Display mode = " + Convert.ToInt32(drawFoodSearchMode));
#endif
			}
		}

		public override void DefsLoaded()
		{
			// ------------------ Basic options -----------------

			//Config.ColonistsPrefPasteOverTasty = Settings.GetHandle<bool>("colonistsPrefPasteOverTasty", "PrefNutrientPasteOverTasty".Translate(), "PrefNutrientPasteOverTasty_desc".Translate(), true);
			//Config.privilegedPrisoners = Settings.GetHandle<bool>("privilegedPrisoners", "PrivilegedPrisoners".Translate(), "PrivilegedPrisoners_desc".Translate(), false);

			//Config.IncapColonistsFeedMode = Settings.GetHandle("incapColonistsFeedMode", "IncapFeedMode".Translate(), "", IncapFeedMode.AnimalsLikeExcludeCorpses, null, "enumSetting_");

			//Config.petsPreferHunt = Settings.GetHandle<bool>("petsPreferHunt", "PetsPreferHunt".Translate(), "PetsPreferHunt_desc".Translate(), true);

			Config.SeparatedNutrientPaste = Settings.GetHandle<bool>("separatedNutrientPaste", "SeparatedNutrientPaste".Translate(), "SeparatedNutrientPaste_desc".Translate(), true);

			Config.PrintPreferencesCommand = Settings.GetHandle<bool>("commandPrintReport", "PrintReportCommand".Translate(), "PrintReportCommand_desc".Translate());
			Config.PrintPreferencesCommand.CustomDrawer = delegate (Rect rect)
			{
				if (Widgets.ButtonText(rect, "commandPrintReportButton".Translate()))
				{
					var floatOptions = new List<FloatMenuOption>();
					foreach (var printMode in Enum.GetValues(typeof(ReportMode)))
					{
						floatOptions.Add(new FloatMenuOption(("commandPrintReportOption_" + printMode.ToString()).Translate(), () =>
						  {
							  Report.PrintCompatibilityReport((ReportMode)printMode);
						  }));
					}
					Find.WindowStack.Add(new FloatMenu(floatOptions));
				}
				return true;
			};

			Config.controlPets = Settings.GetHandle<bool>("controlPets", "ControlPets".Translate(), "ControlPets_desc".Translate(), true);
			Config.controlPrisoners = Settings.GetHandle<bool>("controlPrisoners", "ControlPrisoners".Translate(), "ControlPrisoners_desc".Translate(), true);
			Config.controlColonists = Settings.GetHandle<bool>("controlColonists", "ControlColonists".Translate(), "ControlColonists_desc".Translate(), true);

			Config.ShowAdvancedOptions = Settings.GetHandle<bool>("showAdvancedOptions", "ShowAdvancedOptions".Translate(), "ShowAdvancedOptions_desc".Translate(), false);

			// ------------------ Advanced options -----------------
			{
				SettingHandle.ShouldDisplay VisibilityPredicate = (() => Config.ShowAdvancedOptions);

				Config.petsSafeHuntMaxStrengthRatio = Settings.GetHandle<float>("petsSafeHuntMaxStrenghRatio", "PetsSafeHuntMaxStrenghtRatio".Translate(), "PetsSafeHuntMaxStrenghtRatio_desc".Translate(), 0.25f);
				Config.petsSafeHuntMaxStrengthRatio.VisibilityPredicate = VisibilityPredicate;
				Config.petsSafeHuntMaxStrengthRatio.Validator = delegate (string value)
				{
					float ratio;

					if (float.TryParse(value, out ratio) && 0f < ratio && ratio <= 0.5f)
						return true;

					Logger.Warning("Wrong option value: " + value + " \nreseting...");

					return false;
				};

				Config.useCorpsesForTaming = Settings.GetHandle<bool>("useCorpsesForTaming", "UseCorpsesForTaming".Translate(), "UseCorpsesForTaming_desc".Translate(), false);
				Config.useCorpsesForTaming.VisibilityPredicate = VisibilityPredicate;

				Config.useHumanlikeCorpsesForTaming = Settings.GetHandle<bool>("useHumanlikeCorpsesForTaming", "UseHumanlikeCorpsesForTaming".Translate(), "UseHumanlikeCorpsesForTaming_desc".Translate(), false);
				Config.useHumanlikeCorpsesForTaming.VisibilityPredicate = VisibilityPredicate;

				//Config.extendedFoodOptimality = Settings.GetHandle<bool>("extendedFoodOptimality", "ExtendedFoodOptimality".Translate(), "ExtendedFoodOptimality_desc".Translate(), true);
				//Config.extendedFoodOptimality.VisibilityPredicate = VisibilityPredicate;

				Config.CostFactor = Settings.GetHandle<float>("CostFactor", "CostFactor".Translate(), "CostFactor_desc".Translate(), Config.CONFIG_DEFAULT_COST_FACTOR);
				Config.CostFactor.VisibilityPredicate = VisibilityPredicate;

				Config.FoodSearchMaxItemsCount = Settings.GetHandle<int>("FoodSearchMaxItemsCount", "FoodSearchMaxItemsCount".Translate(), "FoodSearchMaxItemsCount_desc".Translate(), 2000);
				Config.FoodSearchMaxItemsCount.VisibilityPredicate = VisibilityPredicate;

				Config.useMealsForTaming = Settings.GetHandle<bool>("useMealsForTaming", "UseMealsForTaming".Translate(), "UseMealsForTaming_desc".Translate(), false);
				Config.useMealsForTaming.VisibilityPredicate = VisibilityPredicate;

#if DEBUG
				Config.debugNoPawnsRestricted = Settings.GetHandle<bool>("debugNoPawnsRestricted", "debugNoPawnsRestricted", "", false);
				Config.debugFoodPrefConstant = Settings.GetHandle<bool>("debugFoodPrefConstant", "debugFoodPrefConstant", "", false);
				Config.debugNoWasteFactor = Settings.GetHandle<bool>("debugNoWasteFactor", "debugNoWasteFactor", "", false);
#endif
			}

			FoodCategoryUtils.ClearAllRecords();
			processDefs();

			// ducktape
			processCannibalMealDef("MealNutrientPaste");
			//processCannibalMealDef("MealSimple");
			//processCannibalMealDef("MealFine");
			//processCannibalMealDef("MealLavish");

			//#if DEBUG
			Report.PrintCompatibilityReport(ReportMode.DefName, true);
			//#endif
		}

		private void processCannibalMealDef(string prefix)
		{
			try
			{
				ThingDef.Named(prefix + "Cannibal").label = string.Format(("CannibalMealLabel".Translate()), ThingDef.Named(prefix).label);
			}
			catch (Exception ex)
			{
				Log.Warning("Could not dynamicaly name meal def label from " + prefix + ": " + ex + " " + ex.StackTrace);
			}
		}

		private void processDefs()
		{
			var modsList = new List<ModContentPack>();

			// ----------- Injecting tabs -----------

			//{
			//	var caravanDef = DefDatabase<WorldObjectDef>.GetNamed("Caravan");
			//	var newTabType = typeof(UI.WITab_Caravan_Needs);
			//	var oldTabType = typeof(ITab_Pawn_Needs);
			//	if (caravanDef.inspectorTabs.Remove(oldTabType))
			//	{
			//		caravanDef.inspectorTabsResolved.Remove(InspectTabManager.GetSharedInstance(oldTabType));
			//		caravanDef.inspectorTabs.Add(newTabType);
			//		caravanDef.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(newTabType));
			//	}
			//	else
			//	{
			//		//TODO: disaster error message
			//	}
			//}

			foreach (var current in DefDatabase<ThingDef>.AllDefs.Where((ThingDef arg) => arg.race != null))
			{
				//Tab inject
				var newTabType = typeof(UI.ITab_Pawn_Needs);
				var oldTabType = typeof(ITab_Pawn_Needs);

				if (current.inspectorTabs.Remove(oldTabType))
				{
					current.inspectorTabsResolved.Remove(InspectTabManager.GetSharedInstance(oldTabType));
					current.inspectorTabs.Add(newTabType);
					current.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(newTabType));
				}
			}

			// ----------- Processing policies and patches -----------

			foreach (CompabilityDef def in ModCore.patches)
			{
				def.TryApplyPatch();
			}

			// Hardcoded policies
			DefDatabase<Policy>.Add(Policies.Unrestricted);
			DefDatabase<Policy>.Add(Policies.Taming);
			DefDatabase<Policy>.Add(Policies.FriendlyPets);
			DefDatabase<Policy>.Add(Policies.Friendly);
			//Policies.FriendlyPets.DefsLoaded();

			foreach (MyDefClass current in Policies.AllPolicies)
			{
				current.DefsLoaded();
			}

			Policies.BuildMaskTree();

			// ----------- Processing food categories and races -----------

			foreach (ThingDef current in DefDatabaseHelper.AllDefsIngestibleNAnimals)
			{
				if (current.ingestible != null)
				{
					current.DetermineFoodCategory(true);
				}
				else if (current.race != null && current.race.IsFlesh)
				{
					if (current.race.meatDef != null)
					{
						current.race.meatDef.DetermineFoodCategory(true);
					}

					if (current.race.corpseDef != null)
					{
						current.race.corpseDef.DetermineFoodCategory(true);
					}
				}
			}

			// ----------- Processing recipes for food cost factors -----------

			//TODO: manage intermediate recipes + handle side products
			foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs)
			{
				if (recipeDef.products == null || recipeDef.products.Count > 1 || recipeDef.ingredients == null || !recipeDef.products.Any((obj) => obj.thingDef.ingestible != null))
					continue;

				float nutritionCost = recipeDef.ingredients.Sum((arg) => arg.GetBaseCount());
				var singleProduct = recipeDef.products.FirstOrDefault().thingDef;

				if (!recipeDef.products.All((arg) => arg.thingDef == singleProduct))
					continue;

				float nutritionOutput = recipeDef.products.Sum((arg) => arg.thingDef.ingestible.nutrition * arg.count);

				if (nutritionOutput <= 0f)
					continue;

				float costFactor = nutritionCost / nutritionOutput;

				FoodDefRecord record;

				if (FoodCategoryUtils.FoodRecords.TryGetValue(singleProduct, out record))
				{
					record.costFactor = Math.Min(record.costFactor, costFactor);
				}
			}

			// Dispenser(s)
			//TODO: make more reliable
			{
				FoodDefRecord record;
				var dispensableDef = ThingDefOf.MealNutrientPaste;
				if (FoodCategoryUtils.FoodRecords.TryGetValue(dispensableDef, out record))
				{
					float costFactor = 0.3f / dispensableDef.ingestible.nutrition;
					record.costFactor = Math.Min(record.costFactor, costFactor);
				}
			}


			//foreach (ThingDef dispenser in DefDatabase<ThingDef>.AllDefs.Where(arg => arg.IsFoodDispenser))

			//ThingDef meal = null;
			//float foodCost = 0;

			//if (dispenser.thingClass == typeof(Building_NutrientPasteDispenser))
			//{
			//	var dummy = new Building_NutrientPasteDispenser();
			//	meal = dummy.DispensableDef;
			//	foodCost = (dummy.def.building.foodCostPerDispense * 0.05f);
			//}

			//if (meal != null)
			//{
			//	float costFactor = foodCost / meal.ingestible.nutrition;
			//	FoodDefRecord record;
			//	if (FoodCategoryUtils.FoodRecords.TryGetValue(meal, out record))
			//	{
			//		record.costFactor = Math.Min(record.costFactor, costFactor);
			//	}
			//}
			//}

			// ----------- Reporting -----------

			var loadedPaches = patches.Where((arg) => arg.Loaded);

			Logger.Message(string.Format("Loaded - {0} food categorized - {1} pawn diets - {2} compatibility patches ({3} fixes) - {4} policies.", FoodCategoryUtils.TotalFoodPrefsCount, FoodCategoryUtils.TotalAnimalsDietsCount, loadedPaches.Count(), loadedPaches.Sum((arg) => arg.DefsCount), Policies.AllPolicies.Count()));

			if (FoodCategoryUtils.NullPrefFoodsCount > 0)
			{
				Logger.Warning(string.Format("Could not determine food category for {0} Defs. Restricted pawns will ignore them. Are you using unsupported food mods ? ({1})", FoodCategoryUtils.NullPrefFoodsCount, string.Join(" ; ", FoodCategoryUtils.NullPrefFoods.Select((ThingDef arg) => arg.defName + " (" + arg.label + ")").ToArray())));
			}
		}
	}

}

