using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace BrewingEnhanced.Work
{
    public class JobDriver_OpenFermenter : JobDriver
    {
		private const TargetIndex FermenterInd = TargetIndex.A;
		private const int Duration = 200;

		protected CompBlend Fermenter
		{
			get
			{
				return this.job.GetTarget(FermenterInd).Thing.TryGetComp<CompBlend>();
			}
		}

		protected Building_FermentingBarrel Barrel
		{
			get
			{
				return this.job.GetTarget(FermenterInd).Thing as Building_FermentingBarrel;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Fermenter.parent, this.job, errorOnFailed: errorOnFailed);
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(FermenterInd);
			this.FailOnBurningImmobile(FermenterInd);
			base.AddEndCondition(delegate
			{
				if( Fermenter.Opened == true ) { return JobCondition.Succeeded; }
				if( Fermenter.ToOpen == false ) { return JobCondition.Incompletable; }
				return JobCondition.Ongoing;
			});

			// Actual toils:
			yield return Toils_Goto.GotoThing(FermenterInd, PathEndMode.ClosestTouch);
			yield return Toils_General.Wait(Duration, TargetIndex.None).FailOnDestroyedNullOrForbidden(FermenterInd).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A);
			Toil finishOff = ToilMaker.MakeToil("OpenFermeneter");
			finishOff.initAction = delegate ()
			{
				Fermenter.Opened = true;
			};
			finishOff.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return finishOff;
			yield break;
		}
	}
}
