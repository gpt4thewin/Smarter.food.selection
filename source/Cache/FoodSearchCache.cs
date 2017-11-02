using System.Collections.Generic;
using System.Linq;
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
		readonly int tick = Find.TickManager.TicksGame;
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

		public override string ToString()
		{
			return eater + " / " + getter;
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

			internal FoodSourceRating GetBestFoodEntry(bool allowPlant, bool allowCorpse, bool allowPrey)
			{
				if (!AllRankedFoods.Any())
					return null;

				var disallowedCategories = new List<FoodCategory>(5);

				if (!allowPlant)
				{
					disallowedCategories.Add(FoodCategory.Plant);
					disallowedCategories.Add(FoodCategory.Tree);
					disallowedCategories.Add(FoodCategory.Grass);
				}

				if (!allowCorpse)
				{
					disallowedCategories.Add(FoodCategory.Corpse);
					disallowedCategories.Add(FoodCategory.HumanlikeCorpse);
				}

				if (!allowPrey)
					disallowedCategories.Add(FoodCategory.Hunt);

				var entry = AllRankedFoods.FirstOrDefault((arg) => arg.DefRecord != null && !disallowedCategories.Contains(arg.DefRecord.category));
				if (entry != null)
					return entry;

				return null;
			}
			internal FoodSourceRating BestFoodEntry
			{
				get
				{
					return GetBestFoodEntry(true, true, true);
				}
			}
			internal Thing BestFood
			{
				get
				{
					var foodSourceRating = GetBestFoodEntry(true, true, true);
					return foodSourceRating != null ? foodSourceRating.FoodSource : null;
				}
			}
			internal Thing BestFoodNoCorpse
			{
				get
				{
					var foodSourceRating = GetBestFoodEntry(true, false, false);
					return foodSourceRating != null ? foodSourceRating.FoodSource : null;
				}
			}
			internal Thing BestFoodNoPrey
			{
				get
				{
					var foodSourceRating = GetBestFoodEntry(true, true, false);
					return foodSourceRating != null ? foodSourceRating.FoodSource : null;
				}
			}
		}

		static internal void ClearAll()
		{
			//TODO: dispose objects ?
			AllByPawnPair.Clear();
		}
		static internal void ClearExpired()
		{
			//TODO: dispose objects ?
			int count1 = AllByPawnPair.Count();
			int count2 = AllByPawnPair.RemoveAll(arg => arg.Value.Expired());
#if DEBUG
			Log.Message(string.Format("Cleared {0}/{1} expired cache items", count2, count1));
#endif
		}

		internal static void ClearCacheForPawn(Pawn pawn)
		{
			var list = AllByPawnPair.RemoveAll(arg => arg.Key.eater == pawn);
		}

		internal static PawnEntry AddPawnEntry(Pawn getter, Pawn eater, List<FoodSourceRating> rankedFoodSources)
		{
			var pair = new PawnPair(eater, getter);
#if DEBUG
			string textlist = "";

			textlist += "Added food entry for " + pair + ". Food sources count:" + (rankedFoodSources.Count);

			Log.Message(textlist);
#endif
			//mapentry.ByRace[PawnGroupEntryKey.ForPawn(eater)].ByPawn.Add(eater, new PawnEntry() { BestFood = foodSource });
			var pawnEntry = new PawnEntry() { AllRankedFoods = rankedFoodSources };
			AllByPawnPair.Add(pair, pawnEntry);

			return pawnEntry;
		}

		internal static bool TryGetEntryForPawn(Pawn getter, Pawn eater, out PawnEntry foodSourceEntry, bool allowForbidden)
		{
			var pawnPair = new PawnPair(eater, getter);
			if (AllByPawnPair.TryGetValue(pawnPair, out foodSourceEntry))
				if (!foodSourceEntry.Expired() &&
					(foodSourceEntry.BestFood == null || // Accepts null
			         FoodUtils.IsValidFoodSourceForPawn(foodSourceEntry.BestFood, eater, getter, eater.GetPolicyAssignedTo(), allowForbidden)))
				{
#if DEBUG
					//Log.Message(string.Format("Found food entry for {0}/{1} = {2}", eater, getter, foodSourceEntry.BestFood));
#endif
					return true;
				}
				else
				{
#if DEBUG
					Log.Message(string.Format("Deleted expired food entry for {0} = {1}", pawnPair, foodSourceEntry.BestFood));
#endif
					AllByPawnPair.Remove(pawnPair);
				}
			else
			{
#if DEBUG
				Log.Message(string.Format("No food entry found for {0}", pawnPair));
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
