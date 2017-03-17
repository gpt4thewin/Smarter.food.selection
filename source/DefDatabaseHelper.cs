using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class DefDatabaseHelper
	{
		public static List<ThingDef> AllDefsIngestibleNAnimals
		{
			get
			{
				return DefDatabase<ThingDef>.AllDefs.Where((arg) => arg.ingestible != null || (arg.race != null)).ToList();
			}
		}
		public static List<ThingDef> AllDefsIngestible
		{
			get
			{
				return DefDatabase<ThingDef>.AllDefs.Where((arg) => arg.ingestible != null).ToList();
			}
		}
		public static IEnumerable<ThingDef> AllPawnDefs
		{
			get
			{
				return DefDatabase<ThingDef>.AllDefs.Where((arg) => arg.race != null && arg.race.EatsFood);
			}
		}

		public static IEnumerable<ThingDef> AllHumanlikePawnDefs
		{
			get
			{
				return AllPawnDefs.Where((ThingDef arg) => arg.race.Humanlike);
			}
		}

		public static IEnumerable<ThingDef> AllAnimalDefs 
		{ 
			get
			{
				return AllPawnDefs.Where((ThingDef arg) => !arg.race.Humanlike);
			}
		}
	}
}
