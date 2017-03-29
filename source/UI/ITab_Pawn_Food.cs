using System;
using RimWorld;
using UnityEngine;

namespace WM.SmarterFoodSelection.UI
{
	public class ITab_Pawn_Food : ITab
	{
		public ITab_Pawn_Food()
		{
			this.labelKey = "FoodITabLabel";
		}

		public override bool IsVisible
		{
			get
			{
				return SelPawn.CanHaveFoodPolicy();
			}
		}
		protected override void UpdateSize()
		{
			this.size = PawnPolicyCard.Size;
		}
		protected override void FillTab()
		{
			PawnPolicyCard.Draw(new Rect(Vector2.zero,this.size), this.SelPawn);
		}

	}
}
