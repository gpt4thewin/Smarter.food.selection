using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection.Detours
{
	public class WorkGiver_Warden_DeliverFood
	{
		// RimWorld.WorkGiver_Warden_DeliverFood
		//[DetourMethod(typeof(RimWorld.WorkGiver_Warden_DeliverFood), "FoodAvailableInRoomTo")]
		private static bool FoodAvailableInRoomTo(Pawn prisoner)
		{
			if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverFood.NutritionAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
			{
				return true;
			}
			float num = 0f;
			float num2 = 0f;
			Room room = RegionAndRoomQuery.GetRoom(prisoner);
			if (room == null)
			{
				return false;
			}
			for (int i = 0; i < room.RegionCount; i++)
			{
				Region region = room.Regions[i];
				List<Thing> list = region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing = list[j];
					// --------- mod ------------
					if ( (thing.def == ThingDefOf.NutrientPasteDispenser & prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) || thing.def.ingestible.preferability > FoodPreferability.NeverForNutrition)
					{
						num2 += WorkGiver_Warden_DeliverFood.NutritionAvailableForFrom(prisoner, thing);
					}
					// --------- mod end ------------
				}
				//if (region.ListerThings.ThingsOfDef(ThingDefOf.NutrientPasteDispenser).Any<Thing>() && prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				//{
				//	return true;
				//}
				List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int k = 0; k < list2.Count; k++)
				{
					Pawn pawn = list2[k] as Pawn;
					if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.RaceProps.CanEverEat(pawn.carryTracker.CarriedThing)))
					{
						num += pawn.needs.food.NutritionWanted;
					}
				}
			}

			return num2 + 0.5f >= num;
		}

		//TODO: use reflection
		private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
		{
			if (foodSource.def.IsNutritionGivingIngestible && p.RaceProps.CanEverEat(foodSource))
			{
				return foodSource.def.ingestible.CachedNutrition * (float)foodSource.stackCount;
			}
			if (p.RaceProps.ToolUser && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				var building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
				if (building_NutrientPasteDispenser != null && building_NutrientPasteDispenser.CanDispenseNow)
				{
					return Building_NutrientPasteDispenser_Detour.GetAvailableNutrition(building_NutrientPasteDispenser);
				}
			}
			return 0f;
		}

		//static float NutritionAvailableForFrom(Pawn prisoner, Thing thing)
		//{
		//	return (float)typeof(RimWorld.WorkGiver_Warden_DeliverFood).GetMethod("NutritionAvailableForFrom").Invoke(null, new object[] { prisoner, thing });
		//}
	}
}
