using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using WM.SmarterFoodSelection.Detours;

namespace WM.SmarterFoodSelection
{
	//TODO: include Thing and FoodDefRecord to FoodScore
	public class FoodSourceRating
	{
		public class Component
		{
			public Component(string name, float value, bool hidden)
			{
				Name = name;
				Value = value;
				Hidden = hidden;
			}

			public string Name { get; internal set; }
			public float Value { get; internal set; }
			public bool Hidden { get; internal set; }
		}

		public Thing FoodSource { get; set; }
		public FoodDefRecord DefRecord { get; set; }
		public List<Component> ScoreComps { get; set; }

		public FoodSourceRating()
		{
			Score = 0f;
			ScoreComps = new List<Component>();
			ScoreComps.Add(new Component("Base", 500f, true));
		}

		public static implicit operator float(FoodSourceRating obj)
		{
			return obj.Score;
		}

		public float Score { get; private set; }
		public float ScoreForceSum
		{
			get
			{
				return ScoreComps.Sum((arg) => arg.Value);
			}
		}

		public void AddComp(string name, float value, bool hidden = false)
		{
			ScoreComps.Add(new Component(name, value, hidden));
			Score += value;
		}
		public void SetComp(string name, float value, bool hidden = false)
		{
			var target = GetComp(name);
			target.Value = value;
			target.Hidden = hidden;
		}
		public Component GetComp(string name)
		{
			return ScoreComps.Find((obj) => obj.Name == name);
		}

		internal string ToWidgetString(bool advancedInfo, FoodCategory category, out float overridedScore, float overrideDistanceFactor = 0f)
		{
			string text = "";


			var backupDistanceValue = GetComp("Distance").Value;
			SetComp("Distance", overrideDistanceFactor);

			float score = ScoreForceSum;
			overridedScore = score;
			text += score.ToString("F0");

			if (advancedInfo)
			{
				text += "\n" + category;

				foreach (var comp in ScoreComps)
				{
					if (comp.Hidden || comp.Value == 0f)
						continue;

					text += string.Format("\n{0} ({1:F0})", comp.Name, comp.Value);
				}

			}
			SetComp("Distance", backupDistanceValue);

			return text;
		}

	}

}