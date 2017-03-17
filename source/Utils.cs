using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Utils
	{
		public static bool DrawPowerFromNetwork(Building building, float amount, bool simulate = false, bool silent = true)
		{
			var comp = building.GetComp<CompPowerTrader>();

			if (comp != null && comp.PowerNet != null)
				return DrawPowerFromNetwork(comp.PowerNet, amount, simulate);

			if (!silent)
				Log.Warning("Building " + building + " tried to draw power from non existent network.");

			return false;
		}
		public static bool DrawPowerFromNetwork(PowerNet powerNet, float amount, bool simulate = false)
		{
			var batteries = powerNet.batteryComps.OrderByDescending((CompPowerBattery arg) => arg.StoredEnergy);

			float totalEnergy = batteries.Sum((CompPowerBattery arg) => arg.StoredEnergy);

			if (totalEnergy < amount)
				return false;

			if (simulate && totalEnergy >= amount)
				return true;

			float drawnEnergy = 0;

			foreach (var current in batteries)
			{
				float num = Math.Min(current.StoredEnergy, amount - drawnEnergy);

				current.DrawPower(num);

				drawnEnergy += num;

				if (drawnEnergy >= amount)
					break;
			}

			return true;
		}

		public static void ShowRevertAllWorldNonVanillaThingsDialog()
		{
			var list = GetAllWorldNonVanillaThings();

			if (list.Count() == 0)
				return;

			Dialog_MessageBox confirmDia = new Dialog_MessageBox("separatedNutrientPaste_SettingChangedDialog".Translate());
			confirmDia.buttonAText = "Yes".Translate();
			confirmDia.buttonBText = "No".Translate();

			confirmDia.buttonAAction = delegate
			{
				foreach (var thing in list)
				{
					if (thing.def == ThingDef.Named("MealNutrientPasteCannibal"))
						thing.def = ThingDefOf.MealNutrientPaste;
				}
			};

			//confirmDia.cre
		}
		private static IEnumerable<Thing> GetAllWorldNonVanillaThings()
		{
			List<Thing> list = new List<Thing>();

			foreach (var map in Find.Maps)
			{
				foreach (var thing in map.listerThings.AllThings)
				{
					if (thing.def == ThingDef.Named("MealNutrientPasteCannibal"))
						list.Add(thing);
				}
			}

			return list;
		}

		public static ModContentPack GetMod(this Def def)
		{
			foreach (var mod in LoadedModManager.RunningMods.Reverse())
			{
				var AllThingDefs = mod.AllDefs.Where((Def arg) => arg is ThingDef);

				bool result = AllThingDefs.Any(delegate (Def arg)
				{
					if (!(arg is ThingDef))
						return false;

					if (arg == def)
						return true;

					if (((ThingDef)arg).race != null)
					{
						if (((ThingDef)arg).race.meatDef == def)
							return true;
						if (((ThingDef)arg).race.corpseDef == def)
							return true;
					}

					return false;
				});

				if (result)
					return mod;
			}

			Log.Warning("Could not figure out the mod of Def " + def);
			return null;
		}

		public static bool CanReachFoodSource(this Pawn pawn, Thing food)
		{
			if (pawn.inventory != null && pawn.inventory.innerContainer.Contains(food))
			{
				return true;
			}

			IntVec3 position;

			if (food.def.hasInteractionCell)
			{
				position = food.InteractionCell;
			}
			else
			{
				position = food.Position;
			}

			return pawn.CanReach(position, Verse.AI.PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

		// Verse.Corpse
		public static BodyPartRecord GetBestBodyPartToEat(this Corpse self, Pawn ingester, float nutritionWanted)
		{
			return (BodyPartRecord)typeof(Corpse).GetMethod("GetBestBodyPartToEat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(self, new object[] { ingester, nutritionWanted });
		}

		internal static bool isWildAnimal(this Pawn pawn)
		{
			return pawn.Faction == null;
		}

		internal static bool IsIncapacitated(this Pawn pawn)
		{
			return !pawn.health.InPainShock && pawn.Downed;
		}

		internal static bool IsCannibal(this Pawn pawn)
		{
			if (pawn.story == null)
				return false;
			return pawn.story.traits.HasTrait(TraitDefOf.Cannibal);
		}
		internal static bool IsAscetic(this Pawn pawn)
		{
			if (pawn.story == null)
				return false;
			return pawn.story.traits.HasTrait(TraitDefOf.Ascetic);
		}
		public static bool HasForcedFoodPref(this ThingDef def)
		{
			foreach (var current in ModCore.patches)
			{
				if (current.Loaded)
				{
					if (current.foods.Any((obj) => ThingDef.Named(obj.def) == def))
						return true;
				}
			}

			return false;
		}
		public static string ToReportString(this Def def, ReportMode mode)
		{
			if (mode == ReportMode.DefName)
				return def.defName;

			if (mode == ReportMode.Label)
				return string.Format("\"{0}\"", def.label);

			return def.ToString();
		}
		public static bool CaseUnsensitiveCompare(this string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		[Conditional("DEBUG")]
		public static void DebugPrintTrace(string message)
		{
			StackTrace stackTrace = new StackTrace(true);
			StackFrame sf = stackTrace.GetFrame(1);

			string text = "";

			text += ("Trace "
				+ sf.GetMethod().Name + " "
				+ sf.GetFileName() + ":"
				+ sf.GetFileLineNumber() + Environment.NewLine);

			text += (message + Environment.NewLine);

			Log.Message(text);
		}
	}
}
