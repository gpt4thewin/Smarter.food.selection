using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Source.Detour;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{
	public static class FoodUtility
	{
		[DetourMethod(typeof(RimWorld.FoodUtility), "TryFindBestFoodSourceFor")]
		public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true)
		{
			try
			{
				bool result = false;
				result = _TryFindBestFoodSourceFor(getter, eater, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, allowForbidden, allowCorpse);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("{0}: Exception when fetching. (getter={1} eater={2})\n{3}\n{4}", ModCore.modname, getter, eater, ex.Message, ex.StackTrace), ex);
			}
		}

		// RimWorld.FoodUtility
		private static bool _TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true)
		{
			Policy policy;

			//taming ? TODO: check for bug free
			policy = eater.GetPolicyAssignedTo(getter);

			if (getter.isWildAnimal() || policy.unrestricted || getter.InMentalState || Config.ControlDisabledForPawn(eater))
			{
				return Original.FoodUtility.TryFindBestFoodSourceFor(getter, eater, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, allowForbidden, allowCorpse);
			}

			List<FoodSourceRating> FoodListForPawn;

			FoodSearchCache.PawnEntry pawnEntry;

			if (!FoodSearchCache.TryGetEntryForPawn(getter, eater, out pawnEntry, allowForbidden))
			{
				bool foundFood = MakeRatedFoodListForPawn(getter.Map, eater, getter, policy, out FoodListForPawn, canUseInventory, allowForbidden);

				pawnEntry = FoodSearchCache.AddPawnEntry(getter, eater, FoodListForPawn);
			}

			bool flagAllowHunt = (getter == eater && eater.RaceProps.predator && !eater.health.hediffSet.HasTendableInjury());
			bool flagAllowPlant = (getter == eater);

			// C# 5 :'(
			var foodSourceRating = pawnEntry.GetBestFoodEntry(flagAllowPlant, allowCorpse, flagAllowHunt);
			if (foodSourceRating != null)
			{
				foodSource = foodSourceRating.FoodSource;
			}
			else
				foodSource = null;

			if (foodSource == null)
			{
				foodDef = null;
				return false;
			}

			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			return true;

			//bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			//Thing thing = null;
			//if (canUseInventory)
			//{
			//	if (flag)
			//	{
			//		thing = RimWorld.FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.MealAwful, FoodPreferability.MealLavish, 0f, false);
			//	}
			//	if (thing != null)
			//	{
			//		if (getter.Faction != Faction.OfPlayer)
			//		{
			//			foodSource = thing;
			//			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			//			return true;
			//		}
			//		CompRottable compRottable = thing.TryGetComp<CompRottable>();
			//		if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
			//		{
			//			foodSource = thing;
			//			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			//			return true;
			//		}
			//	}
			//}
			//bool allowPlant = getter == eater;
		}


		private static bool MakeRatedFoodListForPawn(Map map, Pawn eater, Pawn getter, Policy policy, out List<FoodSourceRating> foodList, bool canUseInventory, bool allowForbidden)
		{
			Func<Thing, bool> FoodValidator = (arg => IsValidFoodSourceForPawn(arg, eater, getter, policy, allowForbidden));

			var diet = policy.GetDietForPawn(eater);

			//foreach (var item in policy.PerRacesDiet[eater.def].elements)
			//{
			//	FoodCategoryUtils.FoodRecords[item
			//	var foodsOfCategory = ite
			//}

			// ------------------------------------------------------------------------------------------------
			ThingRequestGroup thingRequest;

			//TODO: detour ThingsLister
			if (!policy.PerRacesDiet[eater.def].ContainsElement(FoodCategory.Grass) &&
			   !policy.PerRacesDiet[eater.def].ContainsElement(FoodCategory.Hay))
			{
				thingRequest = ThingRequestGroup.FoodSourceNotPlantOrTree;
			}
			else
			{
				thingRequest = ThingRequestGroup.FoodSource;
			}

			List<Thing> searchSet = map.listerThings.ThingsInGroup(thingRequest).Where(FoodValidator).ToList();

			// TODO: Limits the number of searched by category
			if (searchSet.Count >= Config.FoodSearchMaxItemsCount)
			{
#if DEBUG
				int num = searchSet.Count;
#endif

				var newsearchSet = searchSet.OrderBy((arg) => (arg.Position - getter.Position).LengthManhattan).ToList();
				searchSet = newsearchSet.GetRange(0, Math.Min(newsearchSet.Count, Config.FoodSearchMaxItemsCount));

#if DEBUG
				Log.Message(string.Format("MakeRatedFoodListForPawn(): too many items, reduced from {0} to {1}", num, searchSet.Count));
#endif

				//var newsearchSet = new List<Thing>();
				//var categoriesList = from entry in searchSet
				//					 group entry by entry.def;
				//foreach (var group in categoriesList)
				//{
				//	if (group.Count() > 500)
				//	{
				//		var list = group.OrderByDescending((arg) => (arg.Position - getter.Position).LengthManhattan);
				//	}
				//}
			}

			if (eater == getter && eater.RaceProps.predator && policy.PolicyAllows(FoodCategory.Hunt))
			{
				IEnumerable<Thing> allPawnsSpawned = eater.Map.mapPawns.AllPawnsSpawned
														  .Cast<Thing>()
														  //.Select(arg => arg as Thing)
														  .Where(FoodValidator);

				searchSet.AddRange(allPawnsSpawned);
			}

			if (canUseInventory && getter.inventory != null)
			{
				var inventoryFood = getter.inventory.innerContainer.Where(FoodValidator);
				searchSet.AddRange(inventoryFood);
			}

			foodList = MakeRatedFoodListFromThingList(searchSet, eater, policy);

			if (!foodList.Any())
				return false;

			return true;
		}

		public static List<FoodSourceRating> MakeRatedFoodListFromThingList(IEnumerable<Thing> list, Pawn eater, Policy policy, bool doScoreSort = true)
		{
			var foodList = new List<FoodSourceRating>();

			if (!list.Any())
			{
				return foodList;
			}

			foreach (var item in list)
			{
				var foodEntry = FoodScoreUtils.FoodScoreFor(policy, eater, item, eater.Map == null);

				foodList.Add(foodEntry);
			}

			var sortedFoodList = (from entry in foodList
								  group entry by policy.GetFoodCategoryRankForPawn(eater, entry.FoodSource))
				.OrderBy(arg => arg.Key);

			int relativeRank = 0;
			foreach (var item in sortedFoodList)
			{
				foreach (var item2 in item)
				{
					item2.AddComp("Better sources", relativeRank * -150f);
				}
				relativeRank++;
			}

			if (doScoreSort)
				foodList = foodList.OrderByDescending((arg) => arg.Score).ToList();

			return foodList;
		}

		internal static bool IsValidFoodSourceForPawn(this Thing food, Pawn eater, Pawn getter, Policy policy, bool allowForbidden)
		{
			try
			{
				bool canManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);

				if (!(food != null &&
						food.Spawned &&
						!food.Destroyed &&
					  (allowForbidden || !food.IsForbidden(getter)) &&
						(!food.def.IsIngestible || food.IngestibleNow) &&
					  (food.IsSociallyProper(eater) || food.IsSociallyProper(getter)) &&
						//TODO: eater == getter ? 
						getter.CanReachFoodSource(food) &&
					  getter.CanReserve(food)
					 ))
					return false;

				if (food is Pawn)
				{
					if (food.Map.designationManager.AllDesignationsOn(food).Any(arg => arg.def == DesignationDefOf.Tame) ||
						!FoodUtility.IsAcceptablePreyFor(eater, food as Pawn) ||
						!policy.PolicyAllows(FoodCategory.Hunt) ||
						Utils.IsAnyoneCapturing(food.Map, food as Pawn)
					   )
						return false;
				}
				else
				{
					var category = food.DetermineFoodCategory();
					if (!eater.RaceProps.CanEverEat((food is Building_NutrientPasteDispenser) ? ((Building_NutrientPasteDispenser)food).DispensableDef : food.def) ||
					   ((food is Building_NutrientPasteDispenser) && (!((Building_NutrientPasteDispenser)food).CanDispenseNow || !canManipulate)) ||
					   !policy.PolicyAllows(category))
						return false;

					if (food.def.plant != null && food.Position.GetThingList(food.Map).Any((obj) => obj.def == ThingDef.Named("PlantPot")))
						return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("food={0} eater={1} policy={2}", food, eater, policy), ex);
			}
		}

		// RimWorld.FoodUtility
		public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
		{
			if (!prey.RaceProps.canBePredatorPrey)
			{
				return false;
			}
			if (!prey.RaceProps.IsFlesh)
			{
				return false;
			}
			if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
			{
				return false;
			}
			if (!prey.Downed)
			{
				if (GetPreyRatio(predator, prey) > Config.petsSafeHuntMaxStrengthRatio)
				{
					return false;
				}

				//if (GetPreyRatio1(predator, prey) > Config.petsSafeHuntMaxStrengthRatio)
				//{
				//	return false;
				//}

				//if (GetPreyRatio2(predator, prey) > Config.petsSafeHuntMaxStrengthRatio)
				//{
				//	return false;
				//}
			}
			return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
		}

		internal static float GetPreyRatio(Pawn predator, Pawn prey)
		{
			return Math.Min(
							GetPreyRatio1(predator, prey),
							GetPreyRatio2(predator, prey) / 0.85f);
		}

		internal static float GetPreyRatio1(Pawn predator, Pawn prey)
		{
			return prey.kindDef.combatPower / predator.kindDef.combatPower;
		}

		internal static float GetPreyRatio2(Pawn predator, Pawn prey)
		{
			float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
			float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;

			return num / num2;
		}
	}
}
