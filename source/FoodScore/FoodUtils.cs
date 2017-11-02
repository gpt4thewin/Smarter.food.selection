using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;
using Verse.AI;

namespace WM.SmarterFoodSelection
{
	public static class FoodUtils
	{
		internal static bool MakeRatedFoodListForPawn(Map map, Pawn eater, Pawn getter, Policy policy, out List<FoodSourceRating> foodList, bool canUseInventory, bool allowForbidden)
		{
#if DEBUG
			Log.Message("MakeRatedFoodListForPawn() eater=" + eater + " getter=" + getter + " canuseinventory=" + canUseInventory);
#endif
			Func<Thing, bool> FoodValidator = (arg => IsValidFoodSourceForPawn(arg, eater, getter, policy, allowForbidden));
			var diet = policy.GetDietForPawn(eater);

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

			if (canUseInventory && getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && getter.inventory != null)
			{
				var inventoryFood = getter.inventory.innerContainer.Where(arg => arg.def.IsIngestible).Where(FoodValidator);
				searchSet.AddRange(inventoryFood);
#if DEBUG
				Log.Message(string.Format("MakeRatedFoodListForPawn() eater={0} getter={1} got {2}/{3} things from inventory", eater, getter, inventoryFood.Count(), getter.inventory.innerContainer.Count));
#endif
			}

			foodList = MakeRatedFoodListFromThingList(searchSet, eater, getter, policy);
#if DEBUG
			var foodListInventory = foodList.Where(arg => getter.inventory.innerContainer.Contains(arg.FoodSource));

			var inventoryScoreText = "Inventory:";

			foreach (var item in foodListInventory)
			{
				inventoryScoreText += "\n-------------\n" + item.ToWidgetString(true, item.DefRecord.category);
			}

			Log.Message(inventoryScoreText);
#endif

			if (!foodList.Any())
				return false;

			return true;
		}

		internal static List<FoodSourceRating> MakeRatedFoodListFromThingList(IEnumerable<Thing> list, Pawn eater, Pawn getter, Policy policy, bool doScoreSort = true)
		{
			var foodList = new List<FoodSourceRating>();

			if (!list.Any())
			{
				return foodList;
			}

			foreach (var item in list)
			{
				var foodEntry = FoodScoreUtils.FoodScoreFor(policy, eater, getter, item, eater.Map == null);

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

				bool inventory = (getter.inventory != null && getter.inventory.innerContainer.Contains(food));

				if (!(food != null &&
					  (inventory || food.Spawned) &&
						!food.Destroyed &&
					  (allowForbidden || !food.IsForbidden(getter)) &&
						(!food.def.IsIngestible || food.IngestibleNow) &&
					  (inventory || food.IsSociallyProper(eater) || food.IsSociallyProper(getter)) &&
						//TODO: eater == getter ? 
						getter.CanReachFoodSource(food) &&
					  (inventory || getter.CanReserve(food))
					 ))
					return false;

				if (food is Pawn)
				{
					if (((Pawn)food).RaceProps.Humanlike ||
						food.Map.designationManager.AllDesignationsOn(food).Any(arg => arg.def == DesignationDefOf.Tame) ||
						!IsAcceptablePreyFor(eater, food as Pawn) ||
						!policy.PolicyAllows(FoodCategory.Hunt) ||
						Utils.IsAnyoneCapturing(food.Map, food as Pawn) // redundant with humanlike ?
					   )
						return false;
				}
				else
				{
					var category = food.DetermineFoodCategory();
					if (!eater.RaceProps.CanEverEat((food is Building_NutrientPasteDispenser) ? ((Building_NutrientPasteDispenser)food).DispensableDef : food.def) ||
					   ((food is Building_NutrientPasteDispenser) && (!((Building_NutrientPasteDispenser)food).CanDispenseNow || !canManipulate)) ||
					   !policy.PolicyAllows(category) ||
					  	category == FoodCategory.Ignore ||
						category == FoodCategory.Luxury)
						return false;

					//TODO: plant pot avoidance not tested
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
