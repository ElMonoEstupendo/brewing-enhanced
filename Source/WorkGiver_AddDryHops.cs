using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.Noise;
using Defs;

namespace BrewingEnhanced
{
    public class WorkGiver_AddDryHops : WorkGiver_Scanner
    {
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			MapComponent_BrewingEnhanced map_comp = pawn.Map.GetComponent<MapComponent_BrewingEnhanced>();
			if( null == map_comp ) { yield break; }

			foreach(var item in map_comp.listerSecondaries.Where(b => b.PropsBlend.IsFermenter))
			{
				yield return item.parent;
			}

			yield break;
		}

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			// Valid to add dry hops?
			CompBlend blend = t.TryGetComp<CompBlend>();
			if( !(blend?.AcceptingDryHops ?? true) ){ return false; }

			// Barrel is OK?
			if( t.IsBurning()  ) { return false; }
			if( !pawn.CanReserve(t, ignoreOtherReservations: forced) ){ return false; }
			if( pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null ){ return false; }

			// See if the stuff is around.
			if( this.FindDryHops(pawn, blend) == null )
			{
				JobFailReason.Is("No dry hops available to " + pawn.Label + ".");
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(BEDefOfs.AddDryHops, t, this.FindDryHops(pawn, t.TryGetComp<CompBlend>()));
		}

		private Thing FindDryHops(Pawn pawn, CompBlend blend)
		{
			if( blend?.SecondaryItem?.stockedDef == null ) { return null; }

			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);

			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, 
				ThingRequest.ForDef(blend.SecondaryItem.stockedDef), PathEndMode.ClosestTouch, 
				TraverseParms.For(pawn), validator: validator);
		}
	}
}
