using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	internal static class Diets
	{
		//internal static PolicyAssignmentCategory GetPawnCagetory(this Pawn pawn)
		//{
		//	if (pawn.Faction == Faction.OfInsects)
		//		return PolicyAssignmentCategory.All;

		//	if (pawn.RaceProps.Animal)
		//	{
		//		if (!Config.controlPets)
		//			return PolicyAssignmentCategory.All;

		//		if (pawn.Faction == null)
		//		{
		//			if (FoodPrefUtils.isTaming)
		//			{
		//				return PolicyAssignmentCategory.Pets;
		//			}
		//		}
		//		else
		//			return PolicyAssignmentCategory.Pets;

		//		return PolicyAssignmentCategory.All;
		//	}


		//	if (pawn.IsIncapacitated())
		//	{
		//		if (Config.IncapColonistsFeedMode != IncapFeedMode.Offline)
		//		{
		//			if (Config.IncapColonistsFeedMode == IncapFeedMode.PrisonersLike)
		//				return PolicyAssignmentCategory.Prisoners;

		//			throw new NotImplementedException();
		//		}
		//	}

		//	if (pawn.IsAscetic())
		//	{
		//		return PolicyAssignmentCategory.Ascetic;
		//	}

		//	if (pawn.IsColonist)
		//	{
		//		if (!Config.controlColonists)
		//			return PolicyAssignmentCategory.All;

		//		if
				
		//		return PolicyAssignmentCategory.Colonist;
		//	}

		//	if (pawn.IsPrisoner)
		//	{
		//		if (pawn.Faction == Faction.OfPlayer)
		//		{
		//			if (!Config.controlPrisoners)
		//				return PolicyAssignmentCategory.All;

		//			if ((pawn.guest.interactionMode == PrisonerInteractionMode.AttemptRecruit && Config.privilegedPrisoners))

		//				return PolicyAssignmentCategory.Colonist;

		//			return PolicyAssignmentCategory.Prisoners;
		//		}

		//		return PolicyAssignmentCategory.All;
		//	}


		//	throw new NotImplementedException();
		//}
	}

	//public class Diet
	//{
	//	List<FoodPref> ranking;

	//	public Diet(ThingDef def)
	//	{
	//	}
	//}
	//public class FoodPref
	//{
	//	private static int n = 0;


	public enum PrisonersFeedMode
	{
		RestrictEveryone,
		ChatRecruitWellFed,
		VanillaLike
	}
	public enum IncapFeedMode
	{
		AnimalsLikeExcludeCorpses,
		AnimalsLike,
		PrisonersLike,
		Offline
	}
}
