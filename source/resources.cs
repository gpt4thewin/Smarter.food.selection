using System;
using UnityEngine;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Resources
	{

		public static Texture2D NutrientPaste = ContentFinder<Texture2D>.Get("UI/NutrientPaste", true);
		public static Texture2D NutrientPasteCornInsectHuman = ContentFinder<Texture2D>.Get("UI/NutrientPasteCornInsectHuman", true);
		public static Texture2D NutrientPasteCorn = ContentFinder<Texture2D>.Get("UI/NutrientPasteCorn", true);
		public static Texture2D NutrientPasteHumanInsectCorn = ContentFinder<Texture2D>.Get("UI/NutrientPasteHumanInsectCorn", true);
		public static Texture2D NutrientPasteCornHuman = ContentFinder<Texture2D>.Get("UI/NutrientPasteCornHuman", true);

		public static Texture2D Cycle = ContentFinder<Texture2D>.Get("UI/Cycle", true);


		//public static Texture2D humanmeat = ContentFinder<Texture2D>.Get("UI/MeatHuman", true);
		//public static Texture2D humanmeat_forbidden = ContentFinder<Texture2D>.Get("UI/MeatHuman_forbidden", true);
		//public static Texture2D veggies = ContentFinder<Texture2D>.Get("UI/Corn", true);
		//public static Texture2D veggies_forbidden = ContentFinder<Texture2D>.Get("UI/Corn_forbidden", true);
		//public static Texture2D insectmeat = ContentFinder<Texture2D>.Get("UI/MeatBig", true);
		//public static Texture2D insectmeat_forbidden = ContentFinder<Texture2D>.Get("UI/MeatBig_forbidden", true);
		//public static Texture2D forbidden = ContentFinder<Texture2D>.Get("UI/ForbiddenOverlay", true);

	}
	public static class KeysBinding
	{
		public static KeyBindingDef ToggleFoodScore = KeyBindingDef.Named("WM_SFS_ToggleFoodScore");
	}
}
