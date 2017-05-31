using System;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace WM.SmarterFoodSelection.Detours.InspectPaneUtility
{
	[HarmonyPatch(typeof(RimWorld.InspectPaneUtility), "DoTabs")]
	public static class DoTabs
	{
		//const float TAB_WIDTH = 72f;
		const float TABLIST_WIDTH = 432f;

		//[HarmonyPrepare]
		//static bool MyInitializer()
		//{
		//	return false;
		//}

		[HarmonyPrefix]
		public static bool Prefix()
		{ return false; }

		[HarmonyPostfix]
		//private static void DoTabs(IInspectPane pane)
		private static void Postfix(IInspectPane pane)
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
							InspectPaneUtility.DoTabs.InterfaceToggleTab(current, pane);
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
					GUI.DrawTexture(new Rect(0f, y, width, 30f), InspectPaneUtility.DoTabs.InspectTabButtonFillTex);
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
				return (Texture2D)typeof(RimWorld.InspectPaneUtility).GetField("InspectTabButtonFillTex", AccessTools.all).GetValue(null);
			}
		}
		static void InterfaceToggleTab(InspectTabBase current, IInspectPane pane)
		{
			typeof(RimWorld.InspectPaneUtility).GetMethod("InterfaceToggleTab", AccessTools.all).Invoke(null, new object[] { current, pane });
		}
	}
}