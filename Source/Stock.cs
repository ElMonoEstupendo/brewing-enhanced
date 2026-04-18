using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BrewingEnhanced
{
    public class Stock : IExposable
    {
		public ThingDef stockedDef;
		public int targetCount = 0;
		public int currentCount = 0;

		public int wantingCount => Math.Max(0, targetCount - currentCount);
		public float PercentageSatisfied => ((float)currentCount * 100) / targetCount;
		public bool Satisfied => currentCount >= targetCount;
		public string Description => stockedDef.label + " " + PercentageSatisfied.ToString() + "%";

		public Stock(ThingDef def, int target)
		{
			stockedDef = def;
			targetCount = target;
			currentCount = 0;
		}

		public Stock(Stock oldStock)
		{
			stockedDef = oldStock.stockedDef;
			targetCount = oldStock.targetCount;
			currentCount = oldStock.currentCount;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref stockedDef, "def");
			Scribe_Values.Look(ref targetCount, "targetCount");
			Scribe_Values.Look(ref currentCount, "currentCount");
		}
    }
}
