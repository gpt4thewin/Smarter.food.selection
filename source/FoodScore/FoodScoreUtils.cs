using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using WM.SmarterFoodSelection.Detours;

namespace WM.SmarterFoodSelection
{

	public static class FoodScoreUtils
	{
		public static int GetFoodHash(this Thing t)
		{
			int hash = 0;

			hash += 10 * t.def.GetHashCode();

			var ingredientComp = t.TryGetComp<CompIngredients>();

			if (ingredientComp != null)
			{
				for (int i = 0; i < ingredientComp.ingredients.Count; i++)
				{
					var item = ingredientComp.ingredients[i];

					hash += (20 + i) * item.GetHashCode();
				}
			}

			return hash;
		}

		//TODO: optimize function
		internal static FoodSourceRating FoodScoreFor(Policy policy, Pawn eater, Pawn getter, Thing food, bool noDistanceFactor = false, bool quickScore = false)
		{
			var obj = new FoodSourceRating();

			obj.FoodSource = food;
			obj.DefRecord = food.GetFoodDefRecord();

			// ------ Distance factor ------ 

			bool inInventory = (getter.inventory != null && getter.inventory.innerContainer.Contains(food));

			if (!noDistanceFactor)
			{
				float distanceFactor;
				if (inInventory)
					distanceFactor = 0f;
				else
					distanceFactor = -((food.Position - eater.Position).LengthManhattan) * policy.distanceFactor;

				obj.AddComp("Distance", distanceFactor);
			}

			if (inInventory && getter == eater && food.def.ingestible.preferability >= FoodPreferability.MealAwful 
                && (eater.IsColonistPlayerControlled && (food.GetFoodCategory() != FoodCategory.MealSurvival))) //added to prevent survival meals from being eaten out of inventory
			{
				obj.AddComp("Inventory", 500f);
			}

			// ------------- Policy food category factor -------------

				//if (!policy.unrestricted)
				//	obj.AddComp(food.DetermineFoodCategory().ToString(), policy.GetFoodScoreOffset(eater, food));

				// ------------- Prey score factor -------------

				const float PREY_FACTOR_MULTIPLIER = 5f; //Because the vanilla prey factor is fairly weak.

			{
				if (food is Pawn)
				{
					var preyScore = RimWorld.FoodUtility.GetPreyScoreFor(eater, food as Pawn) * PREY_FACTOR_MULTIPLIER;
					//ducktape, negates the distance factor of the vanilla function as it is already calculated by the mod.
					preyScore += (eater.Position - food.Position).LengthHorizontal * PREY_FACTOR_MULTIPLIER;
					obj.AddComp("Prey (ratio=" + FoodUtils.GetPreyRatio(eater, food as Pawn).ToString("F2") + ")", preyScore);
					return obj;
				}
			}

			// ------------- Food def optimality offset -------------

			if (policy.optimalityOffsetMatters && food.def.ingestible != null)
			{
				float optimalityOffset;

				if (eater.NonHumanlikeOrWildMan())
					optimalityOffset = food.def.ingestible.optimalityOffsetFeedingAnimals;
				else 
					optimalityOffset = food.def.ingestible.optimalityOffsetHumanlikes;
				obj.AddComp("Def", food.def.ingestible.optimalityOffsetHumanlikes);
			}

			if (!quickScore)
			{
				// ------------- Mood effect factor -------------

				//float num2 = 0f;
				if (eater.needs != null && eater.needs.mood != null && policy.moodEffectMatters)
				{
					List<ThoughtDef> list;

					if (!(food is RimWorld.Building_NutrientPasteDispenser))
						list = RimWorld.FoodUtility.ThoughtsFromIngesting(eater, food, food.def);
					else
						list = ((RimWorld.Building_NutrientPasteDispenser)food).GetBestMealThoughtsFor(eater);

					for (int i = 0; i < list.Count; i++)
					{
						obj.AddComp(list[i].defName,
						            policy.moodEffectFactor * Detours.Access.FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect));
					}
				}

				// ------------- Cost factor (waste factor aswell) -------------

				//TODO: concurrent based waste factor.

				float costRatio;
				float foodNutrition;
				var curFoodLevel = eater.needs.food.CurLevel;
				var maxFoodLevel = eater.needs.food.MaxLevel;

				if (food.def.IsFoodDispenser)
					foodNutrition = ThingDefOf.MealNutrientPaste.ingestible.CachedNutrition;
				else if (food.def.IsCorpse)
				{
					var corpse = food as Corpse;
					foodNutrition = RimWorld.FoodUtility.GetBodyPartNutrition(corpse, corpse.GetBestBodyPartToEat(eater, curFoodLevel));
				}
				else
					foodNutrition = food.def.ingestible.CachedNutrition;

				if (foodNutrition >= 0.1f)
				{
					//TODO: ajust; add record for the NPD

					if (food is RimWorld.Building_NutrientPasteDispenser)
						costRatio = (food.def.building.nutritionCostPerDispense * 0.05f) / ThingDefOf.MealNutrientPaste.ingestible.CachedNutrition;
					else
						costRatio = (obj.DefRecord.costFactor);

					if (food.def.IsCorpse)
						costRatio *= 0.25f;

					//TODO: combine waste & cost factors

					float actualMealCost = costRatio * foodNutrition;
					float costFactorOffset = Math.Max(0, actualMealCost - (maxFoodLevel - curFoodLevel)) * -Config.CostFactor /* * policy.costFactorMultiplier*/;
					//* Config.CostFactor * policy.costFactorMultiplier;

					if (costFactorOffset != 0f)
						obj.AddComp("Hunger (min=" + actualMealCost.ToString("F2") + ")", costFactorOffset);
				}


				// ------------- TODO: Rot factor -------------
			}

			return obj;
		}

	}
}