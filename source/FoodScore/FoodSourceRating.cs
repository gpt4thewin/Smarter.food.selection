using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WM.SmarterFoodSelection
{
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

			public Component(Component obj)
			{
				Name = obj.Name;
				Value = obj.Value;
				Hidden = obj.Hidden;
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

		public FoodSourceRating(FoodSourceRating obj)
		{
			Score = 0f;
			FoodSource = obj.FoodSource;
			DefRecord = obj.DefRecord;
			ScoreComps = new List<Component>();

			foreach (var item in obj.ScoreComps)
			{
				AddComp(item.Name, item.Value, item.Hidden);
			}
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

		internal string ToWidgetString(bool advancedInfo, FoodCategory category)
		{
			string text = "";

			var component = GetComp("Distance");
			float backupDistanceValue;

			if (component != null)
				backupDistanceValue = component.Value;
			else
				backupDistanceValue = 0f;

			//SetComp("Distance", overrideDistanceFactor);

			float score = ScoreForceSum;
			text += score.ToString("F0");

			if (advancedInfo)
			{
				text += "\n" + category;

				foreach (var comp in ScoreComps)
				{
					if (comp.Hidden || System.Math.Abs(comp.Value) < float.Epsilon)
						continue;

					text += string.Format("\n{0} ({1:F0})", comp.Name, comp.Value);
				}
			}
			if (component != null)
			{
				SetComp("Distance", backupDistanceValue);
			}

			return text;
		}

	}

}