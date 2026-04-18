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
    public class CompBlend : ThingComp
    {
		// Base Fields
		public Dictionary<ThingDef, int> BlendItems = new Dictionary<ThingDef, int>();
		public Dictionary<ThingDef, int> PreviousItems = null;
		public Stock SecondaryItem = null;
		public Stock PreviousSecondaryItem = null;
		public float TotalProgress = 0.0f;

		// Derived
		public Dictionary<ThingDef, float> BlendFractions => BlendItems.ToDictionary(x => x.Key, x => (float)x.Value / TotalItemCount);
		public Dictionary<ThingDef, int> ReducedItems => BlendItems.ToDictionary(bi => bi.Key, bi => bi.Value / PropsBlend.ReductionFactor);
		public int TotalItemCount => BlendItems.Select(x => x.Value).Sum();
		public int ReducedItemCount => TotalItemCount / PropsBlend.ReductionFactor;
		public bool AcceptingDryHops => !(SecondaryItem?.Satisfied ?? true) && (TotalProgress < PropsBlend.MaxDryHoppingProgress);

		public CompProperties_Blend PropsBlend
		{
			get
			{
				return (CompProperties_Blend)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref BlendItems, "BlendIngredients", LookMode.Def);
			Scribe_Deep.Look(ref SecondaryItem, "SecondaryItem");
			Scribe_Values.Look(ref TotalProgress, "TotalProgress");
			if(BlendItems == null) BlendItems = new Dictionary<ThingDef, int>();
		}

		public void Reset()
		{
			PreviousItems = BlendItems.ToDictionary(bi => bi.Key, bi => bi.Value);
			PreviousSecondaryItem = new Stock(SecondaryItem);
			BlendItems.Clear();
			BlendFractions.Clear();
			SecondaryItem = null;
			ResetListers(parent?.Map);
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
		}

		public void Add(CompBlend newBlend, float multiplier = 1.0f, bool usePrevious = false)
		{
			Dictionary<ThingDef, int> dict = (usePrevious ? newBlend.PreviousItems : newBlend.BlendItems);
			foreach(var item in dict )
			{
				Add(item.Key, (int)(item.Value * multiplier));
			}
			Stock stok = usePrevious ? newBlend.PreviousSecondaryItem : newBlend.SecondaryItem;
			if( stok != null ){ SecondaryItem = new Stock(stok); }
		}

		public void AddSecondary(Thing t)
		{
			if( null == SecondaryItem) { return; }
			if( t.def != SecondaryItem.stockedDef) { return; }
			int num_added = Math.Min(t.stackCount, SecondaryItem.wantingCount);
			if( num_added <= 0) { return; }
			SecondaryItem.currentCount += num_added;
			t.SplitOff(num_added).Destroy();
		}

		public override string CompInspectStringExtra()
		{
			string ret = "Blend: " + (BlendFractions.Keys.Count == 0 ? "Empty" : string.Join(", ", BlendFractions.Select(x => (x.Value * 100).ToString() + "% " + x.Key.label)));
			if( SecondaryItem != null )
			{
				ret += "\r\nDry Hopping: " + SecondaryItem.Description;
			}
			return ret;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if(DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "Spawn Ingredients",
					icon = ContentFinder<Texture2D>.Get("things/items/resource/mash", true),
					action = delegate ()
					{
						ThingDef def = ThingDefOf.Beer;
						CompProperties_Blend beer_blend = def.GetCompProperties<CompProperties_Blend>();
						if( beer_blend != null )
						{
							List<ThingDef> to_spawn = new List<ThingDef>();
							to_spawn.AddRange(beer_blend.AcceptedDefs);
							to_spawn.AddRange(beer_blend.SecondaryItems.Select(x => x.thingDef));
							foreach(var item in to_spawn)
							{
								Log.Message("Spawning " + item.stackLimit.ToString() + "x " + item.label);
								GenSpawn.Spawn(item, parent.Position, parent.Map).stackCount = item.stackLimit;
							}
						}
					},
					defaultDesc = "Spawn some basic brewing ingredients."
				};
			}

			if( PropsBlend.IsFermenter && (SecondaryItem == null || !SecondaryItem.Satisfied) )
			{
				yield return new Command_Action
				{
					defaultLabel = "Dry Hopping",
					icon = ContentFinder<Texture2D>.Get("things/icons/flavour", false),
					action = delegate()
					{
						List<FloatMenuOption> options = PropsBlend.SecondaryItems.Select(item => new FloatMenuOption(item.thingDef.label, 
							delegate()
							{
								SecondaryItem = new Stock(item.thingDef,  item.count);
								parent.Map.GetComponent<MapComponent_BrewingEnhanced>()?.Add(this);
							})).ToList();
						Find.WindowStack.Add(new FloatMenu(options));
					},
					defaultDesc = "Select a flavouring ingredient for dry hopping."
				};
			}

			yield break;
		}

		private void ResetListers(Map map)
		{
			MapComponent_BrewingEnhanced map_comp = map?.GetComponent<MapComponent_BrewingEnhanced>();
			map_comp?.Remove(this);
		}

		public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
		{
			base.PostDeSpawn(map, mode);
			ResetListers(map);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			ResetListers(previousMap);
		}
	}

	public class CompProperties_Blend : CompProperties
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
