using System;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{
	[HarmonyPatch(typeof(RimWorld.WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
	public static class HasFoodToInteractAnimal
	{
		static bool Prefix(Pawn pawn)
		{
			return Config.ControlDisabledForPawn(pawn);
		}
		static void Postfix(RimWorld.WorkGiver_InteractAnimal __instance, ref bool __result, Pawn pawn, Pawn tamee)
		{
			__result = WorkGiver_InteractAnimal.WorkGiver_InteractAnimal.HasFoodToInteractAnimal(pawn, tamee);
		}
	}

	
}
