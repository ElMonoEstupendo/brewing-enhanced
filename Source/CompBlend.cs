using Defs;
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
		public Stock SecondaryItem = null;
		public float TotalProgress = 0.0f;
		public Dictionary<BeerStyleDef, int> TicksInStyle = new Dictionary<BeerStyleDef, int>();
		public bool DelayReset = false;
		public BeerStyleDef Style = null;

		// Derived
		public Dictionary<ThingDef, float> BlendFractions => BlendItems.ToDictionary(x => x.Key, x => (float)x.Value / TotalItemCount);
		public Dictionary<ThingDef, int> ReducedItems => BlendItems.ToDictionary(bi => bi.Key, bi => bi.Value / PropsBlend.ReductionFactor);
		public int TotalItemCount => BlendItems.Select(x => x.Value).Sum();
		public int ReducedItemCount => TotalItemCount / PropsBlend.ReductionFactor;
		public bool AcceptingDryHops => !(SecondaryItem?.Satisfied ?? true) && (TotalProgress < PropsBlend.MaxDryHoppingProgress);
		public bool IsFermenting => PropsBlend.IsFermenter && ((parent as Building_FermentingBarrel).Fermented == false) && (TotalItemCount > 0);
		public List<string> BlendStrings => BlendFractions.NullOrEmpty() ? new List<string>() { "Empty" } : BlendFractions.Select(x => ( x.Value * 100 ).ToString() + "% " + x.Key.label).ToList();

		public CompProperties_Blend PropsBlend
		{
			get
			{
				return (CompProperties_Blend)this.props;
			}
		}

		public CompBlend() : base()
		{
			TicksInStyle = DefDatabase<BeerStyleDef>.AllDefs.ToDictionary(x => x, x => 0);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref BlendItems, "BlendIngredients", LookMode.Def);
			Scribe_Deep.Look(ref SecondaryItem, "SecondaryItem");
			Scribe_Values.Look(ref TotalProgress, "TotalProgress");
			Scribe_Collections.Look(ref TicksInStyle, "TicksInStyle");
			Scribe_Defs.Look(ref Style, "Style");
			if( BlendItems == null ) BlendItems = new Dictionary<ThingDef, int>();
			if( TicksInStyle == null ) TicksInStyle = DefDatabase<BeerStyleDef>.AllDefs.ToDictionary(x => x, x => 0);
		}

		public void Reset()
		{
			if( DelayReset ){ return; }
			BlendItems.Clear();
			BlendFractions.Clear();
			TicksInStyle.Clear();
			SecondaryItem = null;
			ResetListers(parent?.Map);
			TicksInStyle.Clear();
			TicksInStyle = DefDatabase<BeerStyleDef>.AllDefs.ToDictionary(x => x, x => 0);
		}

		public void CullStyles()
		{
			TicksInStyle.RemoveAll(x => !x.Key.AcceptsBlend(this));
		}

		public BeerStyleDef ResolveStyle()
		{
			CullStyles();
			int TotalTicks = TicksInStyle[BEDefOfs.BeerStyle_Off];
			TicksInStyle.Remove(BEDefOfs.BeerStyle_Off);

			List<KeyValuePair<BeerStyleDef, int>> candidates = TicksInStyle.Where(kvp => kvp.Value > TotalTicks / 2).ToList();
			if( candidates.Count == 0 )
			{
				Style = BEDefOfs.BeerStyle_Off;
			} else
			{
				candidates.SortBy(x => x.Key.Priority);
				Style = candidates.Last().Key;
			}
				
			return Style;
		}

		public override bool AllowStackWith(Thing other)
		{
			CompBlend otherBlend = other.TryGetComp<CompBlend>();
			if( otherBlend == null ) { return false; }
			if( Style != null ) { return otherBlend.Style == Style; }
			// Only allow stacking with identical blends.
			foreach( var item in BlendItems )
			{
				if( item.Value != 0 && !otherBlend.BlendItems.ContainsKey(item.Key) ) { return false; }
				if( BlendItems[item.Key] != otherBlend.BlendItems[item.Key] ) { return false; }
			}
			if( SecondaryItem?.stockedDef != otherBlend.SecondaryItem?.stockedDef ) { return false; }
			return true;
		}

		public override string TransformLabel(string label)
		{
			if( parent.def == ThingDefOf.Beer )
			{
				return Style.GetLabelForBlend(this);
			}
			return base.TransformLabel(label);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if( Style != null ) { yield return new StatDrawEntry(BEDefOfs.BrewingEnhanced_BeerStyleCategory, "Style", Style.Label, Style.Describe(this), 1000); }

			yield return new StatDrawEntry(BEDefOfs.BrewingEnhanced_BeerStyleCategory, "Ingredients", string.Join(", ", BlendStrings), 
				string.Join("\r\n", BlendStrings), 200,
				hyperlinks: Dialog_InfoCard.DefsToHyperlinks(BlendItems.Keys.ToList()));
			if( SecondaryItem != null )
			{
				yield return new StatDrawEntry(BEDefOfs.BrewingEnhanced_BeerStyleCategory, "Dry Hopping", SecondaryItem.stockedDef.LabelCap,
				SecondaryItem.stockedDef.LabelCap + " x" + SecondaryItem.currentCount.ToString(), 100,
				hyperlinks: Dialog_InfoCard.DefsToHyperlinks(new List<ThingDef>() { SecondaryItem.stockedDef }));
			}

			yield break;
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

		public void Add(CompBlend newBlend, float multiplier = 1.0f)
		{
			foreach(var item in newBlend.BlendItems)
			{
				Add(item.Key, (int)(item.Value * multiplier));
			}
			if( newBlend.SecondaryItem != null ){ SecondaryItem = new Stock(newBlend.SecondaryItem); }
			foreach(var item in newBlend.TicksInStyle)
			{
				TicksInStyle.SetOrAdd(item.Key, item.Value);
			}
		}

		public void AddSecondary(Thing t)
		{
			if( null == SecondaryItem ) { return; }
			if( t.def != SecondaryItem.stockedDef ) { return; }
			int num_added = Math.Min(t.stackCount, SecondaryItem.wantingCount);
			if( num_added <= 0 ) { return; }
			SecondaryItem.currentCount += num_added;
			t.SplitOff(num_added).Destroy();
			CullStyles();
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			if( !IsFermenting ) { return; }
			float temperature = parent.AmbientTemperature;
			foreach(var item in TicksInStyle.Where(x => x.Key.IsValidTemperature(temperature)).ToList())
			{
				TicksInStyle.Increment(item.Key);
			}
		}

		public override string CompInspectStringExtra()
		{
			string ret = "Blend: " + string.Join(", ", BlendStrings);
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
}
