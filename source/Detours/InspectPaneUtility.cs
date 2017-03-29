using System;
using System.Linq;
using HugsLib.Source.Detour;
using RimWorld;
using UnityEngine;
using Verse;

namespace WM.SmarterFoodSelection.Detours
{
	public class InspectPaneUtility
	{
		public InspectPaneUtility()
		{
		}

		//const float TAB_WIDTH = 72f;
		const float TABLIST_WIDTH = 432f;

		[DetourMethod(typeof(RimWorld.InspectPaneUtility), "DoTabs")]
		// RimWorld.InspectPaneUtility
		private static void DoTabs(IInspectPane pane)
		{
			float TAB_WIDTH = TABLIST_WIDTH / Math.Max(pane.CurTabs.Count(arg => arg.IsVisible), 6);

			try
			{
				float y = pane.PaneTopY - 30f;
				float num = TABLIST_WIDTH - TAB_WIDTH; // mod
				float width = 0f;
				bool flag = false;
				foreach (InspectTabBase current in pane.CurTabs)
				{
					if (current.IsVisible)
					{
						Rect rect = new Rect(num, y, TAB_WIDTH, 30f);
						width = num;

						var labelsize = Text.CalcSize(current.labelKey.Translate());

						if (labelsize.x > TAB_WIDTH)
							Text.Font = GameFont.Tiny;
						else
							Text.Font = GameFont.Small;

						if (Widgets.ButtonText(rect, current.labelKey.Translate(), true, false, true))
						{
							InspectPaneUtility.InterfaceToggleTab(current, pane);
						}
						bool flag2 = current.GetType() == pane.OpenTabType;
						if (!flag2 && !current.TutorHighlightTagClosed.NullOrEmpty())
						{
							UIHighlighter.HighlightOpportunity(rect, current.TutorHighlightTagClosed);
						}
						if (flag2)
						{
							current.DoTabGUI();
							pane.RecentHeight = 700f;
							flag = true;
						}
						num -= TAB_WIDTH;
					}
				}
				if (flag)
				{
					GUI.DrawTexture(new Rect(0f, y, width, 30f), InspectPaneUtility.InspectTabButtonFillTex);
				}
			}
			catch (Exception ex)
			{
				Verse.Log.ErrorOnce(ex.ToString(), 742783);
			}
		}


		public static Texture2D InspectTabButtonFillTex
		{
			get
			{
				return (Texture2D)typeof(RimWorld.InspectPaneUtility).GetField("InspectTabButtonFillTex", Helpers.AllBindingFlags).GetValue(null);
			}
		}
		static void InterfaceToggleTab(InspectTabBase current, IInspectPane pane)
		{
			typeof(RimWorld.InspectPaneUtility).GetMethod("InterfaceToggleTab", Helpers.AllBindingFlags).Invoke(null, new object[] { current, pane });
		}
	}
}