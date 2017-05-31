using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WM.SmarterFoodSelection.Detours.FoodUtility
{
	[HarmonyPatch(typeof(RimWorld.FoodUtility), "DebugFoodSearchFromMouse_OnGUI")]
	public static class DebugFoodSearchFromMouse_OnGUI
	{
		static Thing bestFoodSource;

		static Pawn eater;
		static Pawn getter;

		internal static Thing BestFoodSource
		{
			get;
			private set;
		}

		internal static Thing BestFoodSourceFromMouse
		{
			get;
			private set;
		}

		public static Pawn PawnB
		{
			get;
			private set;
		}

		public static Pawn PawnA
		{
			get;
			private set;
		}

		public static Pawn Eater
		{
			get;
			private set;
		}

		public static Pawn Getter
		{
			get;
			private set;
		}

		[HarmonyPrefix]
		public static bool Prefix()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Postfix()
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

			BestFoodSourceFromMouse = null;
			BestFoodSource = null;

			// ------------------------------------------------------------------------------

			PawnA = null;
			PawnB = null;

			var pawnSelection = Find.Selector.SelectedObjects.Where(arg => arg is Pawn).Cast<Pawn>();
			PawnA = pawnSelection.FirstOrDefault();
			if (pawnSelection.Count() == 2)
				PawnB = pawnSelection.ElementAt(1);


			if (PawnB != null && PawnA != null && (PawnA.Faction != PawnB.Faction || RimWorld.FoodUtility.ShouldBeFedBySomeone(PawnA) || RimWorld.FoodUtility.ShouldBeFedBySomeone(PawnB)))
			{
				//TODO: watch out for compatibility
				if (PawnA.IsColonist)
				{
					eater = PawnB;
					getter = PawnA;
				}
				else if (PawnB.IsColonist)
				{
					eater = PawnA;
					getter = PawnB;
				}
				else
					eater = getter = PawnA;
			}
			else
				eater = getter = PawnA;


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

			var thingAtCursorCell = Find.VisibleMap.thingGrid.ThingsAt(a).FirstOrDefault(delegate (Thing t)
			{
				var category = t.GetFoodCategory();
				if (category == FoodCategory.Null)
					return false;

				return FoodUtils.IsValidFoodSourceForPawn(t, eater, getter, policy, false);
			} );

			ThingDef dum2;
			// Generates cache
			var foundFood = RimWorld.FoodUtility.TryFindBestFoodSourceFor(getter, eater, true, out bestFoodSource, out dum2);

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
			BestFoodSourceFromMouse = bestScoreFromMouse.FoodSource;

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
					if (current == bestScoreFromMouse || current.FoodSource == BestFoodSource || current.FoodSource == thingAtCursorCell)
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
						text += "\n" + "ratio1=" + FoodUtils.GetPreyRatio1(getter, current.FoodSource as Pawn);
						text += "\n" + "ratio2=" + FoodUtils.GetPreyRatio2(getter, current.FoodSource as Pawn);
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

				if (BestFoodSourceFromMouse != null)
				{
					foodname = BestFoodSourceFromMouse.Label;
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
	}

	[HarmonyPatch(typeof(RimWorld.FoodUtility), "DebugFoodSearchFromMouse_Update")]
	public static class DebugFoodSearchFromMouse_Update
	{
		[HarmonyPrefix]
		public static bool Prefix()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Postfix()
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
				if (DebugFoodSearchFromMouse_OnGUI.BestFoodSource != null)
				{
					GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), DebugFoodSearchFromMouse_OnGUI.BestFoodSource.Position.ToVector3Shifted(), SimpleColor.Yellow);
				}
				if (DebugFoodSearchFromMouse_OnGUI.BestFoodSourceFromMouse != null)
				{
					GenDraw.DrawLineBetween(root.ToVector3Shifted(), DebugFoodSearchFromMouse_OnGUI.BestFoodSourceFromMouse.Position.ToVector3Shifted(), SimpleColor.Yellow);
				}
				if (DebugFoodSearchFromMouse_OnGUI.PawnB != null && null != DebugFoodSearchFromMouse_OnGUI.PawnA && DebugFoodSearchFromMouse_OnGUI.PawnA != DebugFoodSearchFromMouse_OnGUI.PawnB)
				{
					GenDraw.DrawLineBetween(DebugFoodSearchFromMouse_OnGUI.PawnA.Position.ToVector3Shifted(), DebugFoodSearchFromMouse_OnGUI.PawnB.Position.ToVector3Shifted(), SimpleColor.Green);
				}
			}

			//duck tape
			try
			{
				if (DebugFoodSearchFromMouse_OnGUI.Getter.playerSettings != null && DebugFoodSearchFromMouse_OnGUI.Getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null)
				{
					GenDraw.DrawFieldEdges(DebugFoodSearchFromMouse_OnGUI.Getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells.ToList(), Color.red);
				}
			}
			catch (Exception)
			{
#if DEBUG
				Log.Warning("DrawFieldEdges() failed");
#endif
			}
		}
	}
}