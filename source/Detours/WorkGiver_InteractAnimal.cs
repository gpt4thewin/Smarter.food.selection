using System;
using HugsLib.Source.Detour;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{

	public class WorkGiver_InteractAnimal
	{
		[DetourMethod(typeof(RimWorld.WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
		// RimWorld.WorkGiver_InteractAnimal
		protected Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
		{
			float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
			Thing thing;
			ThingDef def;

			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, tamee, false, out thing, out def, false, false, false, Config.useCorpsesForTaming))
			{
				return null;
			}
			return new Job(JobDefOf.TakeInventory, thing)
			{
				count = Mathf.CeilToInt(num / thing.GetNutritionAmount())
			};
		}

		[DetourMethod(typeof(RimWorld.WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
		// RimWorld.WorkGiver_InteractAnimal
		protected bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
		{
			if (Policies.Taming == null)
				throw new Exception("Everything is broken !");

			ThingContainer innerContainer = pawn.inventory.innerContainer;
			int num = 0;
			float num2 = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
			float num3 = 0f;

			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				if (tamee.RaceProps.CanEverEat(thing) && Policies.Taming.PolicyAllows(tamee, thing))
				{
					for (int j = 0; j < thing.stackCount; j++)
					{
						num3 += thing.def.ingestible.nutrition;
						if (num3 >= num2)
						{
							num++;
							num3 = 0f;
						}
						if (num >= 2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

	}
}
