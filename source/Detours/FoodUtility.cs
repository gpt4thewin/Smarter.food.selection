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


		// RimWorld.RimWorld.FoodUtility
		private static bool _TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true)
		{
			Policy policy;

			//taming ? TODO: check for bug free
			policy = eater.GetPolicyAssignedTo(getter);

			if (getter.isWildAnimal() || policy.unrestricted || getter.InMentalState)
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

			bool flagAllowHunt = (getter != eater || !eater.RaceProps.predator || eater.health.hediffSet.HasTendableInjury());
			bool flagAllowPlant = (getter == eater);

			foodSource = pawnEntry.GetBestFood(flagAllowPlant, allowCorpse, flagAllowHunt);

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

			if (eater == getter && eater.RaceProps.predator && policy.PolicyAllows(FoodCategory.SafeHunting))
			{
				IEnumerable<Thing> allPawnsSpawned = eater.Map.mapPawns.AllPawnsSpawned
														  .Cast<Thing>()
														  //.Select(arg => arg as Thing)
														  .Where(FoodValidator);

				searchSet.AddRange((IEnumerable<Thing>)allPawnsSpawned);
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

		public static List<FoodSourceRating> MakeRatedFoodListFromThingList(IEnumerable<Thing> list, Pawn eater, Policy policy, bool doSort = true)
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
					item2.AddComp("Better sources", relativeRank * -80f);
				}
				relativeRank++;
			}

			if (doSort)
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
						(allowForbidden || !food.IsForbidden(eater)) &&
						(!food.def.IsIngestible || food.IngestibleNow) &&
					  //TODO: verify social properness works fine
					  (food.IsSociallyProper(eater) || food.IsSociallyProper(getter)) &&
						//TODO: eater == getter ? 
						getter.CanReachFoodSource(food) &&
					  getter.CanReserve(food)
					 ))
					return false;

				if (food is Pawn)
				{
					if (!RimWorld.FoodUtility.IsAcceptablePreyFor(eater, food as Pawn) ||
						!policy.PolicyAllows(FoodCategory.SafeHunting))
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
	}
}
