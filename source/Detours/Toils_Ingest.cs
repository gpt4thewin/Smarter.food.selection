using HugsLib.Source.Detour;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{
	public static class Toils_Ingest
	{
		[DetourMethod(typeof(RimWorld.Toils_Ingest), "TakeMealFromDispenser")]
		// RimWorld.Toils_Ingest
		public static Toil TakeMealFromDispenser(TargetIndex ind, Pawn eater)
		{
			Toil toil = new Toil();
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
			return toil;
		}
	}
}
