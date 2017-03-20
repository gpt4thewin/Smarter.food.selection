using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Report
	{
		//TODO: fix diet list
		internal static void PrintCompatibilityReport(ReportMode mode, bool silent = false)
		{
			string text = "";

			text = "\n========================== Complete food categories list ==========================\n\n";

			var completeSortedList = (from entry in FoodCategoryUtils.FoodRecords
										  //where entry.Key.GetMod() != null
									  group entry by entry.Value.category);

			foreach (var current in completeSortedList)
			{
				var completeSortedList2 = (from entry in current
										   group entry by entry.Key.GetMod());

				if (completeSortedList2.Any())
				{
					text += string.Format("+-- Foods of category {0} {1}: --------- \n", current.Key, (current.Key == FoodCategory.Null) ? "(undetermined)" : "");

					foreach (var modDefs in completeSortedList2)
					{
						text += string.Format("| [Mod : {0}]", modDefs.Key.Name);

						text += " " + String.Join(" ; ", modDefs.Select((arg) => arg.Key.ToReportString(mode) + (arg.Value.costFactor != 1f ? "(cost: " + arg.Value.costFactor.ToString("F2") + ")" : "") + (arg.Key.HasForcedFoodPref() ? " (forced)" : "")).ToArray()) + "\n";
					}
					text += "|\n";
				}
			}

			text += "\n========================== Complete diets list ==========================\n\n";

			foreach (var policy in Policies.AllVisiblePolicies)
			{
				var allDietsSorted = (from entry in policy.PerRacesDiet
									  where entry.Key.GetMod() != null
									  group entry by entry.Key.GetMod());

				var masks = string.Join(" ; ", policy.pawnMasks.Select((arg) => arg.ToString()).ToArray());
				text += string.Format("+-- [Policy : {0}] {1} --------------- \n", policy.label, masks);

				foreach (var mod in allDietsSorted)
				{
					text += string.Format("|\t[Mod : {0}]\n", mod.Key.Name);
					foreach (var diet in mod)
					{
						var detailledDiet = new List<string>();

						if (policy.unrestricted)
						{
							detailledDiet.Add("* Does not ever care *");
						}
						else
						{
							foreach (FoodCategory current in diet.Value.elements.Select((arg) => arg.foodCategory))
							{
								var foodsWithPrefForRace = DefDatabaseHelper.AllDefsIngestibleNAnimals.Where((arg) => arg.ingestible != null && arg.DetermineFoodCategory(true) == current && diet.Key.race.CanEverEat(arg));
								var foodsWithPref = FoodCategoryUtils.GetAllFoodsWithPref(current);
								int foodsWithPrefCount = foodsWithPref.Count();

								string foodsList = "";

								int gap = foodsWithPrefCount - foodsWithPrefForRace.Count();

								if (foodsWithPrefCount == foodsWithPrefForRace.Count() && foodsWithPrefCount > 3)
								{
									foodsList = "*all*";
								}
								else if (4 >= gap && gap >= 1)
								{
									var excludeList = foodsWithPref.Where((arg) => !foodsWithPrefForRace.Contains(arg)).Select((arg) => arg.ToReportString(mode));
									foodsList = string.Format("*all except* {0}", string.Join(";", excludeList.ToArray()));
								}
								else
								{
									foodsList = string.Join("=", foodsWithPrefForRace.Select((arg) => arg.ToReportString(mode)).ToArray());
								}


								detailledDiet.Add(string.Format("[{0} ({1})]", current, foodsList));
							}
						}

						text += string.Format("|\t\t{0} :\t\t{1}\n", diet.Key.ToReportString(mode), string.Join(" > ", detailledDiet.ToArray()));
					}
				}
			}

			Log.Message(string.Format("Compatibility report ({0} lines) :\n{1}", text.Count((arg) => arg == '\n'), text));

			if (!silent)
				Messages.Message("ReportPrintedInLogMessage".Translate(), MessageSound.Standard);
		}
	}
	public enum ReportMode
	{
		DefName,
		Label
	}
}
