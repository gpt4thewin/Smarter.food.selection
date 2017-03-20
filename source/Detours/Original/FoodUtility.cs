using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.Original
{
	public static class FoodUtility
	{
		// RimWorld.FoodUtility
		public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true)
		{
			bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			Thing thing = null;
			if (canUseInventory)
			{
				if (flag)
				{
					thing = RimWorld.FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.MealAwful, FoodPreferability.MealLavish, 0f, false);
				}
				if (thing != null)
				{
					if (getter.Faction != Faction.OfPlayer)
					{
						foodSource = thing;
						foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
						return true;
					}
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						foodSource = thing;
						foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
						return true;
					}
				}
			}
			bool allowPlant = getter == eater;
			Thing thing2 = RimWorld.FoodUtility.BestFoodSourceOnMap(getter, eater, desperate, FoodPreferability.MealLavish, allowPlant, true, allowCorpse, true, canRefillDispenser, allowForbidden);
			if (thing == null && thing2 == null)
			{
				if (canUseInventory && flag)
				{
					thing = RimWorld.FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, false);
					if (thing != null)
					{
						foodSource = thing;
						foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
						return true;
					}
				}
				if (thing2 == null && getter == eater && getter.RaceProps.predator)
				{
					Pawn pawn = FoodUtility.BestPawnToHuntForPredator(getter);
					if (pawn != null)
					{
						foodSource = pawn;
						foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
						return true;
					}
				}
				foodSource = null;
				foodDef = null;
				return false;
			}
			if (thing == null && thing2 != null)
			{
				foodSource = thing2;
				foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
				return true;
			}
			if (thing2 == null && thing != null)
			{
				foodSource = thing;
				foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
				return true;
			}
			float num = FoodUtility.FoodSourceOptimality(eater, thing2, (getter.Position - thing2.Position).LengthManhattan);
			float num2 = FoodUtility.FoodSourceOptimality(eater, thing, 0f);
			num2 -= 32f;
			if (num > num2)
			{
				foodSource = thing2;
				foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
				return true;
			}
			foodSource = thing;
			foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource);
			return true;
		}
		// RimWorld.FoodUtility
		internal static float FoodSourceOptimality(Pawn eater, Thing t, float dist)
		{
			return (float)typeof(RimWorld.FoodUtility).GetMethod("FoodSourceOptimality", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { eater, t, dist });
		}
		// RimWorld.FoodUtility
		public static Pawn BestPawnToHuntForPredator(Pawn predator)
		{
			if (predator.meleeVerbs.TryGetMeleeVerb() == null)
			{
				return null;
			}
			bool flag = false;
			float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
			if (summaryHealthPercent < 0.25f)
			{
				flag = true;
			}
			List<Pawn> allPawnsSpawned = predator.Map.mapPawns.AllPawnsSpawned;
			Pawn pawn = null;
			float num = 0f;
			bool tutorialMode = TutorSystem.TutorialMode;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn2 = allPawnsSpawned[i];
				if (predator.GetRoom() == pawn2.GetRoom())
				{
					if (predator != pawn2)
					{
						if (!flag || pawn2.Downed)
						{
							if (RimWorld.FoodUtility.IsAcceptablePreyFor(predator, pawn2))
							{
								if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
								{
									if (!pawn2.IsForbidden(predator))
									{
										if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
										{
											float preyScoreFor = RimWorld.FoodUtility.GetPreyScoreFor(predator, pawn2);
											if (preyScoreFor > num || pawn == null)
											{
												num = preyScoreFor;
												pawn = pawn2;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return pawn;
		}

		// RimWorld.FoodUtility
		internal static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
{
	new CurvePoint(-100f, -600f),
	new CurvePoint(-10f, -100f),
	new CurvePoint(-5f, -70f),
	new CurvePoint(-1f, -50f),
	new CurvePoint(0f, 0f),
	new CurvePoint(100f, 800f)
};

	}
}
