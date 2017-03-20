using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Source.Detour;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WM.SmarterFoodSelection.Detours
{
	public enum DispenseMode
	{
		Standard,
		Clean, // no insect no human meat
		Cannibal,
		CannibalClean, // no insect meat
		Animal			// prefers human meat then insect meat
	}
	public static class Building_NutrientPasteDispenser_Detour
	{
		static FoodCategory[] ranksForCannibals =
		{
			FoodCategory.RawHuman,
			FoodCategory.RawBad,
			FoodCategory.RawTasty,
			FoodCategory.RawInsect
		};
		static FoodCategory[] ranksForOthers =
		{
			FoodCategory.RawBad,
			FoodCategory.RawTasty,
			FoodCategory.RawInsect,
			FoodCategory.RawHuman
		};
		static FoodCategory[] ranksForAnimals =
		{
			FoodCategory.RawHuman,
			FoodCategory.RawInsect,
			FoodCategory.RawBad,
			FoodCategory.RawTasty
		};

		public static int FoodCostPerDispense
		{
			get
			{
				return ThingDefOf.NutrientPasteDispenser.building.foodCostPerDispense;
			}
		}

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
		static int rankForPawn(DispenseMode mode, ThingDef def)
#pragma warning restore RECS0082 // Parameter has the same name as a member and hides it
		{
			FoodCategory pref = def.DetermineFoodCategory();
			FoodCategory[] rank;

			if (mode == DispenseMode.Cannibal || mode == DispenseMode.CannibalClean)
			{
				rank = ranksForCannibals;
			}
			else
			{
				rank = ranksForOthers;
			}

			int num = Array.IndexOf(rank, pref);
			if (num == -1)
			{
				Log.Warning("Found unexpected food in hopper : " + def);
				num = rank.Count()-1;
			}

			return num;
		}

		//[DetourMethod(typeof(RimWorld.Building_NutrientPasteDispenser),"TryDispenseFood")]
		// RimWorld.Building_NutrientPasteDispenser
		public static Thing TryDispenseFood(this RimWorld.Building_NutrientPasteDispenser self, DispenseMode mode = DispenseMode.Standard, bool silent = false)
		{
			if (!self.CanDispenseNow)
			{
				return null;
			}

			// ----- begin mod code ------

			List<Thing> ingredients;

			ingredients = IngredientsFor(self, mode);

			if (ingredients.Sum((arg) => arg.stackCount) < FoodCostPerDispense)
			{
				if (!silent)
					Log.Error("Did not find enough food in hoppers while trying to dispense. (" + ingredients.Count + "/" + FoodCostPerDispense + ")");
				return null;
			}
#if DEBUG
			//foreach (var e in query)
			//{
			//	Log.Message("dispenser has " + e.def + " rank: #" + rankForPawn(eater, e.def) + " total count: " + e.stackCount);
			//}
			//Log.Message("dispense");
#endif

			self.def.building.soundDispense.PlayOneShot(new TargetInfo(self.Position, self.Map, false));

			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste, null);
			CompIngredients compIngredients = thing2.TryGetComp<CompIngredients>();

			int num = 0;

			for (int i = 0; num < FoodCostPerDispense; i++)
			{
				int num2 = Math.Min(ingredients[i].stackCount, FoodCostPerDispense);
				num += num2;
				ingredients[i].SplitOff(num2);
				compIngredients.RegisterIngredient(ingredients[i].def);
			}

			if (Config.SeparatedNutrientPaste && compIngredients.ingredients.Any((arg) => arg.DetermineFoodCategory() == FoodCategory.RawHuman))
				thing2.def = ThingDef.Named("MealNutrientPasteCannibal");

			// ----- end mod code ------

			return thing2;
		}

		static List<Thing> IngredientsFor(Building_NutrientPasteDispenser self, DispenseMode mode)
		{
			var list = GetAllHoppersThings(self);
			int[] foodCountByRank = new int[4];

			for (int i = 0; i < 4; i++)
			{
				var current = foodCountByRank[i];
				foodCountByRank[i] = list.Where((arg) => rankForPawn(mode, arg.def) == i).Sum((arg) => arg.stackCount);
			}

			FoodCategory[] ranking;

			if (mode == DispenseMode.Cannibal || mode == DispenseMode.CannibalClean)
			{
				ranking = ranksForCannibals;
			}
			else if (mode == DispenseMode.Animal)
			{
				ranking = ranksForAnimals;
			}
			else
			{
				ranking = ranksForOthers;
			}

			var rawbadindex = Array.IndexOf(ranking, FoodCategory.RawBad);
			var rawtastyindex = Array.IndexOf(ranking, FoodCategory.RawTasty);

			foodCountByRank[rawbadindex] = foodCountByRank[rawtastyindex] = foodCountByRank[rawbadindex] + foodCountByRank[rawtastyindex];

			var query = (from e in list
			             orderby foodCountByRank[rankForPawn(mode, e.def)] < 6, rankForPawn(mode, e.def), (e.TryGetComp<CompRottable>() != null ? e.TryGetComp<CompRottable>().TicksUntilRotAtCurrentTemp : -9999999)
						 select e).ToList();

			if (mode == DispenseMode.Clean)
			{
				query = query.Where((Thing arg) => !RimWorld.FoodUtility.IsHumanlikeMeat(arg.def) && arg.def.DetermineFoodCategory() != FoodCategory.RawInsect).ToList();
			}
			else if (mode == DispenseMode.CannibalClean)
			{
				query = query.Where((Thing arg) => arg.def.DetermineFoodCategory() != FoodCategory.RawInsect).ToList();
			}

			return query;
		}

		public static float GetAvailableNutrition(this RimWorld.Building_NutrientPasteDispenser self)
		{
			if (!self.CanDispenseNow)
				return 0f;
			
			var list = GetAllHoppersThings(self);

			return  Mathf.Floor(list.Sum((arg) => arg.stackCount) / FoodCostPerDispense) * ThingDefOf.MealNutrientPaste.ingestible.nutrition;
		}

		static List<Thing> GetAllHoppersThings(this RimWorld.Building_NutrientPasteDispenser self)
		{
			var list = new List<Thing>();

			for (int i = 0; i < self.AdjCellsCardinalInBounds().Count; i++)
			{
				IntVec3 c = self.AdjCellsCardinalInBounds()[i];
				Building edifice = c.GetEdifice(self.Map);
				if (edifice != null && edifice.def == ThingDefOf.Hopper
				   // && eater.CanReach(edifice, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn)
				   )
				{
					List<Thing> thingList = self.AdjCellsCardinalInBounds()[i].GetThingList(self.Map).ToList();

					list.AddRange(thingList.Where((Thing arg) => Building_NutrientPasteDispenser.IsAcceptableFeedstock(arg.def)));
				}
			}

			return list;
		}

		public static List<ThoughtDef> GetBestMealThoughtsFor(this RimWorld.Building_NutrientPasteDispenser self, Pawn eater)
		{
			List<ThoughtDef> thoughts;

			if (!self.HasEnoughFeedstockInHoppers() || !eater.RaceProps.Humanlike)
				return new List<ThoughtDef>();

			var list = IngredientsFor(self, eater.IsCannibal() ? DispenseMode.Cannibal : DispenseMode.Standard);

			// make dummy meal for thoughts simulation

			Thing dummyMeal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste, null);
			CompIngredients compIngredients = dummyMeal.TryGetComp<CompIngredients>();

			int num = 0;

			for (int i = 0; num < FoodCostPerDispense; i++)
			{
				int num2 = Math.Min(list[i].stackCount, FoodCostPerDispense);
				num += num2;
				compIngredients.RegisterIngredient(list[i].def);
			}

			thoughts = RimWorld.FoodUtility.ThoughtsFromIngesting(eater, dummyMeal);

			dummyMeal.Destroy();

			return thoughts;
		}

		static Thing FindFeedInAnyHopper(RimWorld.Building_NutrientPasteDispenser self)
		{
			return (Thing)typeof(RimWorld.Building_NutrientPasteDispenser).GetMethod("FindFeedInAnyHopper", Helpers.AllBindingFlags).Invoke(self, null);
		}

		// RimWorld.Building_NutrientPasteDispenser
		//static private List<IntVec3> AdjCellsCardinalInBounds(this Building_NutrientPasteDispenser self)
		//{
		//	return (List<IntVec3>)typeof(RimWorld.Building_NutrientPasteDispenser).GetProperty("AdjCellsCardinalInBounds", Helpers.AllBindingFlags).GetValue(self,null);
		//}

		static private List<IntVec3> AdjCellsCardinalInBounds(this Building_NutrientPasteDispenser self)
		{
			if (self.cachedAdjCellsCardinal() == null)
			{
				var value = (from c in GenAdj.CellsAdjacentCardinal(self)
							 where c.InBounds(self.Map)
							 select c).ToList<IntVec3>();

				self.cachedAdjCellsCardinal_set_(value);
			}
			return self.cachedAdjCellsCardinal();
		}

		static private List<IntVec3> cachedAdjCellsCardinal(this Building_NutrientPasteDispenser self)
		{
			return (List<IntVec3>)typeof(RimWorld.Building_NutrientPasteDispenser).GetField("cachedAdjCellsCardinal", Helpers.AllBindingFlags).GetValue(self);
		}
		static private void cachedAdjCellsCardinal_set_(this Building_NutrientPasteDispenser self, List<IntVec3> value)
		{
			typeof(RimWorld.Building_NutrientPasteDispenser).GetField("cachedAdjCellsCardinal", Helpers.AllBindingFlags).SetValue(self, value);
		}
	}
}

