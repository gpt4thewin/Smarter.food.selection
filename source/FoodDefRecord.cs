using System;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
	public class FoodDefRecord
	{
		//public ThingDef target;
		public FoodCategory category;
		public float costFactor = 1f;

		public static float CalculateCost(ThingDef target)
		{
			var recipes = target.AllRecipes;

			if (recipes == null || !recipes.Any() || target.ingestible == null)
				return float.MinValue;

			float result;

			try
			{
				result = recipes.Min((recipe) => recipe.ingredients.
									 Sum(delegate (IngredientCount arg)
									 {
										 var output = recipe.products.FirstOrDefault();
										 if (output == null || output.count > 1)
											 return 0;
										 return arg.GetBaseCount() / output.count;
									 }) 
				                    );
			}
			catch (Exception)
			{
				//throw new Exception("CalculateCost() fail", ex);
				return float.MinValue;
			}

#if DEBUG
			if (result >= 0f)
				Log.Message(string.Format("Cost for :{0} = {1}", target, result));
#endif

			return result;
		}
	}
}
