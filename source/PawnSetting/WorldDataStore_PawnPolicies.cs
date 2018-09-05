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

			Scribe_Collections.Look<Pawn, Policy>(ref AssignedPolicies, "WMSFS_PawnFoodPolicies", LookMode.Reference, LookMode.Def, ref keysList, ref valuesList);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				foreach (var item in AssignedPolicies.ToList())
				{
					// policy defname missing ? reset policy
					if (item.Value == null || item.Key == null || item.Key.Dead || !item.Key.CanHaveFoodPolicy() || item.Key.HasHardcodedPolicy())
					{
						AssignedPolicies.Remove(item.Key);

						Log.Warning(item.Key + " had a wrong assigned policy. Reseting. Did you remove its policy from Defs ?");

					}
				}
			}
		}

		// redundant
		//internal IEnumerable<Policy> PoliciesFor(Pawn pawn)
		//{
		//}


		internal static void AssignToAllPawnsOfRacesOnMap(Policy policy, ThingDef race)
		{
			Func<Pawn, bool> validator = (arg) => arg.def == race;
			AssignToAllPawnsMatchingOnMap(policy, validator);
		}

		internal static void AssignToAllPawnsWithMaskOnMap(Policy policy, PawnMask mask)
		{
			Func<Pawn, bool> validator = (arg) => mask.MatchesPawn(arg);
			AssignToAllPawnsMatchingOnMap(policy, validator);
		}

		internal static void AssignToAllPawnsMatchingOnMap(Policy policy, Func<Pawn, bool> validator)
		{
			var map = Find.CurrentMap;

			var pawns = map.mapPawns.AllPawnsSpawned.Where(validator);

			foreach (var item in pawns)
			{
				WorldDataStore_PawnPolicies.SetPolicyForPawn(item, policy);
			}
		}

		internal static void SetPolicyForPawn(Pawn pawn, Policy policy)
		{
			GetPawnEntry(pawn);

			if (policy == null)
			{
				policy = GetDefaultPolicyFor(pawn);
			}

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
				value = GetDefaultPolicyFor(pawn);
				SingleInstance.AssignedPolicies.Add(pawn, value);
			}
			else
			{
				value = SingleInstance.AssignedPolicies[pawn];
			}

			return value;
		}

		internal static Policy GetDefaultPolicyFor(Pawn pawn)
		{
			Policy policy = Policies.Unrestricted;

			//var list = Policies.GetAllPoliciesForPawn(pawn).Where((Policy arg) => arg.pa;
			var list = Policies.AllPawnMasks.Where((PawnMask arg) => arg.MatchesPawn(pawn));
			if (!list.Any())
			{
				policy = Policies.Unrestricted;
			}
			else
			{
				var list2 = list.OrderByDescending(arg => arg.AllSpecifiedAttributes().Count());

				var entry = list2.FirstOrDefault((arg) => arg.targetDefault != Policies.Unrestricted);

				if (entry != null)
					policy = entry.targetDefault;
			}

			return policy;
		}

	}
}
