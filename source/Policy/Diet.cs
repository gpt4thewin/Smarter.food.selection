using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	public class Diet
	{
		public List<DietElement> elements;

		//public Diet(List<DietElement> elements)
		//{
		//	this.elements = elements.ToList();
		//}
		public Diet(ThingDef pawnDef, Policy policy)
		{
			MakeDietForRace(pawnDef, policy);
		}

		void MakeDietForRace(ThingDef def, Policy policy)
		{
			if (policy.unrestricted)
				return;

			elements = policy.baseDiet.ToList();

			if (!def.race.Eats(FoodTypeFlags.Corpse))
			{
				RemoveCategory(FoodCategory.Corpse);
				RemoveCategory(FoodCategory.InsectCorpse);
				RemoveCategory(FoodCategory.HumanlikeCorpse);
				RemoveCategory(FoodCategory.SafeHunting);
				RemoveCategory(FoodCategory.RiskyHunting);
			}
			else if (!def.race.predator)
			{
				RemoveCategory(FoodCategory.SafeHunting);
				RemoveCategory(FoodCategory.RiskyHunting);
			}

			FoodCategory[] huntCat = { FoodCategory.SafeHunting, FoodCategory.RiskyHunting };

			var nonHuntCategories = elements.Where((DietElement arg) => !huntCat.Contains(arg.foodCategory)).ToArray();

			foreach (var current in nonHuntCategories)
			{
				if (policy.PolicyAllows(current.foodCategory))
				{
					if (!DefDatabaseHelper.AllDefsIngestibleNAnimals.Where((arg) => arg.ingestible != null && arg.DetermineFoodCategory(true) == current.foodCategory).Any((arg) => def.race.CanEverEat(arg)))
					{
						RemoveCategory(current.foodCategory);
					}
				}
			}

			float num = 0;
			foreach (var current in elements)
			{
				current.totalOffsetValue = num;
				num += current.scoreOffset;
			}
		}

		internal bool ContainsElement(FoodCategory pref)
		{
			return elements.Any((arg) => arg.foodCategory == pref);
		}

		private void RemoveCategory(FoodCategory category)
		{
			elements.RemoveAll((obj) => obj.foodCategory == category);
		}

		public static implicit operator List<DietElement>(Diet handle)
		{
			return handle.elements;
		}

		public class DietElement
		{
			public static float DefaultOffset = -70f;
			public FoodCategory foodCategory;
			public float scoreOffset = DefaultOffset;

			internal float totalOffsetValue;
		}

	}
}
