using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection.PawnMasks
{
	public class Colonist : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.IsColonist);
		}	}

	public class Prisoner : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.IsPrisonerOfColony);
		}
	}

	public class AppealedPrisoner : Prisoner
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (base.MatchesPawn(pawn) &&
				   pawn.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit);
		}
	}

	public class Ascetic : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.IsAscetic());
		}
	}

	public class Incapacitated : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.RaceProps.Humanlike && pawn.IsIncapacitated());
		}
	}

	public class ColonyAnimal : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer);
		}
	}

	public class Guest : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.HostFaction == Faction.OfPlayer && !pawn.IsPrisonerOfColony && pawn.RaceProps.Humanlike);
		}
	}

	public class GuestWildAnimal : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.HostFaction == Faction.OfPlayer && !pawn.IsPrisonerOfColony && pawn.RaceProps.Animal);
		}
	}

	public class WildAnimal : PawnMask
	{
		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.isWildAnimal());
		}
	}

	public class Friendly : PawnMask
	{
		public override Policy RiggedPolicy
		{
			get
			{
				return (Policies.Friendly);
			}
		}

		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.isFriendly() && pawn.RaceProps.Humanlike);
		}
	}

	public class FriendlyAnimal : PawnMask
	{
		public override Policy RiggedPolicy
		{
			get
			{
				return (Policies.FriendlyPets);
			}
		}

		public override bool MatchesPawn(Pawn pawn)
		{
			return (pawn.isFriendly() && pawn.RaceProps.Animal);
		}
	}
}
