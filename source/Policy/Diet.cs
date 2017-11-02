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
				RemoveCategory(FoodCategory.Hunt);
			}
			else if (!def.race.predator)
			{
				RemoveCategory(FoodCategory.Hunt);
			}

			FoodCategory[] huntCat = { FoodCategory.Hunt };

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
				//TODO: fix broken diet order when removing the first element for a race (eg: thrumbo)
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

		public override string ToString()
		{
			var textgroups = new List<string>();

			if (elements == null)
				return "* Allows anything *"; // TODO: .Translate()

			var list = (from entry in elements
						group entry by entry.totalOffsetValue);

			foreach (var item in list)
			{
				textgroups.Add(string.Join(" = ", item.Select(arg => ("FoodCategory." + arg.foodCategory.ToString()).Translate()).ToArray()));
			}

			string text = string.Join(" > ", textgroups.ToArray());

			return text;
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
