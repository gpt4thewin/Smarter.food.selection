using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.WorkGiver_InteractAnimal
{
	public static class WorkGiver_InteractAnimal
	{
		internal static Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
		{
			float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
			Thing thing;
			ThingDef thingdef;
			// -------- MOD --------
			bool result = Detours.FoodUtility.TryFindBestFoodSourceFor.Internal(pawn, tamee, false, out thing, out thingdef, true, false, false, Config.useCorpsesForTaming, Policies.Taming);
			// -------- --------
			if (!result)
			{
				return null;
			}
			return new Job(JobDefOf.TakeInventory, thing)
			{
				count = Mathf.CeilToInt(num / thing.GetNutritionAmount())
			};
		}
		internal static bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
		{
			if (Policies.Taming == null)
				throw new Exception("Everything is broken !");

			ThingOwner innerContainer = pawn.inventory.innerContainer;
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
