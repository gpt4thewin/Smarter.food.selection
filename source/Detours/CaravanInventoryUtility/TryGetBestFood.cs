using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld.Planet;
using Verse;
using WM.SmarterFoodSelection.Detours.FoodUtility;

namespace WM.SmarterFoodSelection.Detours.CaravanInventoryUtility
{
	[HarmonyPatch(typeof(RimWorld.Planet.CaravanInventoryUtility), "TryGetBestFood")]
	public class TryGetBestFood
	{
		public static void Postfix(ref bool __result,Caravan caravan, Pawn forPawn, out Thing food, out Pawn owner)
		{
			try
			{
				__result = Internal(caravan, forPawn, out food, out owner);
			}
			catch (Exception ex)
			{
				throw new Exception("Error when trying to find best food in caravan. eater=" + forPawn, ex);
			}
		}
		static bool Internal(Caravan caravan, Pawn forPawn, out Thing food, out Pawn owner)
		{
			List<Thing> list = RimWorld.Planet.CaravanInventoryUtility.AllInventoryItems(caravan)
									   .Where(arg => CaravanPawnsNeedsUtility.CanNowEatForNutrition(arg.def, forPawn)).ToList();
			Thing thing = null;

			Policy policy = forPawn.GetPolicyAssignedTo();
			var foodsForPawn = FoodUtils.MakeRatedFoodListFromThingList(list, forPawn, forPawn, forPawn.GetPolicyAssignedTo())
										  .Where(arg => RimWorld.Planet.CaravanPawnsNeedsUtility.CanNowEatForNutrition(arg.FoodSource.def, forPawn) &&
												 policy.PolicyAllows(forPawn, arg.FoodSource) );

			var foodEntry = foodsForPawn.FirstOrDefault();

			if (foodEntry != null)
				thing = foodsForPawn.FirstOrDefault().FoodSource;
			else
				thing = null;

			if (thing != null)
			{
#if DEBUG
				Log.Message("Caravan: best food for " + forPawn + " = " + thing);
#endif
				food = thing;
				owner = RimWorld.Planet.CaravanInventoryUtility.GetOwnerOf(caravan, thing);
				return true;
			}
#if DEBUG
			Log.Message("Caravan: no food found for " + forPawn);
#endif
			food = null;
			owner = null;
			return false;
		}
	}
}
