using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling.LowLevel.Unsafe;
using Verse;
using Verse.Noise;

namespace BrewingEnhanced
{
    class CompBlend : ThingComp
    {
		public Dictionary<ThingDef, float> BlendIngredients = new Dictionary<ThingDef, float>();

		public CompProperties_Blend PropsBlend
		{
			get
			{
				return (CompProperties_Blend)this.props;
			}
		}

		public void AddIngredient(ThingDef def, float value)
		{
			if( BlendIngredients.ContainsKey(def) )
			{
				BlendIngredients[def] += value;
			}
			else
			{
				BlendIngredients.Add(def, value);
			}
		}

		public override string CompInspectStringExtra()
		{
			return "Blended: " + string.Join(", ", BlendIngredients.ToList().Select(x => x.Key.label + " x" + x.Value.ToString()));
		}
	}

	public class CompProperties_Blend : CompProperties
	{
		public int minTotalItems;
		public List<ThingDef> acceptedDefs;

		public CompProperties_Blend()
		{
			compClass = typeof(CompBlend);
		}
	}
}
