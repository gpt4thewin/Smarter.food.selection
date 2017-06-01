using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
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
				return FoodRecords.Count((arg) => arg.Value.category == FoodCategory.Ignore);
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


		internal static FoodCategory GetFoodCategory(this Thing thing, bool silent = true)
		{
			FoodDefRecord record;

			if (FoodRecords.TryGetValue(thing.def, out record))
			{
				return record.category;
			}
			else
			{
				return FoodCategory.Null;
			}
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

		// There might be a cleaner way to do what looks like a deep neural network.
		private static FoodCategory _DetermineFoodCategory(ThingDef def)
		{
			var array = Enum.GetValues(typeof(FoodCategory));

			var CategoryScores = new Dictionary<FoodCategory, float>(array.Length);

			foreach (FoodCategory entry in array)
			{
				CategoryScores.Add(entry, 0f);
			}

			if (def == ThingDefOf.NutrientPasteDispenser)
			{
				return FoodCategory.MealAwful;
			}

			if (def.race != null)
				CategoryScores[FoodCategory.Hunt] = float.MaxValue;

			if (def.ingestible != null)
			{
				if (def.ingestible.nutrition <= 0f || def.IsDrug)
					CategoryScores[FoodCategory.Ignore] = float.MaxValue;

				FoodPreferability foodPref = def.ingestible.preferability;
				FoodTypeFlags foodType = def.ingestible.foodType;

				string defName = def.defName;

				switch (foodPref)
				{
					case FoodPreferability.MealFine:
						CategoryScores[FoodCategory.MealFine] += 100f;
						break;
					case FoodPreferability.MealAwful:
						CategoryScores[FoodCategory.MealAwful] += 100f;
						break;
					case FoodPreferability.MealSimple:
						CategoryScores[FoodCategory.MealSimple] += 100f;
						break;
					case FoodPreferability.MealLavish:
						CategoryScores[FoodCategory.MealLavish] += 100f;
						break;
				}

				if (!def.HasComp(typeof(CompRottable)) && (foodType & FoodTypeFlags.Meal) != 0)
				{
					CategoryScores[FoodCategory.MealSurvival] += 150f;
				}

				if ((foodType & FoodTypeFlags.Kibble) != 0)
					CategoryScores[FoodCategory.Kibble] += 200f;

				if ((foodType & FoodTypeFlags.AnimalProduct) != 0)
				{
					if (def.GetCompProperties<CompProperties_Hatcher>() != null)

						CategoryScores[FoodCategory.FertEggs] += 500f;

					//return WMFoodPref.Null;
				}

				if (def.ingestible.joyKind == JoyKindDefOf.Gluttonous && def.ingestible.joy >= 0.05f)

					CategoryScores[FoodCategory.Luxury] += 500f;

				if ((foodType & FoodTypeFlags.Tree) != 0)

					CategoryScores[FoodCategory.Tree] += 500f;

				if ((foodType & FoodTypeFlags.Plant) != 0)
				{
					if (def.plant == null)
					{
						CategoryScores[FoodCategory.Hay] += 100f;
					}
					else
					{
						if (def.plant.sowTags != null && def.plant.sowTags.Any())
							CategoryScores[FoodCategory.Plant] += 100f;
						else
							CategoryScores[FoodCategory.Grass] += 50f;

						if (def.plant.harvestedThingDef != null)
						{
							CategoryScores[FoodCategory.Plant] += 200f;
						}

						if (def.plant.reproduces)
							CategoryScores[FoodCategory.Grass] += 100f;
					}
				}

				if (RimWorld.FoodUtility.IsHumanlikeMeat(def))
				{
					CategoryScores[FoodCategory.RawHuman] += 50f;
					CategoryScores[FoodCategory.HumanlikeCorpse] += 50f;
				}

				if (def.IsCorpse)
				{
					CategoryScores[FoodCategory.Corpse] += 80f;
					CategoryScores[FoodCategory.HumanlikeCorpse] += 50f;
					CategoryScores[FoodCategory.InsectCorpse] += 50f;

					if (def.FirstThingCategory == ThingCategoryDefOf.CorpsesInsect)

						CategoryScores[FoodCategory.InsectCorpse] += 50f;

					if (def.ingestible.sourceDef.race.IsMechanoid)

						CategoryScores[FoodCategory.Ignore] = float.MaxValue;
				}

				if (def.ingestible.tasteThought != null && def.ingestible.tasteThought.stages.All((ThoughtStage arg) => arg.baseMoodEffect < 0))
				{
					//if (def == ThingDef.Named("Megaspider_Meat"))
					if (def.ingestible.tasteThought == ThoughtDefOf.AteInsectMeatDirect)
					{
						CategoryScores[FoodCategory.InsectCorpse] += 50f;
						CategoryScores[FoodCategory.RawInsect] += 50f;
					}
					CategoryScores[FoodCategory.RawHuman] += 20f;
					CategoryScores[FoodCategory.RawBad] += 50f;
				}

				if ((def.ingestible.tasteThought == null || def.ingestible.tasteThought.stages.All((ThoughtStage arg) => arg.baseMoodEffect >= 0)))
					
					CategoryScores[FoodCategory.RawTasty] += 20f;

				//if ((foodType & FoodTypeFlags.AnimalProduct) != 0)
					
				//	CategoryScores[FoodCategory.AnimalProduct] += 30f;

				if (foodPref == FoodPreferability.NeverForNutrition || def.IsDrug)

					CategoryScores[FoodCategory.Ignore] = float.MaxValue;
			}

			// non ingestible corpse ?
			//if (def.IsCorpse)
			//	return FoodCategory.Ignore;

			var winner = CategoryScores.MaxBy(arg => arg.Value);

			var similar = CategoryScores.Where(arg => Mathf.Abs(arg.Value - winner.Value) < 10f);

			if (similar.Count() > 1)
			{
				Log.Warning("I'm not sure if " + def + " belongs to " + winner + " since others have similar scores: " + String.Join(" ; ", similar.Select(arg => arg.ToString()).ToArray()));
			}
			return winner.Key;
		}
	}
}
