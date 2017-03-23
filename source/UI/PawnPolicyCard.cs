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
		static Vector2 middleRectSize = new Vector2(580, Config.NeedsTabUIHeight);
		static readonly float middleLeftColumnSize = 280;
		static readonly float middleRightColumnSize = middleRectSize.x - middleLeftColumnSize;

		static readonly Vector2 topRectSize = new Vector2(middleRectSize.x, 40);
		static readonly Vector2 botRectSize = new Vector2(topRectSize.x, 30);

		static Vector2 scrollposition;

		public static Vector2 Size
		{
			get
			{
				if (Spoil)
					return new Vector2(topRectSize.x, middleRectSize.y + topRectSize.y + botRectSize.y);
				else
					return new Vector2(topRectSize.x, topRectSize.y + botRectSize.y);
				//return new Vector2(topRectSize.x, botRectSize.y);
			}
		}

		internal static float middleRectHeigth
		{
			set
			{
				middleRectSize.y = value;
			}
		}

		static readonly int horizontalMargin = 10;
		static readonly float verticalMargin = 5;

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

#if DEBUG
				Log.Error(ex.Message + "\n" + ex.StackTrace);
#endif
			}
			Text.Anchor = textanchor;
			return;
		}
		private static void _Draw(Rect rect, Pawn pawn)
		{
			Policy policy = pawn.GetPolicyAssignedTo();

			Text.Anchor = TextAnchor.MiddleLeft;

			// -------------------- Top left --------------------------

			Rect topRect = new Rect(rect.position.x + horizontalMargin, rect.position.y, topRectSize.x - horizontalMargin * 2, topRectSize.y);

			Listing_Standard listing = new Listing_Standard(topRect);
			listing.ColumnWidth = topRect.width / 2;
			listing.verticalSpacing = 4;

			Text.Anchor = TextAnchor.MiddleCenter;
			//listingLeft.Label("PawnPolicyCard_Policy".Translate());

			string readable = pawn.GetPolicyAssignedTo().label;

			var flag = pawn.HasHardcodedPolicy();

			if (flag)
			{
				listing.Label(string.Format("{0} ({1})", "PawnPolicyCard_CannotSetPolicy".Translate(), policy.label));
			}
			else
			{
				if (listing.ButtonTextLabeled("PawnPolicyCard_Policy".Translate(), readable))
				{
					var floatOptions = new List<FloatMenuOption>();
					var policies = Policies.GetAllPoliciesForPawn(pawn)
#if !DEBUG
										   .Where(arg => arg.Visible)
#endif
										   ;
					foreach (var item in policies)
					{
						floatOptions.Add(new FloatMenuOption(item.label, () => WorldDataStore_PawnPolicies.SetPolicyForPawn(pawn, item), MenuOptionPriority.Default,
							 delegate
						{
							//TODO: policy tooltip
						}));
					}
					Find.WindowStack.Add(new FloatMenu(floatOptions));
				}
			}
			listing.NewColumn();

			// -------------------- Top right --------------------------

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

			listing.Label(string.Format("PawnPolicyCard_CurrentBestFood".Translate(), bestFoodInfo));

			//var mask = PawnMask.MakeCompleteMaskFromPawn(pawn);
			//listing.Label(mask.ToString());
			listing.End();

			// -------------------- Top end --------------------------

			Widgets.DrawLineHorizontal(rect.x + horizontalMargin, rect.y + topRectSize.y - verticalMargin, rect.width - horizontalMargin * 2);

			if (!Spoil)
				goto drawbot;

			// ---------------------------- Middle -------------------------------

			Rect policyRectMiddle = new Rect(rect.x + horizontalMargin, rect.y + topRectSize.y, middleRectSize.x - horizontalMargin, middleRectSize.y);

			// ---------------------------- Middle left -------------------------------

			Listing_Standard listingMiddleLeft = new Listing_Standard(policyRectMiddle.LeftPart(middleLeftColumnSize / policyRectMiddle.width));

			listingMiddleLeft.verticalSpacing = verticalMargin;

			//listingMiddleLeft.ColumnWidth = middleLeftColumnSize;

			//Rect policyRectMiddleLeft = policyRectMiddle.LeftHalf();

			Text.Font = GameFont.Small;

			//string policyDesc = "";


			//Rect policyRectMiddleLeft_inner = new Rect(0, 0, policyRectMiddleLeft.width, policyRectMiddleLeft.height * 2);
			//Widgets.BeginScrollView(policyRectMiddleLeft, ref scrollposition, policyRectMiddleLeft_inner);

			//var listingMiddleLeft_inner = new Listing_Standard(policyRectMiddleLeft);

			Text.Anchor = TextAnchor.UpperLeft;
			if (policy.description != null && policy.description.Any())
				listingMiddleLeft.Label(policy.description);
			else
				listingMiddleLeft.Label("(No description)"); //TODO: lang file

			{
				var font = Text.Font;
				Text.Font = GameFont.Tiny;
				listingMiddleLeft.Label(policy.GetDietForPawn(pawn).ToString());
				Text.Font = font;
			}

			listingMiddleLeft.End();

			//Widgets.EndScrollView();

			// ----------------------------- Middle right ----------------------------------

			var rectMiddleRight = policyRectMiddle.RightPart((middleRightColumnSize - horizontalMargin * 4) / policyRectMiddle.width);
			rectMiddleRight.x -= horizontalMargin;
			Listing_Standard listingMiddleRight = new Listing_Standard(rectMiddleRight);

			listingMiddleRight.verticalSpacing = verticalMargin;

			if (!flag)
			{
				Text.Anchor = TextAnchor.MiddleCenter;

				listingMiddleRight.verticalSpacing = verticalMargin;

				if (listingMiddleRight.ButtonText("PawnPolicyCard_ResetPolicy".Translate()))
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
					//TODO: setter for rescued pawns
					//else if (pawn.HostFaction != null && pawn.HostFaction.IsPlayer)
					//{
					//	validator = (arg) => (pawn.HostFaction != null && pawn.HostFaction.IsPlayer);
					//	targetGroupName = "guests".Translate();
					//}
					else if (pawn.Faction.IsPlayer && pawn.RaceProps.Animal)
					{
						validator = (arg) => (pawn.Faction.IsPlayer && arg.RaceProps.Animal && arg.def == pawn.def);
						targetGroupName = pawn.def.label;
					}

					if (validator != null)
					{
						if (listingMiddleRight.ButtonText(string.Format("PawnPolicyCard_AssignToAllOnMap".Translate(), targetGroupName)))
						{
							WorldDataStore_PawnPolicies.AssignToAllPawnsMatchingOnMap(policy, validator);
						}
						if (listingMiddleRight.ButtonText(string.Format("PawnPolicyCard_ResetAllOnMap".Translate(), targetGroupName)))
						{
							WorldDataStore_PawnPolicies.AssignToAllPawnsMatchingOnMap(null, validator);
						}
					}
				}
			}

			listingMiddleRight.End();

		// ----------------------------- Bottom ----------------------------------

		drawbot:

			{
				var font = Text.Font;
				var anchor = Text.Anchor;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;

				Vector2 botrectpos = new Vector2(rect.x + 0 + horizontalMargin, rect.y + topRectSize.y + verticalMargin);

				if (Spoil)
					botrectpos += new Vector2(0, middleRectSize.y);

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

