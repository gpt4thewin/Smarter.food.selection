using Verse;

namespace WM.SmarterFoodSelection
{
	public class MyDefClass : Def
	{
		static int defsCount = 0;

		public virtual void DefsLoaded()
		{
		}
		public override void PostLoad()
		{
			base.PostLoad();

			if (defName == DefaultDefName)
			{
				//int defsCount = Verse.DefDatabase<MyDefClass>.AllDefs.Count();

				defName = string.Format("{0}Def{1}", this.GetType().Name, defsCount);

				defsCount++;
			}
		}
	}
}
