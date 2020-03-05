using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Collections.Generic;

namespace WM.SmarterFoodSelection.UI
{
	// Obsolete

	public class ITab_Pawn_Needs : RimWorld.ITab_Pawn_Needs
	{
		private Rect policyCardRect;

		public ITab_Pawn_Needs()
		{
			//this.labelKey = "WM_TabFood";
			//this.size.x = Math.Max(PawnPolicyCard.Size.x, this.size.x);
			//this.size.y = PawnPolicyCard.RectSize.y + RimWorld.NeedsCardUtility.GetSize(SelPawn).y;
		}

		//[DetourMethod(typeof(ITab_Pawn_Needs), "FillTab")]
		protected override void FillTab()
		{
			NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0, 0, this.size.x, RimWorld.NeedsCardUtility.GetSize(SelPawn).y), base.SelPawn, ref this.thoughtScrollPosition);
			//base.FillTab();

			// ------ MOD -----------

			if (SelPawn.CanHaveFoodPolicy())
			{
				policyCardRect = new Rect(0, this.size.y - PawnPolicyCard.Size.y, PawnPolicyCard.Size.x, PawnPolicyCard.Size.y);
				//policyRect = new Rect(0, 0, PawnPolicyCard.RectSize.x, PawnPolicyCard.RectSize.y);
				PawnPolicyCard.Draw(policyCardRect, SelPawn);
			}

			// ------ MOD END -----------
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();

			if (SelPawn.CanHaveFoodPolicy())
			{
				this.size.x = Math.Max(this.size.x, PawnPolicyCard.Size.x);
				this.size.y += PawnPolicyCard.Size.y;
			}
		}

		//TODO: fix incorrect reference
		private Vector2 thoughtScrollPosition;
	}
}
