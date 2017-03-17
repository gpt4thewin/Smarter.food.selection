/*
 * Created by SharpDevelop.
 * User: Julien
 * Date: 22/11/2016
 * Time: 20:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using HugsLib.Source.Detour;

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

			if (FoodUtility.TryFindBestFoodSourceFor(pawn, tamee, false, out thing, out def, false, true, false, Config.useCorpsesForTaming))
			{
				return null;
			}
			return new Job(JobDefOf.TakeInventory, thing)
			{
				count = Mathf.CeilToInt(num / thing.def.ingestible.nutrition)
			};
		}


	}
}
