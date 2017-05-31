using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;
using Verse.Sound;

namespace WM.SmarterFoodSelection.Detours
{
	public class CompProperties_ExtraNPD : CompProperties
	{
		public CompProperties_ExtraNPD()
		{
			this.compClass = typeof(Building_NutrientPasteDispenser_ExtraComp);
		}
	}

	public class Building_NutrientPasteDispenser_ExtraComp : ThingComp
	{
		const float ForceDispensePowerCost = 15f;

		DispenseMode currentMode = DispenseMode.Standard;

		Command_Action switchMode;

		public DispenseMode CurrentMode
		{
			get
			{
				return currentMode;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			var gizmos = new List<Gizmo>();

			gizmos.Add(MakeDispenseNGizmo(1));
			gizmos.Add(MakeDispenseNGizmo(5));

			switchMode = new Command_Action();

			switchMode.icon = Resources.Cycle;

			switchMode.defaultLabel = (currentMode.ToString() + "Mode").Translate();
			switchMode.defaultDesc = (currentMode.ToString() + "Mode_desc").Translate() + "\n" + "DispenserModeWarning".Translate();

			switchMode.action = delegate
			{
				var values = Enum.GetValues(typeof(DispenseMode));
				int n = Array.IndexOf(values, currentMode);
				n++;
				if (n >= values.Length)
					n = 0;
				currentMode = (DispenseMode)values.GetValue(n);
			};

			gizmos.Add(switchMode);

			return gizmos;
		}

		Gizmo MakeDispenseNGizmo(int count)
		{
			var gizmo = new Command_Action();

			gizmo.defaultLabel = string.Format("DispenseXcount".Translate(), count);
			gizmo.defaultDesc = string.Format("DispenseXcount_desc".Translate(), count) + "\n" + string.Format("ManualDispenseCostWarning".Translate(), ForceDispensePowerCost, count, ForceDispensePowerCost * count);

			switch (currentMode)
			{
				case DispenseMode.Standard:
					gizmo.icon = Resources.NutrientPasteCornInsectHuman;
					break;
				case DispenseMode.Clean:
					gizmo.icon = Resources.NutrientPasteCorn;
					break;
				case DispenseMode.Cannibal:
					gizmo.icon = Resources.NutrientPasteHumanInsectCorn;
					break;
				case DispenseMode.CannibalClean:
					gizmo.icon = Resources.NutrientPasteCornHuman;
					break;
				case DispenseMode.Animal:
					gizmo.icon = Resources.NutrientPasteCornHuman;
					break;
			}

			gizmo.action = delegate
			{
				ForceDispense(count, currentMode);
			};

			return gizmo;
		}

		bool ForceDispense(int count, DispenseMode mode)
		{
			if (!Utils.DrawPowerFromNetwork((Verse.Building)this.parent, ForceDispensePowerCost * count, true))
			{
				Messages.Message("NPDNoEnergy".Translate(), MessageSound.RejectInput);
				return false;
			}

			for (int i = 0; i < count; i++)
			{
				Thing meal = ((Building_NutrientPasteDispenser)this.parent).TryDispenseFood(mode, true);
				if (meal == null)
					return false;

				// TODO: refund (very unlikely)
				if (!GenPlace.TryPlaceThing(meal, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near))
					return false;
			}

			Utils.DrawPowerFromNetwork((Verse.Building)this.parent, ForceDispensePowerCost * count);

			return true;
		}
	}
}
