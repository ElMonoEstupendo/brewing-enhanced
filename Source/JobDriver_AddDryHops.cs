using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace BrewingEnhanced
{
    public class JobDriver_AddDryHops : JobDriver
    {
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

		protected Thing DryHops
		{
			get
			{
				return this.job.GetTarget(DryHopsInd).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Fermenter.parent, this.job, errorOnFailed: errorOnFailed) && this.pawn.Reserve(this.DryHops, this.job, errorOnFailed: errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			base.AddEndCondition(delegate
				{
					if( this.Barrel.SpaceLeftForWort > 0 )
					{
						return JobCondition.Ongoing;
					}
					return JobCondition.Succeeded;
				});
			base.AddFailCondition(delegate
				{
					if( Fermenter.AcceptingDryHops ) { return false; }
					return true;
				});
			yield return Toils_General.DoAtomic(delegate
			{
				this.job.count = Fermenter.SecondaryItem.wantingCount;
			});
			Toil reserveDryHops = Toils_Reserve.Reserve(TargetIndex.B);
			yield return reserveDryHops;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveDryHops, TargetIndex.B, TargetIndex.None, true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(Fermenter.PropsBlend.DryHoppingTickCount, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate ()
			{
				Fermenter.AddSecondary(DryHops);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield break;
		}

		// Token: 0x0400500A RID: 20490
		private const TargetIndex FermenterInd = TargetIndex.A;

		// Token: 0x0400500B RID: 20491
		private const TargetIndex DryHopsInd = TargetIndex.B;

		// Token: 0x0400500C RID: 20492
		private const int Duration = 200;
	}
}
