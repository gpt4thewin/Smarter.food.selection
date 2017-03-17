using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
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
		internal static FoodSourceRating FoodScoreFor(Policy policy, Pawn eater, Thing food, bool noDistanceFactor = false, bool quickScore = false)
		{
			var obj = new FoodSourceRating();

			obj.FoodSource = food;
			obj.DefRecord = food.GetFoodDefRecord();

			// ------ Distance factor ------ 

			if (!noDistanceFactor)
				obj.AddComp("Distance", -((food.Position - eater.Position).LengthManhattan) * policy.distanceFactor);

			// ------------- Policy food category factor -------------

			//if (!policy.unrestricted)
			//	obj.AddComp(food.DetermineFoodCategory().ToString(), policy.GetFoodScoreOffset(eater, food));

			// ------------- Prey score factor -------------
			{
				if (food is Pawn)
				{
					var preyScore = RimWorld.FoodUtility.GetPreyScoreFor(eater, food as Pawn);
					//ducktape, negates the distance factor of the vanilla function as it is already calculated by the mod.
					preyScore += (eater.Position - food.Position).LengthHorizontal;
					obj.AddComp("Prey", preyScore);
					return obj;
				}
			}

			// ------------- Food def optimality offset -------------

			if (policy.optimalityOffsetMatters && food.def.ingestible != null)
			{
				obj.AddComp("Def", food.def.ingestible.optimalityOffset);
			}

			if (!quickScore)
			{
				// ------------- Mood effect factor -------------

				//float num2 = 0f;
				if (eater.needs != null && eater.needs.mood != null && policy.moodEffectMatters)
				{
					List<ThoughtDef> list;

					if (!(food is RimWorld.Building_NutrientPasteDispenser))
						list = RimWorld.FoodUtility.ThoughtsFromIngesting(eater, food);
					else
						list = ((RimWorld.Building_NutrientPasteDispenser)food).GetBestMealThoughtsFor(eater);

					for (int i = 0; i < list.Count; i++)
					{
						obj.AddComp(list[i].defName,
									policy.moodEffectFactor * Detours.Original.FoodUtility.FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect));
					}
				}

				// ------------- Cost factor (waste factor aswell) -------------

				float costRatio;
				float foodNutrition;
				var curFoodLevel = eater.needs.food.CurLevel;
				var maxFoodLevel = eater.needs.food.MaxLevel;

				if (food.def.IsFoodDispenser)
					foodNutrition = ThingDefOf.MealNutrientPaste.ingestible.nutrition;
				else if (food.def.IsCorpse)
				{
					var corpse = food as Corpse;
					foodNutrition = RimWorld.FoodUtility.GetBodyPartNutrition(corpse.InnerPawn, corpse.GetBestBodyPartToEat(eater, curFoodLevel));
				}
				else
					foodNutrition = food.def.ingestible.nutrition;

				if (foodNutrition >= 0.1f)
				{
					//TODO: ajust; add record for the NPD

					if (food is RimWorld.Building_NutrientPasteDispenser)
						costRatio = (food.def.building.foodCostPerDispense * 0.05f) / ThingDefOf.MealNutrientPaste.ingestible.nutrition;
					else
						costRatio = (obj.DefRecord.costFactor);

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