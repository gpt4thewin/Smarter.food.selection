using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using Verse;

namespace WM.SmarterFoodSelection
{
	public class WorldDataStore_PawnPolicies : UtilityWorldObject
	{
		static WorldDataStore_PawnPolicies SingleInstance;

		Dictionary<Pawn, Policy> AssignedPolicies = new Dictionary<Pawn, Policy>();

		public static int AssignedPoliciesCount
		{
			get
			{
				return SingleInstance.AssignedPolicies.Values.Count((arg) => arg != Policies.Unrestricted);
			}
		}

		public WorldDataStore_PawnPolicies()
		{
			SingleInstance = this;

		}

		// dummy fields ?
		List<Policy> valuesList;
		List<Pawn> keysList;

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Collections.LookDictionary<Pawn, Policy>(ref AssignedPolicies, "WMSFS_PawnFoodPolicies", LookMode.Reference, LookMode.Def, ref keysList, ref valuesList);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				foreach (var item in AssignedPolicies.ToList())
				{
					if (!item.Key.CanHaveFoodPolicy() || item.Key.HasHardcodedPolicy())
					{
						AssignedPolicies.Remove(item.Key);
#if DEBUG
						Log.Warning(item.Key + " had assigned policy but should not have one. Discarding. Did it changed faction ?");
#endif
					}
				}
			}

			//TODO: manage missing policy defs
		}

		// redundant
		//internal IEnumerable<Policy> PoliciesFor(Pawn pawn)
		//{
		//}

		internal static void SetDefaultForRaces(Policy policy)
		{
			throw new NotImplementedException();
		}

		internal static void AssignToAllPawnsOfRacesOnMap(Policy policy)
		{
			throw new NotImplementedException();
		}

		internal static void AssignToAllPawnsOfRaces(Policy policy)
		{
			throw new NotImplementedException();
		}

		internal static void AssignToAllPawnsWithMask(PawnMask mask)
		{
			throw new NotImplementedException();
		}

		internal static void SetPolicyForPawn(Pawn pawn, Policy policy)
		{
			GetPawnEntry(pawn);

			SingleInstance.AssignedPolicies[pawn] = policy;

			FoodSearchCache.ClearCacheForPawn(pawn);
		}

		internal static Policy GetPawnEntry(Pawn pawn)
		{
			if (pawn.HasHardcodedPolicy())
			{
				throw new Exception("Tried to fetch assigned policy for " + pawn + " but has a non modifiable policy");
			}

			Policy value;

			if (!SingleInstance.AssignedPolicies.TryGetValue(pawn, out value))
			{
				//var list = Policies.GetAllPoliciesForPawn(pawn).Where((Policy arg) => arg.pa;
				var list = Policies.AllPawnMasks.Where((PawnMask arg) => arg.MatchesPawn(pawn));
				if (!list.Any())
				{
					value = Policies.Unrestricted;
				}
				else
				{
					value = list.MaxBy(arg => arg.AllSpecifiedAttributes().Count()).targetDefault;
				}
				SingleInstance.AssignedPolicies.Add(pawn, value);
			}
			else
			{
				value = SingleInstance.AssignedPolicies[pawn];
			}

			return value;
		}

	}
}
