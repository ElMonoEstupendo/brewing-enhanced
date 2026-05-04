using Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace BrewingEnhanced.Work
{
    public class WorkGiver_OpenFermenter : WorkGiver_Scanner
    {
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			MapComponent_BrewingEnhanced map_comp = pawn.Map.GetComponent<MapComponent_BrewingEnhanced>();
			if( null == map_comp ) { yield break; }

			foreach( var item in map_comp.listerFermenters )
			{
				if( item.ToOpen && !item.Opened) yield return item.parent;
			}

			yield break;
		}

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompBlend blend = t.TryGetComp<CompBlend>();
			if( null == blend ) { return false; }
			if( blend.Opened ) { return false; }
			if( !blend.ToOpen ) { return false; }
			if( t.IsBurning() ) { return false; }
			if( !pawn.CanReserve(t, ignoreOtherReservations: forced) ){ return false; }
			if( pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null ) {  return false; }

			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(BEDefOfs.OpenFermenter, t);
		}
	}
}
