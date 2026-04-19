using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BrewingEnhanced
{
    public class BeerStyleDef : Def
    {
		public string Label;
		public Dictionary<List<ThingDef>, float> IngredientRatios = null;
		public ThingDef SecondaryIngredient = null;
		public int MinTemp;
		public int MaxTemp;
		public int Priority = 0;

		public const float RatioTolerance = 0.1f;

		public bool AcceptsBlend(CompBlend blend)
		{
			if( SecondaryIngredient != null )
			{
				if( blend.SecondaryItem.stockedDef != SecondaryIngredient ) { return false; }
			}

			if( IngredientRatios != null )
			{
				foreach( var ratio in IngredientRatios )
				{
					int valid_count = ratio.Key.Where(x => blend.BlendItems.ContainsKey(x)).Sum(x => blend.BlendItems[x]);
					float valid_ratio = valid_count / blend.TotalItemCount;
					if( ratio.Value > valid_ratio + RatioTolerance || ratio.Value < valid_ratio - RatioTolerance )
					{
						return false;
					}
				}
			}

			return true;
		}

		public bool IsValidTemperature(float temperature)
		{
			if(temperature > MaxTemp) return false;
			if(temperature < MinTemp) return false;
			return true;
		}
    }
}
