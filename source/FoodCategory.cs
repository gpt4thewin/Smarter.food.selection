using System;
using Verse;
using RimWorld;

namespace WM.SmarterFoodSelection
{
	public enum FoodCategory : byte
	{
		Null,
		Ignore,
		HumanlikeCorpse,
		InsectCorpse,
		Corpse,
		Hay,
		Kibble,
		RawBad,
		RawTasty,
		RawInsect,
		RawHuman,
		FertEggs,
		AnimalProduct,
		MealAwful,
		MealSimple,
		MealFine,
		MealLavish,
		Plant,
		Grass,
		Luxury,
		SafeHunting,
		RiskyHunting
	}

	//public class FoodCategory
	//{
	//	FoodTypeFlags flags;
	//	public Acceptance[] preferability = new Acceptance[(Enum.GetValues(typeof(FoodPreferability)).Length)];
	//	public Acceptance negativeThoughts = AcceptanceMode.Never;
	//	public Acceptance positiveThoughts;
	//	public Acceptance joy;

	//	public bool AcceptsDef(ThingDef def)
	//	{
	//		var ingestible = def.ingestible;

	//		if (joy.Test(ingestible.joy > 0) == AcceptanceResult.ReturnFalse)
	//			return false;
	//	}

	//}
	//public class Acceptance
	//{
	//	AcceptanceMode mode;

	//	public Acceptance(AcceptanceMode mode)
	//	{
	//		this.mode = mode;
	//	}
	//	public AcceptanceResult Test(bool condition)
	//	{
	//		if (condition)
	//		{
	//			if (mode == AcceptanceMode.Never)
	//				return AcceptanceResult.ReturnFalse;
	//		}
	//		else if (mode == AcceptanceMode.Only)
	//			return AcceptanceResult.ReturnFalse;

	//		return AcceptanceResult.Continue;
	//	}

	//	public static implicit operator AcceptanceMode(Acceptance obj)
	//	{
	//		return obj.mode;
	//	}
	//	public static implicit operator Acceptance(AcceptanceMode arg)
	//	{
	//		return new Acceptance(arg);
	//	}
	//}
	//public enum AcceptanceMode
	//{
	//	Never = 0,
	//	Accept,
	//	Only
	//}
	//public enum AcceptanceResult
	//{
	//	ReturnFalse,
	//	Continue
	//}
}
