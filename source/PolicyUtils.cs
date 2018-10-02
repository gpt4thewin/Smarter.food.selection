using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class PolicyUtils
	{
		internal static Policy GetPolicyAssignedTo(this Pawn eater, Pawn getter = null)
		{
#if DEBUG
			if (Config.debugNoPawnsRestricted)
				return Policies.Unrestricted;
#endif
			if (getter == null)
				getter = eater;

            Policy policy = GetHardcodedPolicy(eater, getter);

			if (policy != null)
				return policy;

			return WorldDataStore_PawnPolicies.GetPawnEntry(eater);

			//nopolicy:

			//	Log.Warning("Found no active policy for " + eater + ". Using default policy.");
			//	return Policies.Unrestricted;
		}

		public static bool CanHaveFoodPolicy(this Pawn pawn)
		{
			//if (HasHardcodedPolicy(pawn))
			//	return true;

			if (pawn.needs == null || !pawn.RaceProps.EatsFood || pawn.isWildAnimal() || pawn.isInsectFaction())
				return false;

            //if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
            //	return false;

            //after pawn.isWildAnimal() the only null case should be space refugee
            if (pawn.Faction == null)
            {
                return true;
            }
            if (pawn.Faction != null)
			{
				if (pawn.HostFaction == Faction.OfPlayer)
					return true;
				if (pawn.Faction.HostileTo(Faction.OfPlayer))
					return false;
			}


			if (pawn.needs == null || pawn.needs.food == null)
				return false;

			return true;
		}

		internal static Policy GetHardcodedPolicy(this Pawn eater, Pawn getter = null)
		{
			if (eater.isWildAnimal())
			{
				if (getter != null && getter.IsColonist)
					return Policies.Taming;
				else
					return Policies.Wild;
			}

            // Should only occur if eater is unfactioned space refugee or WildMan | prevents error with eater.Faction.IsPlayer in next if statement
            if (eater.Faction == null)
            {
                if (eater.KindLabel == "space refugee" ||
                    ((eater.KindLabel == "wild man" || eater.KindLabel == "wild woman") &&
                        eater.mindState.lastJobTag == Verse.AI.JobTag.TuckedIntoBed))
                    return Policies.Friendly;
                else
                    return null;
            }
            if (!eater.Faction.IsPlayer && eater.Faction.RelationWith(Faction.OfPlayer).kind != FactionRelationKind.Hostile && !eater.IsPrisonerOfColony)
			{
				if (eater.RaceProps.Animal)
					return Policies.FriendlyPets;
				else
					return Policies.Friendly;
			}

			return null;
		}
		internal static bool HasHardcodedPolicy(this Pawn pawn)
		{
			return pawn.GetHardcodedPolicy() != null;
		}
		internal static IEnumerable<PawnMask> GetAllMasksFor(this Pawn pawn)
		{
			return Policies.AllPawnMasks.Where((PawnMask arg) => arg.MatchesPawn(pawn));
		}
	}
}
