using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Source.Detour;
using RimWorld.Planet;
using Verse;

namespace WM.SmarterFoodSelection.Detours
{
	public class CaravanInventoryUtility
	{
		[DetourMethod(typeof(RimWorld.Planet.CaravanInventoryUtility), "TryGetBestFood")]
		public static bool TryGetBestFood(Caravan caravan, Pawn forPawn, out Thing food, out Pawn owner)
		{
			try
			{
				return _TryGetBestFood(caravan, forPawn, out food, out owner);
			}
			catch (Exception ex)
			{
				throw new Exception("Error when trying to find best food in caravan. eater=" + forPawn, ex);
			}
		}
		// RimWorld.Planet.CaravanInventoryUtility
		public static bool _TryGetBestFood(Caravan caravan, Pawn forPawn, out Thing food, out Pawn owner)
		{
			List<Thing> list = RimWorld.Planet.CaravanInventoryUtility.AllInventoryItems(caravan)
									   .Where(arg => CaravanPawnsNeedsUtility.CanNowEatForNutrition(arg.def, forPawn)).ToList();
			Thing thing = null;

			Policy policy = forPawn.GetPolicyAssignedTo();
			var foodsForPawn = FoodUtility.MakeRatedFoodListFromThingList(list, forPawn, forPawn, forPawn.GetPolicyAssignedTo())
										  .Where(arg => RimWorld.Planet.CaravanPawnsNeedsUtility.CanNowEatForNutrition(arg.FoodSource.def, forPawn) &&
												 policy.PolicyAllows(forPawn, arg.FoodSource)
												);

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
