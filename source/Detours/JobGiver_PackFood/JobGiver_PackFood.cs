using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.JobGiver_PackFood
{
	[HarmonyPatch(typeof(RimWorld.JobGiver_PackFood), "TryGiveJob")]
	public static class TryGiveJob
	{
		[HarmonyPostfix]
		public static void _Postfix(ref Job __result, Pawn pawn)
		{
			if (__result == null)
			{
				//only search for food when the base search returns something.
				//this keeps basic nutrition / inventory checks intact (checking if there's already food in inventory etc)
				//it also means pawns won't be wandering across half the map to 'pack their lunch' since the base search only searches 20 tiles, they'll only check for SFS food if there's *some* food within 20 tiles.
				return;
			}

			//find best SFS food source

			Thing foodSource;
			ThingDef foodDef;

			if (RimWorld.FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, false, out foodSource, out foodDef, true, false, false, false, false))
			{
				//don't pickup food that doesnt get counted as a meal when originally searching for food to pick up.
				//prevents pawns picking up endless amounts of raw ingredients as meals in a loop.
				if (foodDef.ingestible.nutrition > 0.3f && foodDef.ingestible.preferability >= FoodPreferability.MealAwful)
				{
				    //overwrite values in existing job
				    __result.SetTarget(TargetIndex.A, foodSource); //food source thing target
				    __result.count = Mathf.Max(1, Mathf.Min(foodDef.ingestible.maxNumToIngestAtOnce, RimWorld.FoodUtility.StackCountForNutrition(foodDef, 0.3f))); //number of things to pickup
				    return;
				}
			}
			//if SFS didn't find an acceptable food, don't go picking up whatever the original search said to.
			__result = null;
		}
	}
}
