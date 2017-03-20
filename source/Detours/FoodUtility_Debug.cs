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

		static Pawn pawnA = null;
		static Pawn pawnB = null;

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
				ModCore.drawFoodSearchMode = ModCore.DrawFoodSearchMode.Off;

				throw new Exception("DebugFoodSearchFromMouse_OnGUI() threw exception." + ex.Message + "\n" + ex.StackTrace, ex);
			}
		}

		static void _DebugFoodSearchFromMouse_OnGUI()
		{
			IntVec3 a = Verse.UI.MouseCell();
			var thingAtCursorCell = Find.VisibleMap.thingGrid.ThingsAt(a).FirstOrDefault(arg => arg.GetFoodCategory() != FoodCategory.Null);

			bestFoodSourceFromMouse = null;
			bestFoodSource = null;

			// ------------------------------------------------------------------------------

			pawnA = null;
			pawnB = null;

			var pawnSelection = Find.Selector.SelectedObjects.Where(arg => arg is Pawn).Cast<Pawn>();
			pawnA = pawnSelection.FirstOrDefault();
			if (pawnSelection.Count() == 2)
				pawnB = pawnSelection.ElementAt(1);


			if (pawnB != null && pawnA != null && (pawnA.Faction != pawnB.Faction || RimWorld.FoodUtility.ShouldBeFedBySomeone(pawnA) || RimWorld.FoodUtility.ShouldBeFedBySomeone(pawnB)))
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
				//goto cursorwidget;
				return;

			// ------------------------------------------------------------------------------

			List<FoodSourceRating> foodsToDisplay;

			var selectedFoods = Find.Selector.SelectedObjects
												.Where(arg => (arg is Thing) && (arg as Thing).DetermineFoodCategory() != FoodCategory.Null)
												.Cast<Thing>().ToList();

			FoodSearchCache.PawnEntry pawnEntry;
			//IEnumerable<Thing> foodList = 
			//TODO: no score shown with unrestricted policies
			if (getter.isWildAnimal() || policy.unrestricted || !FoodSearchCache.TryGetEntryForPawn(getter, eater, out pawnEntry, true))
				return;

			foodsToDisplay = pawnEntry.AllRankedFoods
									 .OrderBy(arg => (a - arg.FoodSource.Position).LengthManhattan).ToList()
										 .GetRange(0, Math.Min(300, pawnEntry.AllRankedFoods.Count));

			//bool advancedInfoForAll;

			//if (!selectedFoods.Any())
			//{
			//	selectedFoods.Add(bestFoodSource);
			//	selectedFoods.Add(foodList.AllRankedFoods.MinBy((arg) => (a - arg.FoodSource.Position).LengthHorizontal).FoodSource);
			//	advancedInfoForAll = false;
			//}
			//else
			//	advancedInfoForAll = true;

			bool advancedInfoForAll = ModCore.drawFoodSearchMode == ModCore.DrawFoodSearchMode.Advanced;

			// ----------------------- Recalculate food source rating distance factors ------------------------------

			FoodSourceRating bestScoreFromMouse = null;

			for (int i = 0; i < foodsToDisplay.Count; i++)
			{
				var current = foodsToDisplay[i];

				foodsToDisplay[i] = new FoodSourceRating(foodsToDisplay[i]);

				foodsToDisplay[i].SetComp("Distance", -(a - current.FoodSource.Position).LengthManhattan * policy.distanceFactor);
			}

			//bestScoreFromMouse = foodsToDisplay.MaxBy((arg) => arg.Score);

			bestScoreFromMouse = foodsToDisplay.MaxBy((arg) => arg.ScoreForceSum);
			bestFoodSourceFromMouse = bestScoreFromMouse.FoodSource;

			// ----------------------- Food sources widgets ------------------------------

			foreach (var current in foodsToDisplay)
			{
				//float score = current.Score;

				Vector2 vector = current.FoodSource.DrawPos.MapToUIPosition();
				//Rect rect = new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f);
				Rect rect = new Rect(vector.x, vector.y, 200f, 200f);

				if (getter.inventory == null || !getter.inventory.innerContainer.Contains(current.FoodSource))
				{
					bool advancedInfo = (advancedInfoForAll);

					Color widgetColor;
					if (current == bestScoreFromMouse || current.FoodSource == bestFoodSource || current.FoodSource == thingAtCursorCell)
					{
						if (ModCore.drawFoodSearchMode == ModCore.DrawFoodSearchMode.AdvancedForBest)
							advancedInfo = true;
						if (current.FoodSource == thingAtCursorCell)
							widgetColor = Color.green;
						else
							widgetColor = Resources.Color.Orange;
					}
					else
					{
						widgetColor = Color.white;
					}

					string text = current.ToWidgetString(advancedInfo, current.FoodSource.DetermineFoodCategory());

#if DEBUG
					if (current.FoodSource is Pawn)
					{
						text += "\n" + "ratio1=" + FoodUtility.GetPreyRatio1(getter, current.FoodSource as Pawn);
						text += "\n" + "ratio2=" + FoodUtility.GetPreyRatio2(getter, current.FoodSource as Pawn);
					}
#endif

					Text.Anchor = TextAnchor.UpperLeft;
					{
						var backup = GUI.color;
						GUI.color = widgetColor;
						Widgets.Label(rect, text);
						GUI.color = backup;
					}
				}
			}

			// ----------------------- Cursor widget -----------------------

			//TODO: cursor widget not drawn when no food found 

			string pawninfo;
			pawninfo = string.Format("{0}", getter.NameStringShort);
			if (getter != eater)
			{
				pawninfo += string.Format(" (gives to) {0}", eater.NameStringShort);
			}

			//pawninfo += string.Format(" ---> {4} ({6:F0}){5}\nFood need: {1:F1} / {2:F1}\nPolicy: {3}",
			//						  eater.NameStringShort,
			//eater.needs.food.CurLevel,
			//eater.needs.food.MaxLevel,
			//policy != null ? policy.label : "(no policy)",
			//bestFoodSourceFromMouse != null ? bestFoodSourceFromMouse.Label : "(no reachable food)",
			//(bestFoodSourceFromMouse != null && getter.inventory != null && getter.inventory.innerContainer.Contains(bestFoodSourceFromMouse)) ? " (inventory)" : "",
			//bestScoreFromMouse.Score);


			{
				string score;
				string foodname;

				if (bestFoodSourceFromMouse != null)
				{
					foodname = bestFoodSourceFromMouse.Label;
					score = bestScoreFromMouse.ScoreForceSum.ToString("F0");
				}
				else
				{
					foodname = "(no reachable food)";
					score = "";
				}

				pawninfo += " ---> " + foodname + " (" + score + ")" + "\n";
				pawninfo += "Food need: " + eater.needs.food.CurLevel.ToString("F1") + " / " + eater.needs.food.MaxLevel.ToString("F1") + "\n";
				pawninfo += "Policy: " + (policy != null ? policy.label : "(no policy)") + "\n";
			}

			Text.Anchor = TextAnchor.UpperLeft;
			//TODO: spread or merge widgets +
			Widgets.Label(new Rect((a.ToUIPosition() + new Vector2(-100f, -80f)), new Vector2(200f, 200f)), pawninfo);
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
				ModCore.drawFoodSearchMode = ModCore.DrawFoodSearchMode.Off;
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
			{
				//TODO: fix broken lines
				if (bestFoodSource != null)
				{
					GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), bestFoodSource.Position.ToVector3Shifted(), SimpleColor.Yellow);
				}
				if (bestFoodSourceFromMouse != null)
				{
					GenDraw.DrawLineBetween(root.ToVector3Shifted(), bestFoodSourceFromMouse.Position.ToVector3Shifted(), SimpleColor.Yellow);
				}
				if (pawnB != null && null != pawnA && pawnA != pawnB)
				{
					GenDraw.DrawLineBetween(pawnA.Position.ToVector3Shifted(), pawnB.Position.ToVector3Shifted(), SimpleColor.Green);
				}
			}

			//duck tape
			try
			{
				if (getter.playerSettings != null && getter.playerSettings.AreaRestrictionInPawnCurrentMap != null)
				{
					GenDraw.DrawFieldEdges(getter.playerSettings.AreaRestrictionInPawnCurrentMap.ActiveCells.ToList(), Color.red);
				}
			}
			catch (Exception)
			{
#if DEBUG
				Log.Warning("DrawFieldEdges() failed");
#endif
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