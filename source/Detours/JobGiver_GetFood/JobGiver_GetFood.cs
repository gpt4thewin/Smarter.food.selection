using System;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.JobGiver_GetFood
{
	//[HarmonyPatch(typeof(RimWorld.JobGiver_GetFood), "JobGiver_GetFood")]
	public static class TryGiveJob
	{
	}
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		//[DetourMethod(typeof(RimWorld.JobGiver_GetFood),"TryGiveJob")]
		// RimWorld.JobGiver_GetFood
		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag;
			if (pawn.RaceProps.Animal)
			{
				flag = true;
			}
			else
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
				flag = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
			}
			bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
			bool allowCorpse = flag;
			Thing thing;
			ThingDef def;
			if (!RimWorld.FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out def, true, true, false, allowCorpse))
			{
				return null;
			}
			Pawn pawn2 = thing as Pawn;
			if (pawn2 != null)
			{
				return new Job(JobDefOf.PredatorHunt, pawn2)
				{
					killIncappedTarget = true
				};
			}
			var building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
			{
				Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
				if (building != null)
				{
					ISlotGroupParent hopperSgp = building as ISlotGroupParent;
					//TODO utiliser original?
					Job job = RimWorld.WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
					if (job != null)
					{
						return job;
					}
				}
				thing = RimWorld.FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, FoodPreferability.MealLavish, false, false, false, false, false, false);
				if (thing == null)
				{
					return null;
				}
				def = thing.def;
			}
			return new Job(JobDefOf.Ingest, thing)
			{
				count = RimWorld.FoodUtility.WillIngestStackCountOf(pawn, def)
			};
		}
	}
}