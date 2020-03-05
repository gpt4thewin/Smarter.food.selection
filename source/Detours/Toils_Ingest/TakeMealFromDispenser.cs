using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.Toils_Ingest
{
	[HarmonyPatch(typeof(RimWorld.Toils_Ingest), "TakeMealFromDispenser")]
	public static class TakeMealFromDispenser
	{
		[HarmonyPrefix]
		public static bool Prefix()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Postfix(out Toil __result,TargetIndex ind, Pawn eater)
		{
			var toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				var building_NutrientPasteDispenser = (Building_NutrientPasteDispenser)curJob.GetTarget(ind).Thing;

				DispenseMode mode;

				if (eater.IsCannibal())
					mode = DispenseMode.Cannibal;
				else if(eater.RaceProps.Animal)
					mode = DispenseMode.Animal;
				else
					mode = DispenseMode.Standard;

				Thing thing = building_NutrientPasteDispenser.TryDispenseFood(mode); // mod
				if (thing == null)
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					return;
				}
				actor.carryTracker.TryStartCarry(thing);
				actor.jobs.curJob.targetA = actor.carryTracker.CarriedThing;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;

			__result = toil;

			return;
		}
	}
}
