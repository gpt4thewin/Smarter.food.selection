using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
	public abstract class PawnMask
	{
		public abstract bool MatchesPawn(Pawn pawn);

		public virtual string Label
		{
			get
			{
				var name = this.GetType().Name;
				return ("WM.PawnMask." + name).Translate();
			}
		}

		public virtual string Desc
		{
			get
			{
				var name = this.GetType().Name;
				return ("WM.PawnMask.Desc." + name).Translate();
			}
		}

		public bool HasRiggedPolicy
		{
			get
			{
				return (RiggedPolicy != null);
			}
		}

		public virtual Policy RiggedPolicy
		{
			get
			{
				return (null);
			}
		}

		public Policy UserDefaultPolicy
		{
			get
			{
				return (Policies.DefaultPolicyForMask(this));
			}
		}

		public IEnumerable<Policy> AllRelatedPolicies
		{
			get
			{
				return (Policies.AllPolicies.Where((Policy arg) => arg.AdmitsPawnMask(this)));
			}
		}
	}
}
