using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;


// System.Runtime.Caching is not available in .NET 3.5

namespace WM.SmarterFoodSelection
{
	//public abstract class CacheClass<TKey, TValue>
	//{
	//	Dictionary<TKey, TValue> data;

	//	public CacheClass()
	//	{
	//	}
	//}


	

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
