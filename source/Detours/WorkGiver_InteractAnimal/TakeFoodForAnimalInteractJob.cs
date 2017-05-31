using System;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{

	[HarmonyPatch(typeof(RimWorld.WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
	public static class TakeFoodForAnimalInteractJob
	{
		static bool Prefix(Pawn pawn)
		{
			return Config.ControlDisabledForPawn(pawn);
		}
		static void Postfix(RimWorld.WorkGiver_InteractAnimal __instance, ref Job __result, Pawn pawn, Pawn tamee)
		{
			__result = WorkGiver_InteractAnimal.WorkGiver_InteractAnimal.TakeFoodForAnimalInteractJob(pawn, tamee);
		}
	}
}
