using System;
using System.Linq;
using System.Collections.Generic;
using HugsLib.Source.Detour;
using RimWorld.Planet;
using Verse;

namespace WM.SmarterFoodSelection.Detours
{
	public class CaravanInventoryUtility
	{
		[DetourMethod(typeof(RimWorld.Planet.CaravanInventoryUtility), "TryGetBestFood")]
		// RimWorld.Planet.CaravanInventoryUtility
		public static bool TryGetBestFood(Caravan caravan, Pawn forPawn, out Thing food, out Pawn owner)
		{
			List<Thing> list = RimWorld.Planet.CaravanInventoryUtility.AllInventoryItems(caravan);
			Thing thing = null;

			var foodsForPawn = FoodUtility.MakeRatedFoodListFromThingList(list, forPawn, forPawn.GetPolicyAssignedTo())
			                              .Where(arg => RimWorld.Planet.CaravanPawnsNeedsUtility.CanNowEatForNutrition(arg.FoodSource.def, forPawn));

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
