using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HugsLib.Source.Detour;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours
{
	public static class FoodUtility_Debug
	{
		static Thing bestFoodSource;
		static Thing bestFoodSourceFromMouse;

		static Pawn eater;
		static Pawn getter;

		// RimWorld.FoodUtility
		[DetourMethod(typeof(RimWorld.FoodUtility), "DebugFoodSearchFromMouse_OnGUI")]
		public static void DebugFoodSearchFromMouse_OnGUI()
		{
			try
			{
				_DebugFoodSearchFromMouse_OnGUI();
			}
			catch (Exception ex)
			{
				DebugViewSettings.drawFoodSearchFromMouse = false;

				throw new Exception("DebugFoodSearchFromMouse_OnGUI() threw exception." + ex.Message + "\n" + ex.StackTrace, ex);
			}
		}

		static void _DebugFoodSearchFromMouse_OnGUI()
		{
			IntVec3 a = Verse.UI.MouseCell();

			bestFoodSourceFromMouse = null;
			bestFoodSource = null;

			// ------------------------------------------------------------------------------

			Pawn pawnA = null;
			Pawn pawnB = null;

			var pawnSelection = Find.Selector.SelectedObjects.Where(arg => arg is Pawn).Cast<Pawn>();
			pawnA = pawnSelection.FirstOrDefault();
			if (pawnSelection.Count() == 2)
				pawnB = pawnSelection.ElementAt(1);


			if (pawnB != null && pawnA != null && pawnA.Faction != pawnB.Faction)
			{
				//TODO: watch out for compatibility
				if (pawnA.IsColonist)
				{
					eater = pawnB;
					getter = pawnA;
				}
				else if (pawnB.IsColonist)
				{
					eater = pawnA;
					getter = pawnB;
				}
				else
					eater = getter = pawnA;
			}
			else
				eater = getter = pawnA;


			// ------------------------------------------------------------------------------

			if (eater == null)
			{
				return;
			}
			if (eater.Map != Find.VisibleMap)
			{
				return;
			}
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;

			var policy = eater.GetPolicyAssignedTo();

			ThingDef dum2;
			// Generates cache
			var foundFood = FoodUtility.TryFindBestFoodSourceFor(getter, eater, true, out bestFoodSource, out dum2);

			if (!foundFood)
				return;

			// ------------------------------------------------------------------------------

			List<FoodSourceRating> foodsToDisplay;

			var selectedFoods = Find.Selector.SelectedObjects
												.Where(arg => (arg is Thing) && (arg as Thing).DetermineFoodCategory() != FoodCategory.Null)
												.Cast<Thing>().ToList();

			FoodSearchCache.PawnEntry foodList;
			//IEnumerable<Thing> foodList = 
			if (getter.isWildAnimal() || policy.unrestricted || !FoodSearchCache.TryGetEntryForPawn(getter, eater, out foodList, true))
				return;

			foodsToDisplay = foodList.AllRankedFoods
									 .OrderBy(arg => (a - arg.FoodSource.Position).LengthManhattan).ToList()
										 .GetRange(0, Math.Min(200, foodList.AllRankedFoods.Count));


			bool advancedInfoForAll;

			if (!selectedFoods.Any())
			{
				selectedFoods.Add(bestFoodSource);
				selectedFoods.Add(foodList.AllRankedFoods.MinBy((arg) => (a - arg.FoodSource.Position).LengthHorizontal).FoodSource);
				advancedInfoForAll = false;
			}
			else
				advancedInfoForAll = true;

			// ------------------------------------------------------------------------------

			float bestScoreFromMouse = float.MinValue;

			foreach (var current in foodsToDisplay)
			{
				//float score = current.Score;

				Vector2 vector = current.FoodSource.DrawPos.MapToUIPosition();
				//Rect rect = new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f);
				Rect rect = new Rect(vector.x, vector.y, 200f, 200f);

				if (getter.inventory == null || !getter.inventory.innerContainer.Contains(current.FoodSource))
				{
					bool advancedInfo = ModCore.drawFoodSearchMode == ModCore.DrawFoodSearchMode.Advanced && (advancedInfoForAll || selectedFoods.Contains(current.FoodSource));
					float scoreFromMouse;
					string text = current.ToWidgetString(advancedInfo, current.FoodSource.DetermineFoodCategory(), out scoreFromMouse, -(a - current.FoodSource.Position).LengthManhattan * policy.distanceFactor);

					if (scoreFromMouse > bestScoreFromMouse)
					{
						bestScoreFromMouse = scoreFromMouse;
						bestFoodSourceFromMouse = current.FoodSource;
					}

					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect, text);
				}
			}

			// ------------------------------------------------------------------------------

			string pawninfo;
			pawninfo = string.Format("{0}", getter.NameStringShort);
			if (getter != eater)
				pawninfo += string.Format(" (gives to) {0}", eater.NameStringShort);

			pawninfo += string.Format(" ---> {4}\nFood: {1:F1}/{2:F1} {4}\nPolicy: {3}",
									  eater.NameStringShort,
									  eater.needs.food.CurLevel,
									  eater.needs.food.MaxLevel,
									  policy != null ? policy.label : "(no policy)",
			                          bestFoodSourceFromMouse != null ? bestFoodSourceFromMouse.Label : "(no reachable food)",
			                          (bestFoodSourceFromMouse != null && getter.inventory != null && getter.inventory.innerContainer.Contains(bestFoodSourceFromMouse)) ? " (inventory)" : ""
			                         );
			Text.Anchor = TextAnchor.UpperLeft;
			//TODO: spread or merge widgets +
			Widgets.Label(new Rect(a.ToUIPosition(), new Vector2(200f, 200f)), pawninfo);
		}

		// RimWorld.FoodUtility
		[DetourMethod(typeof(RimWorld.FoodUtility), "DebugFoodSearchFromMouse_Update")]
		public static void DebugFoodSearchFromMouse_Update()
		{
			try
			{
				_DebugFoodSearchFromMouse_Update();
			}
			catch (Exception ex)
			{
				DebugViewSettings.drawFoodSearchFromMouse = false;
				throw ex;
			}
		}
		static void _DebugFoodSearchFromMouse_Update()
		{
			IntVec3 root = Verse.UI.MouseCell();
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				return;
			}
			if (pawn.Map != Find.VisibleMap)
			{
				return;
			}

			if (bestFoodSource != null)
			{
				GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), bestFoodSource.Position.ToVector3Shifted());
			}
			if (bestFoodSourceFromMouse != null)
			{
				GenDraw.DrawLineBetween(root.ToVector3Shifted(), bestFoodSourceFromMouse.Position.ToVector3Shifted());
			}
		}

		// RimWorld.FoodUtility
		//[DetourMethod(typeof(RimWorld.FoodUtility),"DebugDrawPredatorFoodSource")]
		//public static void DebugDrawPredatorFoodSource()
		//{
		//	Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
		//	if (pawn == null)
		//	{
		//		return;
		//	}
		//	Thing thing;
		//	ThingDef thingDef;
		//	if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out thing, out thingDef, false, false, false, true))
		//	{
		//		GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), thing.Position.ToVector3Shifted());
		//		if (!(thing is Pawn))
		//		{
		//			Pawn pawn2 = FoodUtility.BestPawnToHuntForPredator(pawn);
		//			if (pawn2 != null)
		//			{
		//				GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn2.Position.ToVector3Shifted());
		//			}
		//		}
		//	}
		//}
	}
}