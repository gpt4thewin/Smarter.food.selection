using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
	public class CompabilityDef : MyDefClass
	{
		public class ComptabilityElement
		{
			public string def;
			public FoodCategory pref;
		}

		public string requiredMod;
		public List<ComptabilityElement> foods;

		//public bool optional = false;

		private bool loaded = false;

		public bool Loaded
		{
			get
			{
				return loaded;
			}
		}
		public int DefsCount
		{
			get
			{
				return foods.Count;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			RecordPatch();
		}

		private void RecordPatch()
		{
			ModCore.patches.Add(this);
		}

		internal int TryApplyPatch()
		{
			try
			{
				if (foods == null)
					return 0;

				int loadedElements = 0;

				if (LoadedModManager.RunningMods.Any((ModContentPack arg) => arg.PackageId == requiredMod || arg.Name == requiredMod))
				{
					loaded = true;
					foreach (ComptabilityElement current in foods)
					{
						if (current.def != null)
						{
							var thingDef = ThingDef.Named(current.def);
							FoodCategoryUtils.RecordFood(thingDef, current.pref);
							loadedElements++;
						}
					}
					//Log.Message("Loaded compatibility patch \"" + this.defName + "\" for mod " + requiredMod + ". Applied " + loadedElements + " fixes.");
				}

				return loadedElements;
			}
			catch (Exception ex)
			{
				Log.Error("Exception when loading compatibility Def for mod " + requiredMod + " : " + ex + " " + ex.StackTrace);
			}

			return 0;
		}
	}
}
