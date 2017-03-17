using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class FoodCategoryUtils
	{
		internal static Dictionary<ThingDef, FoodDefRecord> FoodRecords = new Dictionary<ThingDef, FoodDefRecord>();

		// ducktape here
		internal static bool isTaming = false;

		internal static int TotalFoodPrefsCount
		{
			get
			{
				return FoodRecords.Count();
			}
		}
		internal static int TotalAnimalsDietsCount
		{
			get
			{
				return Policies.AllPolicies.Sum((arg) => arg.RacesDietCount);
			}
		}
		internal static int IgnoredFoodPrefsCount
		{
			get
			{
				return FoodRecords.Where((arg) => arg.Value.category == FoodCategory.Ignore).Count();
			}
		}
		internal static IEnumerable<ThingDef> NullPrefFoods
		{
			get
			{
				return FoodRecords.Where((arg) => arg.Value.category == FoodCategory.Null).Select((arg) => arg.Key);
			}
		}
		internal static int NullPrefFoodsCount
		{
			get
			{
				return NullPrefFoods.Count();
			}
		}

		internal static void ClearAllRecords()
		{
			FoodRecords.Clear();
		}

		internal static IEnumerable<ThingDef> GetAllFoodsWithPref(FoodCategory category)
		{
			return FoodRecords.Where((arg) => arg.Value.category == category).Select((arg) => arg.Key);
		}
		internal static IEnumerable<ThingDef> GetAllFoodsDefMatching(Func<ThingDef, bool> validator)
		{
			return FoodRecords.Keys.Where(validator);
		}

		internal static void RecordFood(ThingDef def, FoodCategory category)
		{
			if (FoodRecords.ContainsKey(def))
			{
				//Log.Error("Tried to record a FoodPref for an already recorded Def : " + def);
				return;
			}
			FoodRecords.Add(def, new FoodDefRecord() { category = category });
		}

		internal static FoodDefRecord GetFoodDefRecord(this Thing thing)
		{
			return GetFoodDefRecord(thing.def);
		}
		internal static FoodDefRecord GetFoodDefRecord(this ThingDef def)
		{
			FoodDefRecord result;
			FoodRecords.TryGetValue(def, out result);
			return result;
		}

		internal static FoodCategory DetermineFoodCategory(this Thing thing, bool silent = false)
		{
			return DetermineFoodCategory(thing.def, silent);
		}
		internal static FoodCategory DetermineFoodCategory(this ThingDef def, bool silent = false)
		{
#if DEBUG
			if (Config.debugFoodPrefConstant)
				return FoodCategory.MealSimple;
#endif

			FoodDefRecord record;
			FoodCategory category;

			if (def == null)

				//throw new ArgumentNullException(nameof(def));
				throw new ArgumentNullException("def");

			if (FoodRecords.TryGetValue(def, out record))
			{
				return record.category;
			}
			else
			{
				try
				{
					category = _DetermineFoodCategory(def);
				}
				catch (Exception ex)
				{
					category = FoodCategory.Null;

					Log.Error("Exception when trying to determine food category for " + def + " : " + ex.Message);
				}

				if (!silent && category == FoodCategory.Null)

					Log.Warning("Could not determine food preferability for " + def + ", ignoring this ThingDef... Are you using an unsupported food mod ?");

				RecordFood(def, category);

#if DEBUG
				if (!silent)
					Log.Message("Recorded food pref: " + def + " = " + category);
#endif
			}

			return category;
		}

		private static FoodCategory _DetermineFoodCategory(ThingDef def)
		{
			if (def == ThingDefOf.NutrientPasteDispenser)
			{
				return FoodCategory.MealAwful;
			}

			if (def.race != null)
				return FoodCategory.SafeHunting;

			if (def.ingestible != null)
			{
				if (def.ingestible.nutrition <= 0f || def.IsDrug)
					return FoodCategory.Ignore;

				FoodPreferability foodPref = def.ingestible.preferability;
				FoodTypeFlags foodType = def.ingestible.foodType;

				String defName = def.defName;

				if (foodPref == FoodPreferability.MealFine)

					return FoodCategory.MealFine;

				if (foodPref == FoodPreferability.MealAwful)

					return FoodCategory.MealAwful;

				if (foodPref == FoodPreferability.MealSimple)

					return FoodCategory.MealSimple;

				if (foodPref == FoodPreferability.MealLavish)

					return FoodCategory.MealLavish;

				if ((foodType & FoodTypeFlags.Kibble) != 0)

					return FoodCategory.Kibble;

				if ((foodType & FoodTypeFlags.AnimalProduct) != 0)
				{
					if (def.GetCompProperties<CompProperties_Hatcher>() != null)

						return FoodCategory.FertEggs;

					//return WMFoodPref.Null;
				}

				if (def.ingestible.joyKind == JoyKindDefOf.Gluttonous && def.ingestible.joy >= 0.05f)

					return FoodCategory.Luxury;

				if ((foodType & FoodTypeFlags.Tree) != 0)

					return FoodCategory.Plant;

				if ((foodType & FoodTypeFlags.Plant) != 0)
				{
					if (def == ThingDefOf.Hay)

						return FoodCategory.Hay;

					//TODO: Make more reliable
					if (defName == "PlantGrass" || defName == "PlantTallGrass")

						return FoodCategory.Grass;

					return FoodCategory.Plant;
				}

				if (def.IsCorpse)
				{
					if (RimWorld.FoodUtility.IsHumanlikeMeat(def))

						return FoodCategory.HumanlikeCorpse;

					//TODO: Make more reliable
					if (def.ingestible.sourceDef.race.Animal)
					{
						if (def.FirstThingCategory == ThingCategoryDefOf.CorpsesInsect)

							return FoodCategory.InsectCorpse;

						return FoodCategory.Corpse;
					}

					if (def.ingestible.sourceDef.race.IsMechanoid)

						return FoodCategory.Ignore;
				}

				if (def.ingestible.tasteThought != null && def.ingestible.tasteThought.stages.All((ThoughtStage arg) => arg.baseMoodEffect < 0))
				{
					if (RimWorld.FoodUtility.IsHumanlikeMeat(def))

						return FoodCategory.RawHuman;

					if (def == ThingDef.Named("Megaspider_Meat"))
						//if (def.ingestible.tasteThought == ThoughtDefOf.AteInsectMeatAsIngredient)

						return FoodCategory.RawInsect;


					return FoodCategory.RawBad;
				}

				if ((def.ingestible.tasteThought == null || def.ingestible.tasteThought.stages.All((ThoughtStage arg) => arg.baseMoodEffect >= 0)))

					return FoodCategory.RawTasty;

				if ((foodType & FoodTypeFlags.AnimalProduct) != 0)

					return FoodCategory.AnimalProduct;

				if (foodPref == FoodPreferability.NeverForNutrition || def.IsDrug)

					return FoodCategory.Ignore;
			}

			// non ingestible corpse ?
			if (def.IsCorpse)
				return FoodCategory.Ignore;

			return FoodCategory.Null;
		}
	}
}
