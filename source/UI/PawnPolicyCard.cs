using System;
using HugsLib.Source.Detour;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Collections.Generic;

namespace WM.SmarterFoodSelection.UI
{
	//TODO: card for caravans
	public static class PawnPolicyCard
	{
		static readonly Vector2 leftRectSize = new Vector2(180, 150);
		static readonly Vector2 rightRectSize = new Vector2(220, leftRectSize.y);
		static readonly Vector2 topRectSize = new Vector2(leftRectSize.x + rightRectSize.y, 60);

		public static readonly Vector2 Size = new Vector2(leftRectSize.x + rightRectSize.x, Math.Max(leftRectSize.y, rightRectSize.y) + topRectSize.y);

		static readonly int horizontalMargin = 10;
		static readonly float verticalMargin = 8;

		public static void Draw(Rect rect, Pawn pawn)
		{
			try
			{
				_Draw(rect, pawn);
			}
			catch (Exception ex)
			{
				Listing_Standard listing = new Listing_Standard(rect);
				listing.Label(ex.Message);
				listing.Label(ex.StackTrace);
				listing.End();
			}
			return;
		}
		private static void _Draw(Rect rect, Pawn pawn)
		{
			Policy policy = pawn.GetPolicyAssignedTo();

			Text.Anchor = TextAnchor.MiddleLeft;

			// ---------------------------------------------------------------

			var mask = PawnMask.MakeCompleteMask(pawn);

			Rect topRect = new Rect(rect.position.x + horizontalMargin, rect.position.y, topRectSize.x - horizontalMargin * 2, topRectSize.y);

			Listing_Standard listing = new Listing_Standard(topRect);
			listing.verticalSpacing = 4;

			Thing bestFood;
			ThingDef bestFoodDef;

			string bestFoodInfo;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out bestFood, out bestFoodDef))
			{
				bestFoodInfo = bestFood.Label;
				if (pawn.inventory != null && pawn.inventory.innerContainer.Contains(bestFood))
					bestFoodInfo += " "+"PawnPolicyCard_Inventory".Translate();
			}
			else
			{
				bestFoodInfo = "PawnPolicyCard_NoFoodFound".Translate();
			}

			listing.GapLine(20);
			listing.Label(string.Format("PawnPolicyCard_CurrentBestFood".Translate(), bestFoodInfo));
			listing.Label(mask.ToString());
			listing.End();


			// ---------------------------------------------------------------

			Rect policyRectLeft = new Rect(rect.x + horizontalMargin, rect.y + topRect.height, leftRectSize.x - horizontalMargin, leftRectSize.y);

			Text.Font = GameFont.Small;
			//Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f).ContractedBy(10f);
			Listing_Standard listingLeft = new Listing_Standard(policyRectLeft);
			listingLeft.verticalSpacing = verticalMargin;

			listing.GapLine(8);

			Text.Anchor = TextAnchor.MiddleCenter;
			listingLeft.Label("PawnPolicyCard_Policy".Translate());

			string readable = pawn.GetPolicyAssignedTo().label;

			var flag = pawn.HasHardcodedPolicy();

			if (flag)
			{
				listingLeft.Label(policy.label);
			}
			else
			{
				if (listingLeft.ButtonText(readable))
				{
					var floatOptions = new List<FloatMenuOption>();
					var policies = Policies.GetAllPoliciesForPawn(pawn);
					foreach (var item in policies)
					{
						floatOptions.Add(new FloatMenuOption(item.label, () => WorldDataStore_PawnPolicies.SetPolicyForPawn(pawn, item)));
					}
					Find.WindowStack.Add(new FloatMenu(floatOptions));
				}
			}

			listingLeft.End();

			// ---------------------------------------------------------------

			if (!flag)
			{
				Rect policyRectRight = new Rect(rect.x + leftRectSize.x + horizontalMargin, rect.y + topRect.height, rightRectSize.x - horizontalMargin * 2, rightRectSize.y);

				Listing_Standard listingRight = new Listing_Standard(policyRectRight);
				listingRight.verticalSpacing = verticalMargin;

				if (listingRight.ButtonText(string.Format("PawnPolicyCard_AssignToAll".Translate(), pawn.def.label)))
				{
					WorldDataStore_PawnPolicies.AssignToAllPawnsOfRaces(policy);
				}
				if (listingRight.ButtonText(string.Format("PawnPolicyCard_AssignToAll".Translate(), mask.ToString())))
				{
					WorldDataStore_PawnPolicies.AssignToAllPawnsWithMask(mask);
				}
				if (listingRight.ButtonText(string.Format("PawnPolicyCard_AssignToAllOnMap".Translate(), pawn.def.label)))
				{
					WorldDataStore_PawnPolicies.AssignToAllPawnsOfRacesOnMap(policy);
				}

				listingRight.End();
			}

		}
	}
}
