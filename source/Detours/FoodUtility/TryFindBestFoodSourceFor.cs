using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.FoodUtility
{
	[HarmonyPatch(typeof(RimWorld.FoodUtility), "TryFindBestFoodSourceFor")]
	public static class TryFindBestFoodSourceFor
	{
		[HarmonyPrefix]
		//public static bool Prefix(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, Policy forcedPolicy = null)
		public static bool _Prefix(ref bool __state, Pawn getter, Pawn eater, bool desperate, bool canRefillDispenser, bool canUseInventory, bool allowForbidden, bool allowCorpse)
		{
#if DEBUG
			//Log.Message("Prefix of TryFindBestFoodSourceFor() getter=" + getter + " eater=" + eater + " desperate=" + desperate + " canUseInventory=" + canUseInventory + " allowForbidden=" + allowForbidden);
#endif
			Policy policy;

			//taming ? TODO: check for bug free
			//if (forcedPolicy != null)
			//	policy = forcedPolicy;
			//else
			policy = eater.GetPolicyAssignedTo(getter);

			if (getter.isWildAnimal() || getter.isInsectFaction() || policy.unrestricted || getter.InMentalState || Config.ControlDisabledForPawn(eater))
			{
				__state = true;
			}
			else
			{
				__state = false;
			}

			return __state;
		}

		[HarmonyPostfix]
		public static void _Postfix(ref bool __state, ref bool __result, Pawn getter, Pawn eater, bool desperate, ref Thing foodSource, ref ThingDef foodDef, bool canRefillDispenser, bool canUseInventory, bool allowForbidden, bool allowCorpse)
		{
			if (__state)
				return;

			try
			{
				__result = Internal(getter, eater, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, allowForbidden, allowCorpse);
				return;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("{0}: Exception when fetching. (getter={1} eater={2})\n{3}\n{4}", ModCore.modname, getter, eater, ex, ex.StackTrace), ex);
			}
		}

		internal static bool Internal(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, Policy forcedPolicy = null)
		{
			List<FoodSourceRating> FoodListForPawn;

			FoodSearchCache.PawnEntry pawnEntry;

			if (!FoodSearchCache.TryGetEntryForPawn(getter, eater, out pawnEntry, allowForbidden))
			{
				Policy policy;

				if (forcedPolicy != null)
					policy = forcedPolicy;
				else
					policy = PolicyUtils.GetPolicyAssignedTo(eater, getter);
				
				bool foundFood = FoodUtils.MakeRatedFoodListForPawn(getter.Map, eater, getter, policy, out FoodListForPawn, canUseInventory, allowForbidden);

				pawnEntry = FoodSearchCache.AddPawnEntry(getter, eater, FoodListForPawn);
			}

			bool flagAllowHunt = (getter == eater && eater.RaceProps.predator && !eater.health.hediffSet.HasTendableInjury());
			bool flagAllowPlant = (getter == eater);

			// C# 5 :'(
			var foodSourceRating = pawnEntry.GetBestFoodEntry(flagAllowPlant, allowCorpse, flagAllowHunt);
			if (foodSourceRating != null)
			{
				foodSource = foodSourceRating.FoodSource;
			}
			else
				foodSource = null;

			if (foodSource == null)
			{
				foodDef = null;
				return false;
			}

			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			return true;

			//bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			//Thing thing = null;
			//if (canUseInventory)
			//{
			//	if (flag)
			//	{
			//		thing = RimWorld.FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.MealAwful, FoodPreferability.MealLavish, 0f, false);
			//	}
			//	if (thing != null)
			//	{
			//		if (getter.Faction != Faction.OfPlayer)
			//		{
			//			foodSource = thing;
			//			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			//			return true;
			//		}
			//		CompRottable compRottable = thing.TryGetComp<CompRottable>();
			//		if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
			//		{
			//			foodSource = thing;
			//			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			//			return true;
			//		}
			//	}
			//}
			//bool allowPlant = getter == eater;
		}
	}
}
