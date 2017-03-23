using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WM.SmarterFoodSelection
{
	public static class Policies
	{
		//public static readonly Policy Default = new Policy
		//{
		//	moodEffectMatters = true,
		//	Active = true,
		//	description = "DefaultColonistsFoodPolicy".Translate(),
		//	allowUnlisted = true,
		//	unrestricted = true
		//};
		//public static readonly Policy Default = Unrestricted;
		public static readonly Policy Unrestricted = new Policy
		{
			defName = "WM_Unrestricted_Policy",
			label = "UnrestrictedPolicyLabel".Translate(),
			moodEffectMatters = true,
			unrestricted = true,
			conditionPredicate = (PawnPair arg) => true,
			description = "Same behavior as vanilla."
		};
		// should never be used by user
		public static readonly Policy Wild = new Policy
		{
			defName = "WM_Wild_Policy",
			label = "Wild animals",
			unrestricted = true,
			distanceFactor = 1f,
			pawnMasks = { new PawnMask { pawnType = new MaskAttribute(PawnMaskType.WildAnimal), factionCategory = new MaskAttribute(PawnMaskFaction.Wild) } }
		};

		// Hardcoded policy for non player non wild pawns.
		// user should not be allowed to set a policy for those.
		public static readonly Policy FriendlyPets = new Policy
		{
			defName = "hidden_friendlypets_policy",
			label = "Friendly (pets)",
			Visible = false,
			conditions = new List<string>
			{
				new string("pet/friendly".ToCharArray())
			},
			baseDiet =
			{
				new Diet.DietElement() {foodCategory = FoodCategory.Grass},
				new Diet.DietElement() {foodCategory = FoodCategory.HumanlikeCorpse}, 
				new Diet.DietElement() {foodCategory = FoodCategory.Hay},
				new Diet.DietElement() {foodCategory = FoodCategory.Kibble},
				new Diet.DietElement() {foodCategory = FoodCategory.MealAwful},
				new Diet.DietElement() {foodCategory = FoodCategory.MealSimple},
				new Diet.DietElement() {foodCategory = FoodCategory.RawHuman},
				new Diet.DietElement() {foodCategory = FoodCategory.RawInsect},
				new Diet.DietElement() {foodCategory = FoodCategory.RawBad},
				new Diet.DietElement() {foodCategory = FoodCategory.InsectCorpse},
				new Diet.DietElement() {foodCategory = FoodCategory.Corpse},
				new Diet.DietElement() {foodCategory = FoodCategory.RawTasty},
				new Diet.DietElement() {foodCategory = FoodCategory.MealFine},
				new Diet.DietElement() {foodCategory = FoodCategory.MealLavish}
			}
		};

		internal static readonly Policy Taming = new Policy
		{
			defName = "hidden_taming_policy",
			label = "Taming", 
			Visible = false,
			conditionPredicate = ((PawnPair arg) => arg.eater.isWildAnimal() && arg.getter.IsColonist),
			allowFoodPredicate = delegate (Thing food)
			{
				var category = food.DetermineFoodCategory();
				if (!Config.useHumanlikeCorpsesForTaming && category == FoodCategory.HumanlikeCorpse)
					return false;
				if (!Config.useCorpsesForTaming && category == FoodCategory.Corpse)
					return false;
				if (!Config.useMealsForTaming && (food.def.ingestible.foodType & FoodTypeFlags.Meal) == FoodTypeFlags.Meal)
					return false;

				return true;
			},
			baseDiet =
			{
				new Diet.DietElement() {foodCategory = FoodCategory.HumanlikeCorpse},
				new Diet.DietElement() {foodCategory = FoodCategory.Hay},
				new Diet.DietElement() {foodCategory = FoodCategory.Kibble},
				new Diet.DietElement() {foodCategory = FoodCategory.RawHuman},
				new Diet.DietElement() {foodCategory = FoodCategory.RawInsect},
				new Diet.DietElement() {foodCategory = FoodCategory.RawBad},
				//new Diet.DietElement() {foodCategory = FoodCategory.MealAwful},
				//new Diet.DietElement() {foodCategory = FoodCategory.MealSimple},
				new Diet.DietElement() {foodCategory = FoodCategory.InsectCorpse},
				new Diet.DietElement() {foodCategory = FoodCategory.Corpse}
			}
		};

		public static readonly Policy Friendly = new Policy
		{
			defName = "hidden_friendly_policy",
			label = "Friendly",
			Visible = false,
			conditions = new List<string>
			{
				new string("human/friendly".ToCharArray())
			},
			//unrestricted = true
			moodEffectFactor = 1.5f, // we're guests after all
			allowUnlisted = true,
			baseDiet =
			{
				new Diet.DietElement() {foodCategory = FoodCategory.MealLavish},
				new Diet.DietElement() {foodCategory = FoodCategory.MealFine},
				new Diet.DietElement() {foodCategory = FoodCategory.MealSimple},
				new Diet.DietElement() {foodCategory = FoodCategory.MealAwful},
				new Diet.DietElement() {foodCategory = FoodCategory.RawTasty},
				new Diet.DietElement() {foodCategory = FoodCategory.Kibble},
				new Diet.DietElement() {foodCategory = FoodCategory.RawInsect},
				new Diet.DietElement() {foodCategory = FoodCategory.RawBad},
				new Diet.DietElement() {foodCategory = FoodCategory.InsectCorpse},
				new Diet.DietElement() {foodCategory = FoodCategory.Corpse},
				new Diet.DietElement() {foodCategory = FoodCategory.RawHuman},
			}
		};

		public static IEnumerable<Policy> AllPolicies
		{
			get
			{
				return DefDatabase<Policy>.AllDefs.Where((arg) => arg.ValidPolicy);
			}
		}
		public static IEnumerable<Policy> AllVisiblePolicies
		{
			get
			{
				return AllPolicies.Where(arg => arg.Visible);
			}
		}


		//public static IEnumerable<Policy> AllPetPolicies
		//{
		//	get
		//	{
		//		return AllPolicies.Where((arg) => arg.assignablePawns.pets);
		//	}
		//}

		internal static IEnumerable<PawnMask> AllPawnMasks
		{
			get
			{
				var list = new List<PawnMask>();

				foreach (var policy in Policies.AllPolicies)
				{
					list.AddRange(policy.pawnMasks);
				}

				return list.Distinct();
			}
		}

		//public static readonly Policy DefaultColonists = AllPolicies.DefaultIfEmpty(Unrestricted).FirstOrDefault((obj) => obj.assignablePawns.colonists && obj.targetDefault);
		//public static readonly Policy DefaultPrisoners = AllPolicies.DefaultIfEmpty(Unrestricted).FirstOrDefault((obj) => obj.assignablePawns.prisoners && obj.targetDefault);
		//public static readonly Policy DefaultAnimals = AllPolicies.DefaultIfEmpty(Unrestricted).FirstOrDefault((obj) => obj.assignablePawns.pets && obj.targetDefault);

		public static IEnumerable<Policy> GetAllPoliciesForPawn(Pawn pawn)
		{
			return AllPolicies.Where((Policy arg) => arg.AdmitsPawn(pawn));
		}

		internal static void BuildMaskTree()
		{
			foreach (var item in AllPawnMasks)
			{
				var possibleParents = AllPawnMasks.Where((PawnMask arg) => arg.AllSpecifiedAttributes().Count() < item.AllSpecifiedAttributes().Count() && arg.CanBeParentOrEqualOf(item));

				if (possibleParents.Any())
				{
					var groupedList = possibleParents.GroupBy((arg) => arg.AllSpecifiedAttributes().Count()).OrderByDescending((arg) => arg.Key);
					if (groupedList.First().Count() > 1)
					{
						Log.Warning("More than one possible parent for mask " + item);
					}
					item.Parent = groupedList.First().First();
					item.Parent.Children.Add(item);
				}
			}
		}
	}
}
