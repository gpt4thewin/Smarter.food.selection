using Harmony;
using Verse;

namespace WM.SmarterFoodSelection.Detours
{
	[HarmonyPatch(typeof(RimWorld.WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
	public static class HasFoodToInteractAnimal
	{
		static bool Prefix(Pawn pawn)
		{
			return Config.ControlDisabledForPawn(pawn);
		}
		static void Postfix(ref bool __result, Pawn pawn, Pawn tamee)
		{
			__result = WorkGiver_InteractAnimal.WorkGiver_InteractAnimal.HasFoodToInteractAnimal(pawn, tamee);
		}
	}
}
