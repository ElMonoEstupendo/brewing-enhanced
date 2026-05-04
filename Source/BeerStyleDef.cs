using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BrewingEnhanced
{
    public class BeerStyleDef : Def
    {
		public string Label;
		public Dictionary<ThingDef, MinMaxInt> ItemRanges;
		public List<ThingDef> AcceptedSecondaryIngredients;
		public bool NeedsSecondary = true;
		public bool PrependSecondary = false;
		public bool mustOpen = false;
		public int MinTemp;
		public int MaxTemp;
		public int Priority = 0;
		public string TastingNotes = "No notes. Yet!";

		public const float RatioTolerance = 0.1f;

		public bool AcceptsBlend(CompBlend blend)
		{
			if( AcceptedSecondaryIngredients != null )
			{
				if( blend?.SecondaryItem?.stockedDef != null )
				{
					if( !AcceptedSecondaryIngredients.Contains(blend.SecondaryItem.stockedDef) ) { return false; }
				} else if( NeedsSecondary )
				{
					return false;
				}
			}

			if( ItemRanges != null )
			{
				foreach( var kvp in ItemRanges )
				{
					int present = blend.BlendItems.ContainsKey(kvp.Key) ? blend.BlendItems[kvp.Key] : 0;
					if( !kvp.Value.IsInRange(present) ) { return false; }
				}
			}

			return true;
		}

		public string GetLabelForBlend(CompBlend blend)
		{
			if( blend.Style != this ) { return "STYLE ERROR!"; }
			if( PrependSecondary ) { return blend.SecondaryItem.stockedDef.LabelCap + " " + Label; }
			return Label;
		}

		public bool IsValidTemperature(float temperature)
		{
			if( temperature > MaxTemp ) return false;
			if( temperature < MinTemp ) return false;
			return true;
		}

		public string Describe(CompBlend blend)
		{
			string ret = TastingNotes + "\r\n\r\nThis beer is categorised as " + Label + " due to:\r\n";
			if( ItemRanges != null )
			{
				foreach( var kvp in ItemRanges )
				{
					ret += "\t" + kvp.Key.LabelCap + "(" + (blend.BlendItems.ContainsKey(kvp.Key) ? blend.BlendItems[kvp.Key] : 0).ToString() + "): " + kvp.Value.min.ToString() + "-" + kvp.Value.max.ToString() + "\r\n";
				}
			}
			if( AcceptedSecondaryIngredients != null && blend.SecondaryItem?.stockedDef != null )
			{
				ret += "Dry Hopping: " + blend.SecondaryItem.stockedDef.LabelCap + "\r\n";
			}
			ret += "Brewed between " + MinTemp.ToString() + " and " + MaxTemp.ToString() + " degrees.\r\n";

			return ret;
		}
    }
}
