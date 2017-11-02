using Harmony;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.WorkGiver_InteractAnimal
{
	[HarmonyPatch(typeof(RimWorld.WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
	public static class TakeFoodForAnimalInteractJob
	{
		static bool Prefix(Pawn pawn)
		{
			return Config.ControlDisabledForPawn(pawn);
		}
		static void Postfix(ref Job __result, Pawn pawn, Pawn tamee)
		{
			__result = WorkGiver_InteractAnimal.TakeFoodForAnimalInteractJob(pawn, tamee);
		}
	}
}
