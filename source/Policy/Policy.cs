using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace WM.SmarterFoodSelection
{
	public class Policy : MyDefClass
	{
		internal const float DEFAULT_DISTANCE_FACTOR = 0.8f; // vanilla is 1f

		// -------- Basic Parameters -------- 

		//public PolicyAssignable assignablePawns;
		public List<string> conditions;
		//public bool targetDefault = false;
		public List<ThingDef> targetPawnDefs = new List<ThingDef>(); //TODO: implement custom target pawn defs
		public bool moodEffectMatters = false;
		public string diet = "";

		// -------- Uncommon parameters -------- 

		public bool allowUnlisted = false;
		public bool unrestricted = false; // should only be used for the "unrestricted" policy
		public bool optimalityOffsetMatters = true;

		// -------- Advanced parameters -------- 

		public float moodEffectFactor = 1f;
		public float distanceFactor = DEFAULT_DISTANCE_FACTOR;
		public float costFactorMultiplier = 1f;
		public SimpleCurve FoodOptimalityEffectMoodCurve = Detours.Access.FoodOptimalityEffectFromMoodCurve;

		// -------- Hardcoded parameters -------- 

		internal Func<PawnPair, bool> pawnValidator = null;
		internal Func<FoodCategory, bool> foodcategoryValidator = null;
		internal Func<Thing, bool> sourceValidator = null;

		// -------- Parsed stuff -------- 

		internal List<PawnMask> pawnMasks = new List<PawnMask>();
		internal List<Diet.DietElement> baseDiet = new List<Diet.DietElement>();

		internal Dictionary<ThingDef, Diet> PerRacesDiet = new Dictionary<ThingDef, Diet>();

		// -------- Properties and stuff -------- 

		internal bool Visible { get; set; }
		internal bool ValidPolicy { get; set; }

		private static int PoliciesCount = 0;

		public Policy()
		{
			// Unity wants C# 5 or bellow
			Visible = true;
			ValidPolicy = true;
		}

		public string UniqueID
		{
			get
			{
				return label + "_" + defName;
			}
		}

		internal static Policy Named(string value)
		{
			return Policies.AllPolicies.First(arg => arg.defName == value);
		}

		public override void PostLoad()
		{
			if (defName == "")
				Log.Warning("Policy with label \"" + label + "\" has no defName is getting a default one. This may cause problems when assigning them.");
			base.PostLoad();
		}

		public override void DefsLoaded()
		{
			try
			{
				_DefsLoaded();
#if DEBUG
				Verse.Log.Message(string.Format("Recorded policy {1} \"{0}\". {3} diet elements. {2} races diets.", label, defName, PerRacesDiet != null ? PerRacesDiet.Count : 0, baseDiet != null ? baseDiet.Count : 0));
#endif
			}
			catch (Exception ex)
			{
				ValidPolicy = false;
				Verse.Log.Error(string.Format("Error when recording policy \"{2}\":\n{0}\n{1}", ex, ex.StackTrace, label != null ? label : "(noname)"));
				return;
			}
		}

		//TODO: print more about syntax errors
		private void _DefsLoaded()
		{
			if (unrestricted)
				allowUnlisted = true;

			//assignablePawns.DefsLoaded();

			if (label == "" || label == null)
			{
				label = string.Format("Policy #{1} for {0}", (conditions != null && conditions.Any()) ? string.Join(" / ", conditions.ToArray()) : "everyone", PoliciesCount);
			}

			// ----------- Parse masks -----------

			if (conditions != null)
			{
				foreach (var textmask in conditions)
				{
					pawnMasks.Add(PawnMask.Parse(textmask, this));
				}
				var pawnMasks2 = pawnMasks.Distinct().ToList();

				if (pawnMasks2.Count() < pawnMasks.Count())
				{
					var list = pawnMasks2.Where((arg) => !pawnMasks.Any((arg2) => arg2 == arg)).Select((arg) => arg.ToString());
					Log.Warning("Policy " + label + " has redundant conditions: " + list);
				}

				pawnMasks = pawnMasks2;
			}

			// ----------- Parse base diet -----------

			if (diet != null && diet.Any())
			{
				//	var diet = new Diet();
				var dietElements = Regex.Replace(diet, "[\n\r\t ]", "").Split('/');

				if (dietElements.Any())
					foreach (var item in dietElements)
					{
						float offset = Diet.DietElement.DefaultOffset;
						var levelElements = item.Split('=');

						foreach (var item2 in levelElements)
						{
							bool customOffsetDefined = false;
							float num;

							//not implemented
							if (float.TryParse(item2, out num))
							{
								if (customOffsetDefined)
									Log.Warning("Diet level has several custom offset values set in policy " + this.ToString());

								offset = num;
								customOffsetDefined = true;
							}
							else
							{
								baseDiet.Add(new Diet.DietElement()
								{
									foodCategory = (FoodCategory)Enum.Parse(typeof(FoodCategory), item2),
									scoreOffset = 0
								});
							}
						}

						//todo: fix custom offset 
						baseDiet.Last().scoreOffset = -Math.Abs(offset);
					}
			}

			// ----------- Create diets for every related races -----------

			bool hasCustomPawnDefsSet = targetPawnDefs.Any();

			foreach (var current in DefDatabaseHelper.AllPawnDefs)
			{
				if (AdmitsPawnDef(current))
				{
					if (!hasCustomPawnDefsSet)
						targetPawnDefs.Add(current);

					PerRacesDiet.Add(current, new Diet(current, this));
				}
			}

			PoliciesCount++;
		}

		internal int RacesDietCount
		{
			get
			{
				if (PerRacesDiet == null)
					return 0;
				return PerRacesDiet.Count;
			}
		}

		internal bool AdmitsPawnDef(ThingDef pawn)
		{
			return
				conditions == null ||
				pawnMasks.Any((arg) => arg.MatchesPawnDef(pawn));
		}
		internal bool AdmitsPawn(Pawn pawn)
		{
			return
				conditions == null ||
				pawnMasks.Any((arg) => arg.MatchesPawn(pawn));
		}
		internal bool AdmitsPawnPair(PawnPair pair)
		{
			if (pawnValidator != null && pawnValidator(pair))
				return true;

			return AdmitsPawn(pair.eater);
		}


		internal int IndexOfPref(ThingDef pawnDef, FoodCategory foodPref)
		{
			return PerRacesDiet[pawnDef].elements.FindIndex((obj) => obj.foodCategory == foodPref);
		}

		internal bool PolicyAllows(Pawn pawn, Thing t)
		{
			if (sourceValidator != null && !sourceValidator(t))
				return false;
			
			return PolicyAllows(pawn, t.DetermineFoodCategory());
		}
		internal bool PolicyAllows(Pawn pawn, ThingDef def)
		{
			return PolicyAllows(pawn, def.DetermineFoodCategory());
		}
		internal bool PolicyAllows(Pawn pawn, FoodCategory pref)
		{
			if (foodcategoryValidator != null && !foodcategoryValidator(pref))
				return false;

			if (allowUnlisted || unrestricted)
				return true;

			return GetDietForPawn(pawn).ContainsElement(pref);
		}
		internal bool PolicyAllows(FoodCategory pref)
		{
			if (allowUnlisted || unrestricted)
				return true;

			return baseDiet.Any((obj) => obj.foodCategory == pref);
		}

		public int GetFoodCategoryRankForPawn(Pawn pawn, Thing category)
		{
			return GetFoodCategoryRankForPawn(pawn, category.def);
		}
		public int GetFoodCategoryRankForPawn(Pawn pawn, ThingDef category)
		{
			return GetFoodCategoryRankForPawn(pawn, category.DetermineFoodCategory());
		}
		public int GetFoodCategoryRankForPawn(Pawn pawn, FoodCategory category)
		{
			var diet = GetDietForPawn(pawn);

			//TODO: optimize
			var rank = diet.elements.GroupBy(arg => arg.totalOffsetValue).ToList().FindIndex((obj) => obj.Any((arg) => arg.foodCategory == category));
			//var rank = diet.elements.FindIndex((obj) => obj.foodCategory == category);

			if (rank < 0)
				return diet.elements.Count;

			return rank;
		}

		public Diet GetDietForPawn(Pawn pawn)
		{
			return GetDietForPawn(pawn.def);
		}
		public Diet GetDietForPawn(ThingDef pawn)
		{
			Diet dietForPawn;
			if (!PerRacesDiet.TryGetValue(pawn, out dietForPawn))
			{
				throw new InvalidOperationException(string.Format("Cannot find diet for specie of pawn with active policy. Returning policy base diet. Pawn: {0}. Policy: {1}", pawn, this));
			}

			return dietForPawn;
		}

		//public override string ToString()
		//{
		//	//return string.Format("[FoodPolicy: defName={0}, label={1}]", defName, label);
		//	return label;
		//}
	}
}
