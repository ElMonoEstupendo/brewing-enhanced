using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BrewingEnhanced
{
	public class CompProperties_Blend:CompProperties
	{
		public List<ThingDef> AcceptedDefs;
		public List<ThingDefCountClass> SecondaryItems;
		public int ReductionFactor;
		public bool IsFermenter = false;
		public float MaxDryHoppingProgress;
		public int DryHoppingTickCount;

		public CompProperties_Blend()
		{
			compClass = typeof(CompBlend);
		}
	}
}
