using System;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.Original
{
	public class WorkGiver_CookFillHopper
	{
		// RimWorld.WorkGiver_CookFillHopper
		public static Job HopperFillFoodJob(Pawn pawn, ISlotGroupParent hopperSgp)
		{
			return (Job)Type.GetType("RimWorld.WorkGiver_CookFillHopper").GetMethod("HopperFillFoodJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { pawn, hopperSgp });
		}
	}
}
