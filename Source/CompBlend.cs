using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace BrewingEnhanced
{
    class CompBlend : ThingComp
    {
		public Dictionary<ThingDef, int> BlendItems = new Dictionary<ThingDef, int>();
		public Dictionary<ThingDef, float> BlendFractions = new Dictionary<ThingDef, float>();
		public Dictionary<ThingDef, int> PreviousItems = null;
		public Dictionary<ThingDef, int> ReducedItems => BlendItems.ToDictionary(bi => bi.Key, bi => bi.Value / PropsBlend.reductionFactor);
		public int TotalItemCount => BlendItems.Select(x => x.Value).Sum();
		public int ReducedItemCount => TotalItemCount / PropsBlend.reductionFactor;

		public CompProperties_Blend PropsBlend
		{
			get
			{
				return (CompProperties_Blend)this.props;
			}
		}

		public void Reset()
		{
			PreviousItems = BlendItems.ToDictionary(bi => bi.Key, bi => bi.Value);
			BlendItems.Clear();
			BlendFractions.Clear();
		}

		public void Add(ThingDef def, int value)
		{
			Log.Message("Blending " + value.ToString() + " of " + def.label);
			if( BlendItems.ContainsKey(def) )
			{
				BlendItems[def] += value;
			} else
			{
				BlendItems.Add(def, value);
			}
			UpdateFractions();
		}

		public void Add(CompBlend newBlend, float multiplier = 1.0f, bool usePrevious = false)
		{
			Dictionary<ThingDef, int> dict = (usePrevious ? newBlend.PreviousItems : newBlend.BlendItems);
			foreach(var item in dict )
			{
				Add(item.Key, (int)(item.Value * multiplier));
			}
		}

		public void UpdateFractions()
		{
			float totalItems = TotalItemCount;
			BlendFractions.Clear();
			foreach(ThingDef ingredient in BlendItems.Keys)
			{
				BlendFractions[ingredient] = (float)BlendItems[ingredient] / totalItems;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref BlendItems, "BlendIngredients", LookMode.Def);
			if(BlendItems == null) BlendItems = new Dictionary<ThingDef, int>();
			UpdateFractions();
		}

		public override string CompInspectStringExtra()
		{
			return "Blend: " + (BlendFractions.Keys.Count == 0 ? "Empty" : string.Join(", ", BlendFractions.Select(x => (x.Value * 100).ToString() + "% " + x.Key.label)));
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			yield return new Command_Action
			{
				defaultLabel = "Spawn Ingredients",
				icon = ContentFinder<Texture2D>.Get("things/items/resource/mash", true),
				action = delegate ()
				{
					ThingDef def = ThingDefOf.Wort;
					CompProperties_Blend wort_blend = def.GetCompProperties<CompProperties_Blend>();
					if( wort_blend != null )
					{
						foreach(var item in wort_blend.acceptedDefs)
						{
							Log.Message("Spawning " + item.stackLimit.ToString() + "x " + item.label);
							GenSpawn.Spawn(item, parent.Position, parent.Map).stackCount = item.stackLimit;
						}
					}
				},
				defaultDesc = "Spawn some basic brewing ingredients."
			};

			yield break;
		}
	}

	public class CompProperties_Blend : CompProperties
	{
		public int capacity;
		public List<ThingDef> acceptedDefs;
		public int reductionFactor;

		public CompProperties_Blend()
		{
			compClass = typeof(CompBlend);
		}
	}
}
