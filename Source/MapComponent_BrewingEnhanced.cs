using System.Collections.Generic;
using Verse;

namespace BrewingEnhanced
{
	public class MapComponent_BrewingEnhanced : MapComponent
	{
		public List<CompBlend> listerFermenters = new List<CompBlend>();
		public MapComponent_BrewingEnhanced(Map map) : base(map)
		{
			Log.Message("Adding the brewing map component. You know what that means...");
		}

		public void Add(CompBlend blend)
		{
			if( blend.PropsBlend.IsFermenter ){ listerFermenters.AddDistinct(blend); }
		}

		public void Remove(CompBlend blend)
		{
			listerFermenters.Remove(blend);
		}
    }
}
