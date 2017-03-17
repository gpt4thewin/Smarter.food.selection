using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;


namespace WM.SmarterFoodSelection
{
	//public abstract class CacheClass<TKey, TValue>
	//{
	//	Dictionary<TKey, TValue> data;

	//	public CacheClass()
	//	{
	//	}
	//}

	public abstract class CacheEntry
	{
		int tick = Find.TickManager.TicksGame;
		public int LifeTime { get; protected set; }

		public bool NeverExpires { get; protected set; }

		public CacheEntry()
		{
			LifeTime = 300;
			NeverExpires = false;
		}

		public bool Expired()
		{
			if (NeverExpires)
				return false;

			return tick + LifeTime < Find.TickManager.TicksGame;
		}
	}
	public class PawnPair
	{
		public Pawn eater;
		public Pawn getter;

		public PawnPair(Pawn eater, Pawn getter)
		{
			this.eater = eater;
			this.getter = getter;
		}

		public override bool Equals(object obj)
		{
			var pawnpair = obj as PawnPair;

			if (pawnpair == null)
				return false;

			return
				pawnpair.eater == eater &&
				pawnpair.getter == getter;
		}
		public override int GetHashCode()
		{
			int hash =
				2 * eater.GetHashCode() +
				4 * getter.GetHashCode();

			return hash;
		}
	}
	public static class FoodSearchCache
	{
		//internal static Dictionary<Map, MapEntry> ByMap = new Dictionary<Map, MapEntry>();

		// Pair: eater - getter
		internal static Dictionary<PawnPair, PawnEntry> AllByPawnPair = new Dictionary<PawnPair, PawnEntry>();

		internal class PawnEntry : CacheEntry
		{
			internal List<FoodSourceRating> AllRankedFoods { get; set; }

			internal Thing GetBestFood(bool allowPlant, bool allowCorpse, bool allowPrey)
			{
				if (!AllRankedFoods.Any())
					return null;
				
				List<FoodCategory> disallowedCategories = new List<FoodCategory>(3);

				if (!allowPlant)
					disallowedCategories.Add(FoodCategory.Plant);

				if (!allowCorpse)
					disallowedCategories.Add(FoodCategory.Corpse);

				if (!allowPrey)
					disallowedCategories.Add(FoodCategory.SafeHunting);

				var entry = AllRankedFoods.FirstOrDefault((arg) => !disallowedCategories.Contains(arg.DefRecord.category));
				if (entry != null)
					return entry.FoodSource;

				return null;
			}
			internal Thing BestFood
			{
				get
				{
					return GetBestFood(true, true, true);
				}
			}
			internal Thing BestFoodNoCorpse
			{
				get
				{
					return GetBestFood(true, false, false);
				}
			}
			internal Thing BestFoodNoPrey
			{
				get
				{
					return GetBestFood(true, true, false);
				}
			}
		}

		static internal void ClearAll()
		{
			//TODO: dispose objects ?
			AllByPawnPair.Clear();
		}

		internal static void ClearCacheForPawn(Pawn pawn)
		{
			var list = AllByPawnPair.RemoveAll(arg => arg.Key.eater == pawn);
		}

		internal static PawnEntry AddPawnEntry(Pawn getter, Pawn eater, List<FoodSourceRating> rankedFoodSources)
		{
#if DEBUG
			var policy = eater.GetPolicyAssignedTo();

			string textlist = "";

			textlist += "Added food entry for " + eater + ". Food sources count:" + (rankedFoodSources.Count);

			Log.Message(textlist);
#endif
			//mapentry.ByRace[PawnGroupEntryKey.ForPawn(eater)].ByPawn.Add(eater, new PawnEntry() { BestFood = foodSource });
			var pawnEntry = new PawnEntry() { AllRankedFoods = rankedFoodSources };
			AllByPawnPair.Add(new PawnPair(eater, getter), pawnEntry);

			return pawnEntry;
		}

		internal static bool TryGetEntryForPawn(Pawn getter, Pawn eater, out PawnEntry foodSourceEntry, bool allowForbidden)
		{
			var pawnPair = new PawnPair(eater, getter);
			if (AllByPawnPair.TryGetValue(pawnPair, out foodSourceEntry))
				if (!foodSourceEntry.Expired() &&
					(foodSourceEntry.BestFood == null || // Accepts null
					 Detours.FoodUtility.IsValidFoodSourceForPawn(foodSourceEntry.BestFood, eater, getter, eater.GetPolicyAssignedTo(), allowForbidden)))
				{
#if DEBUG
					//Log.Message(string.Format("Found food entry for {0}/{1} = {2}", eater, getter, foodSourceEntry.BestFood));
#endif
					return true;
				}
				else
				{
#if DEBUG
					Log.Message(string.Format("Deleted expired food entry for {0}/{1} = {2}", eater, getter, foodSourceEntry.BestFood));
#endif
					AllByPawnPair.Remove(pawnPair);
				}
			else
			{
#if DEBUG
				Log.Message(string.Format("No food entry found for {0}", eater));
#endif
			}

			foodSourceEntry = null;
			return false;
		}
	}

	//	internal class CacheForPawnMask : CacheClass
	//	{
	//		internal class ClassEntry
	//		{
	//			internal FoodRecord foodrecord { get; set; }
	//			internal IGrouping<int, Thing> group { get; set; }
	//			internal float score { get; set; }
	//		}

	//		static Dictionary<Map, IEnumerable<IEnumerable<ClassEntry>>> Cache = new Dictionary<Map, IEnumerable<IEnumerable<ClassEntry>>>();

	//
	//		//internal static void Add(Map map, IEnumerable<ClassEntry> data)
	//		//{
	//		//	IEnumerable<IEnumerable<ClassEntry>> entry;

	//		//	if (!Cache.TryGetValue(map, out entry))
	//		//	{
	//		//	}
	//		//}

	//		internal static bool TryGetEntry(Map map, Pawn eater, out IEnumerable<ClassEntry> cacheentry)
	//		{
	//			throw new NotImplementedException();
	//		}

	//		internal static void Add(Map map, Pawn eater, IEnumerable<ClassEntry> cacheentry)
	//		{
	//			throw new NotImplementedException();
	//		}
	//	}

	//	internal class CacheForSpecificPawn : CacheClass
	//	{
	//		static Dictionary<Map, > Cache = new Dictionary<Map, IEnumerable<IEnumerable<ClassEntry>>>();

	//		//internal static void Add(Map map, IEnumerable<ClassEntry> data)
	//		//{
	//		//	IEnumerable<IEnumerable<ClassEntry>> entry;

	//		//	if (!Cache.TryGetValue(map, out entry))
	//		//	{
	//		//	}
	//		//}

	//		internal static bool TryGetEntry(Map map, Pawn eater, out IEnumerable<ClassEntry> cacheentry)
	//		{
	//			throw new NotImplementedException();
	//		}

	//		internal static void Add(Map map, Pawn eater, Thing cacheentry)
	//		{
	//			throw new NotImplementedException();
	//		}
	//	}
}
