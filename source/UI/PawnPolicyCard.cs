using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace WM.SmarterFoodSelection.UI
{
	//TODO: card for caravans
	public static class PawnPolicyCard
	{
		static readonly Vector2 leftRectSize = new Vector2(300, 180);
		static readonly Vector2 rightRectSize = new Vector2(280, leftRectSize.y);
		static readonly Vector2 topRectSize = new Vector2(leftRectSize.x + rightRectSize.x, 75);
		static readonly Vector2 botRectSize = new Vector2(topRectSize.x, 60);

		public static Vector2 Size
		{
			get
			{
				if (Spoil)
					return new Vector2(topRectSize.x, Math.Max(leftRectSize.y, rightRectSize.y) + topRectSize.y + botRectSize.y);
				else
					return new Vector2(topRectSize.x, topRectSize.y + botRectSize.y);
			}
		}

		static readonly int horizontalMargin = 10;
		static readonly float verticalMargin = 8;

		static bool Spoil;

		public static void Draw(Rect rect, Pawn pawn)
		{
			var textanchor = Text.Anchor;
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
			Text.Anchor = textanchor;
			return;
		}
		private static void _Draw(Rect rect, Pawn pawn)
		{
			Policy policy = pawn.GetPolicyAssignedTo();

			Text.Anchor = TextAnchor.MiddleLeft;

			// -------------------- Top --------------------------

			var mask = PawnMask.MakeCompleteMaskFromPawn(pawn);

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
					bestFoodInfo += " " + "PawnPolicyCard_Inventory".Translate();
			}
			else
			{
				bestFoodInfo = "PawnPolicyCard_NoFoodFound".Translate();
			}

			listing.GapLine();
			//listing.GapLine(20);
			listing.Label(string.Format("PawnPolicyCard_CurrentBestFood".Translate(), bestFoodInfo));
			listing.Label(mask.ToString());
			listing.GapLine();
			listing.End();

			if (!Spoil)
				goto drawbot;

			// ---------------------------- Bottom left -------------------------------

			Rect policyRectLeft = new Rect(rect.x + horizontalMargin, rect.y + topRect.height, leftRectSize.x - horizontalMargin, leftRectSize.y);

			Text.Font = GameFont.Small;
			//Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f).ContractedBy(10f);
			Listing_Standard listingLeft = new Listing_Standard(policyRectLeft);
			listingLeft.verticalSpacing = verticalMargin;

			Text.Anchor = TextAnchor.MiddleCenter;
			//listingLeft.Label("PawnPolicyCard_Policy".Translate());

			string readable = pawn.GetPolicyAssignedTo().label;

			var flag = pawn.HasHardcodedPolicy();

			if (flag)
			{
				listingLeft.Label("PawnPolicyCard_CannotSetPolicy".Translate());
				listingLeft.Label(policy.label);
			}
			else
			{
				if (listingLeft.ButtonTextLabeled("PawnPolicyCard_Policy".Translate(), readable))
				{
					var floatOptions = new List<FloatMenuOption>();
					var policies = Policies.GetAllPoliciesForPawn(pawn)
#if !DEBUG
										   .Where(arg => arg.Visible)
#endif
										   ;
					foreach (var item in policies)
					{
						floatOptions.Add(new FloatMenuOption(item.label, () => WorldDataStore_PawnPolicies.SetPolicyForPawn(pawn, item)));
					}
					Find.WindowStack.Add(new FloatMenu(floatOptions));
				}
			}

			Text.Anchor = TextAnchor.UpperLeft;
			if (policy.description != null && policy.description.Any())
				listingLeft.Label(policy.description);
			else
				listingLeft.Label("(No description)");

			{
				var font = Text.Font;
				Text.Font = GameFont.Tiny;
				listingLeft.Label(policy.GetDietForPawn(pawn).ToString());
				Text.Font = font;
			}


			listingLeft.End();

			// ----------------------------- Bottom right ----------------------------------

			if (!flag)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect policyRectRight = new Rect(rect.x + leftRectSize.x + horizontalMargin, rect.y + topRect.height, rightRectSize.x - horizontalMargin * 2, rightRectSize.y);

				Listing_Standard listingRight = new Listing_Standard(policyRectRight);
				listingRight.verticalSpacing = verticalMargin;


				if (listingRight.ButtonText("PawnPolicyCard_ResetPolicy".Translate()))
				{
					WorldDataStore_PawnPolicies.SetPolicyForPawn(pawn, null);
				}
				//if (listingRight.ButtonText(string.Format("PawnPolicyCard_AssignToAll".Translate(), pawn.def.label)))
				//{
				//	WorldDataStore_PawnPolicies.AssignToAllPawnsOfRaces(policy,pawn.def);
				//}
				{
					string targetGroupName = "";
					Func<Pawn, bool> validator = null;

					if (pawn.IsColonist)
					{
						validator = (arg) => arg.IsColonist;
						targetGroupName = "colonists".Translate();
					}
					else if (pawn.IsPrisonerOfColony)
					{
						validator = (arg) => arg.IsPrisonerOfColony;
						targetGroupName = "prisoners".Translate();
					}
					else if (pawn.Faction.IsPlayer && pawn.RaceProps.Animal)
					{
						validator = (arg) => (pawn.Faction.IsPlayer && arg.RaceProps.Animal && arg.def == pawn.def);
						targetGroupName = pawn.def.label;
					}

					if (validator != null)
					{
						if (listingRight.ButtonText(string.Format("PawnPolicyCard_AssignToAllOnMap".Translate(), targetGroupName)))
						{
							WorldDataStore_PawnPolicies.AssignToAllPawnsMatchingOnMap(policy, validator);
						}
						if (listingRight.ButtonText(string.Format("PawnPolicyCard_ResetAllOnMap".Translate(), targetGroupName)))
						{
							WorldDataStore_PawnPolicies.AssignToAllPawnsMatchingOnMap(null, validator);
						}
					}
				}

				listingRight.End();

			}
		// ----------------------------- Bottom ----------------------------------

		drawbot:

			{
				var font = Text.Font;
				var anchor = Text.Anchor;
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;

				Vector2 botrectpos = new Vector2(rect.x + 0 + horizontalMargin, rect.y + topRectSize.y + 0 + verticalMargin);

				if (Spoil)
					botrectpos += new Vector2(0, rightRectSize.y);

				Rect botrect = new Rect(botrectpos, new Vector2(botRectSize.x - horizontalMargin * 2, botRectSize.y - verticalMargin * 2));

				if (Widgets.ButtonText(botrect, "PawnPolicyCard_Title".Translate()))
				{
					Spoil ^= true;
				}
				Text.Font = font;
				Text.Anchor = anchor;
			}
		}
	}
}

