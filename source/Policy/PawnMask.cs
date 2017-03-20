using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	public enum PawnMaskFaction
	{
		Colonist,
		Friendly,
		Prisoner,
		Wild,
		Hostile
	}
	public enum PawnMaskType
	{
		Human,
		Pet,
		WildAnimal
	}
	public class PawnMask
	{
		internal Policy targetDefault = Policies.Unrestricted;

		internal IEnumerable<MaskAttribute> AllSpecifiedAttributes()
		{
			return Attributes.Where((MaskAttribute arg) => !arg.Unspecified);
		}

		// parent mask = more abstract mask
		public PawnMask Parent
		{
			get;
			internal set;
		}
		public List<PawnMask> Children = new List<PawnMask>();

		internal IEnumerable<Policy> RelatedPolicies
		{
			get
			{
				return Policies.AllPolicies.Where((Policy arg) => arg.pawnMasks.Any((obj) => obj.CanBeParentOrEqualOf(this)));
			}
		}

		// I did not want to use generic type for a reason...

		internal readonly MaskAttribute[] Attributes =
		{
			new MaskAttribute(PawnMaskType.WildAnimal),
			new MaskAttribute(PawnMaskFaction.Wild),
			new MaskAttribute("AppealedPrisoner", new bool()),
			new MaskAttribute("Cannibal", new bool()),
			new MaskAttribute("Ascetic", new bool()),
			new MaskAttribute("Incapacitated", new bool())
		};

		internal MaskAttribute pawnType
		{
			get { return Attributes[0]; }
			set { Attributes[0] = value; }
		}
		internal MaskAttribute factionCategory
		{
			get { return Attributes[1]; }
			set { Attributes[1] = value; }
		}
		internal MaskAttribute appealedPrisoner
		{
			get { return Attributes[2]; }
			set { Attributes[2] = value; }
		}
		internal MaskAttribute cannibal
		{
			get { return Attributes[3]; }
			set { Attributes[3] = value; }
		}
		internal MaskAttribute ascetic
		{
			get { return Attributes[4]; }
			set { Attributes[4] = value; }
		}
		internal MaskAttribute incapacitated
		{
			get { return Attributes[5]; }
			set { Attributes[5] = value; }
		}

		internal bool Humanlike
		{
			get
			{
				return ((PawnMaskType)pawnType.Value) == PawnMaskType.Human;
			}
		}
		internal bool Pet
		{
			get
			{
				return ((PawnMaskType)pawnType.Value) == PawnMaskType.Pet;
			}
		}

		public bool MatchesPawnDef(ThingDef def)
		{
			if (def.race.Humanlike && !Humanlike)
				return false;

			if (def.race.Animal && !Pet)
				return false;

			return true;
		}
		public bool MatchesPawn(Pawn pawn)
		{
			PawnMaskFaction factionCategory;
			PawnMaskType pawnType;

			try
			{
				if (pawn.IsPrisonerOfColony && pawn.Faction != Faction.OfPlayer)
					factionCategory = PawnMaskFaction.Prisoner;
				else if (pawn.IsColonist || pawn.Faction.IsPlayer)
					factionCategory = PawnMaskFaction.Colonist;
				else if ((pawn.HostFaction != null && pawn.HostFaction.IsPlayer) ||
						 (pawn.Faction != null && !pawn.Faction.RelationWith(Faction.OfPlayer).hostile))
					factionCategory = PawnMaskFaction.Friendly;
				else if (pawn.Faction == null)
					factionCategory = PawnMaskFaction.Wild;
				else if (pawn.Faction.HostileTo(Faction.OfPlayer))
					factionCategory = PawnMaskFaction.Hostile;
				else
					throw new Exception("Unknown faction category");
			}
			catch (Exception ex)
			{
				throw new Exception("Could not determine pawn faction category of " + pawn, ex);
			}

			if (!this.factionCategory.Matches(factionCategory))
				return false;

			try
			{
				if (pawn.RaceProps.Humanlike)
					pawnType = PawnMaskType.Human;
				else if (pawn.RaceProps.Animal && pawn.Faction != null)
					pawnType = PawnMaskType.Pet;
				else if (pawn.Faction == null)
					pawnType = PawnMaskType.WildAnimal;
				else
					throw new Exception("Unknown pawn type");
			}
			catch (Exception ex)
			{
				throw new Exception("Could not determine pawn type category of " + pawn, ex);
			}

			if (!this.pawnType.Matches(pawnType))
				return false;

			if (!appealedPrisoner.Matches(pawn.guest != null && pawn.IsPrisonerOfColony && pawn.guest.interactionMode == PrisonerInteractionMode.AttemptRecruit))
				return false;

			//if (pawn.story != null)
			if (pawn.RaceProps.Humanlike)
			{
				if (!cannibal.Matches(pawn.IsCannibal()))
					return false;

				if (!ascetic.Matches(pawn.IsAscetic()))
					return false;
			}

			if (!incapacitated.Matches(pawn.IsIncapacitated()))
				return false;

			return true;
		}

		public bool CanBeParentOrEqualOf(PawnMask mask)
#if DEBUG
		{
			var result = _CanBeParentOf(mask);
			//Log.Message(string.Format("CanBeParentOf() {0} | {1} = {2}", this, mask, result));
			return result;
		}
		public bool _CanBeParentOf(PawnMask mask)
#endif
		{
			//var list = Attributes.Select((arg1, arg2) => new { source = arg1, index = arg2 });
			//var specifiedList = list.Where((arg) => !arg.source.Unspecified);

			//var text = "";
			//foreach (var current in specifiedList)
			//{
			//	text += "\n" + current.source.Value.ToString() + "=" + mask.Attributes[current.index].Value.ToString();
			//}

			//Log.Message("CanBeParentOf() :" + text);

			//if (specifiedList.Any((arg) => mask.Attributes[arg.index].Unspecified))
			//	return false;

			//return specifiedList.All((arg) => mask.Attributes[arg.index].Value == arg.source.Value);

			for (int i = 0; i < Attributes.Length; i++)
			{
				var parent = Attributes[i];
				var child = mask.Attributes[i];

				if (!parent.Matches(child.Value))
				{
					return false;
				}
			}

			return true;

			//Attributes.Where((MaskAttribute arg) => !arg.Unspecified).All((MaskAttribute arg) => arg.);
		}

		internal static PawnMask Parse(string syntax, Policy policy)
		{
			var commands = syntax.Split(':');
			var maskSyntax = commands.Last();
			var maskFlags = maskSyntax.Split('/');

			PawnMask mask = new PawnMask();

			foreach (var flagName in maskFlags)
			{
				var attribute = mask.Attributes.FirstOrDefault((arg) => arg.GetValueNamed(flagName) != null);

				if (attribute != null)
				{
					attribute.SetValueNamed(flagName);
				}
				else
				{
					goto error;
				}
			}

			if (commands.Any((arg) => arg.CaseUnsensitiveCompare("default")))
			{
				mask.targetDefault = policy;
			}

			if (!mask.factionCategory.Unspecified && ((PawnMaskFaction)mask.factionCategory.Value == PawnMaskFaction.Colonist ||
											  (PawnMaskFaction)mask.factionCategory.Value == PawnMaskFaction.Prisoner)
				||
				(!mask.ascetic.Unspecified && (bool)mask.ascetic.Value) ||
				(!mask.cannibal.Unspecified && (bool)mask.cannibal.Value))
			{
				mask.pawnType.Value = PawnMaskType.Human;
			}

			return mask;
		error:
			throw new Exception("Wrong mask format: " + syntax);
		}

		public static PawnMask MakeCompleteMaskFromPawn(Pawn pawn)
		{
			var mask = new PawnMask();

			if (pawn.IsAscetic())
				mask.ascetic.Value = true;

			if (pawn.IsCannibal())
				mask.cannibal.Value = true;

			if (pawn.IsIncapacitated())
				mask.incapacitated.Value = true;

			if (pawn.IsPrisonerOfColony)
				mask.factionCategory.Value = PawnMaskFaction.Prisoner;
			else if (!pawn.Faction.IsPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer))
				mask.factionCategory.Value = PawnMaskFaction.Friendly;
			else if (pawn.IsColonist || pawn.Faction.IsPlayer)
				mask.factionCategory.Value = PawnMaskFaction.Colonist;
			else
			{
				goto invalidpawn;
			}

			if (pawn.RaceProps.Humanlike)
				mask.factionCategory.Value = PawnMaskType.Human;
			else if (pawn.RaceProps.Animal)
			{
				if (pawn.Faction != null && !pawn.isWildAnimal())
					mask.factionCategory.Value = PawnMaskType.Pet;
				else
					goto invalidpawn;
			}

			return mask;

		invalidpawn:
			throw new Exception("Trying to create a mask for an invalid pawn: " + pawn);
		}

		public override string ToString()
		{
			var specifiedFieldsValue = this.AllSpecifiedAttributes();

			if (!specifiedFieldsValue.Any())
				return "(no attributes specified)";

			return string.Join(" - ", specifiedFieldsValue.Select((arg) => arg.GetValueName()).ToArray());
		}

		public string ToExtendedString()
		{
			var text = new List<string>(Attributes.Length);

			foreach (var item in Attributes)
			{
				var value = item.Value;

				if (value != null)
				{
					if (value is bool)
					{
						if (((bool)value))
						{
							text.Add(item.Name.ToString());
						}
					}
					else
					{
						text.Add(value.ToString());
					}
				}
			}

			return string.Join(" - ", text.ToArray());
		}

		public override bool Equals(object obj)
		{
			var target = obj as PawnMask;

			if (target == null)
				return false;

			//todo: fix awful compare method

			return this.ToString() == target.ToString();

			//for (int i = 0; i < Attributes.Length; i++)
			//{
			//	var selfAttribute = Attributes[i];
			//	var targetAttribute = target.Attributes[i];

			//	if (selfAttribute.Value != targetAttribute.Value)
			//	{
			//		return false;
			//	}
			//}

			//return true;
		}

		public override int GetHashCode()
		{
			int hash = 0;

			foreach (var current in Attributes)
			{
				hash *= 30 + Convert.ToInt32(current.Value);
			}

			return hash;
		}
	}
}
