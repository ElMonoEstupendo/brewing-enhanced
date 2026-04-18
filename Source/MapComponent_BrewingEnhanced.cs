using System.Collections.Generic;
using Verse;

namespace BrewingEnhanced
{
	public class MapComponent_BrewingEnhanced : MapComponent
	{
		public List<CompBlend> listerSecondaries = new List<CompBlend>();
		public MapComponent_BrewingEnhanced(Map map) : base(map)
		{
			Log.Message("Adding the brewing map component. You know what that means...");
		}

		public void Add(CompBlend blend)
		{
			listerSecondaries.AddDistinct(blend);
		}

		public void Remove(CompBlend blend)
		{
			listerSecondaries.Remove(blend);
		}
    }
}
