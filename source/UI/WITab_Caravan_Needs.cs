using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WM.SmarterFoodSelection.UI
{
	public class WITab_Caravan_Needs : RimWorld.Planet.WITab_Caravan_Needs
	{
		// RimWorld.Planet.WITab_Caravan_Needs
		//[DetourMethod(typeof(RimWorld.Planet.WITab_Caravan_Needs),"ExtraOnGUI")]
		protected override void ExtraOnGUI()
		{
			//base.ExtraOnGUI();
			Log.Message("ExtraOnGUI() 0 ");

			Pawn localSpecificNeedsTabForPawn = this.specificNeedsTabForPawn;
			if (localSpecificNeedsTabForPawn != null)
			{
				Rect tabRect = base.TabRect;

				tabRect.y -= UI.PawnPolicyCard.Size.y;
				tabRect.height += UI.PawnPolicyCard.Size.y;

				float specificNeedsTabWidth = this.SpecificNeedsTabWidth;
				//Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificNeedsTabWidth, tabRect.height);

				Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificNeedsTabWidth, tabRect.height + UI.PawnPolicyCard.Size.y);

				Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
				{
					if (localSpecificNeedsTabForPawn.DestroyedOrNull())
					{
						return;
					}
					Log.Message("ExtraOnGUI() 1 " + rect);
					NeedsCardUtility.DoNeedsMoodAndThoughts(rect, localSpecificNeedsTabForPawn, ref this.thoughtScrollPosition);

					// --------- MOD --------- 

					Rect policyCardRect = new Rect(rect.x, rect.y + UI.PawnPolicyCard.Size.y, UI.PawnPolicyCard.Size.x, UI.PawnPolicyCard.Size.y);
					Log.Message("ExtraOnGUI() 2 " + rect);

					UI.PawnPolicyCard.Draw(rect, this.specificNeedsTabForPawn);

					// --------- MOD END --------- 

					if (Widgets.CloseButtonFor(rect.AtZero()))
					{
						this.specificNeedsTabForPawn = null;
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
				}, true, false, 1f);
			}
		}

		// RimWorld.Planet.WITab_Caravan_Needs
		private Pawn specificNeedsTabForPawn;

		// RimWorld.Planet.WITab_Caravan_Needs
		private float SpecificNeedsTabWidth
		{
			get
			{
				return (float)typeof(RimWorld.Planet.WITab_Caravan_Needs).GetProperty("SpecificNeedsTabWidth", AccessTools.all).GetValue(this, null);
			}
		}

		// RimWorld.Planet.WITab_Caravan_Needs
		private Vector2 thoughtScrollPosition;

	}
}

