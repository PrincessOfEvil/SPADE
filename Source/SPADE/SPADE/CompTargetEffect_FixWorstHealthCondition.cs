using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace SPADE
	{
	public class CompTargetEffect_FixWorstHealthCondition : CompTargetEffect
		{

		public override void DoEffectOn(Pawn user, Thing target)
			{
			if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
				{
				Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("spade_Repair"), target, parent);
				job.count = 1;
				user.jobs.TryTakeOrderedJob(job);
				}
			}
		}

	public class JobDriver_Repair : JobDriver
		{
		private const int DurationTicks = 300;

		protected Pawn targetPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		protected Thing Item => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
			{
			if (pawn.Reserve(targetPawn, job, 1, -1, null, errorOnFailed))
				{
				return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
				}
			return false;
			}

		protected override IEnumerable<Toil> MakeNewToils()
			{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = Toils_General.Wait(DurationTicks);
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return toil;
			yield return Toils_General.Do(Execute);
			yield break;
			}

		public void Execute()
			{
			HealthUtility.FixWorstHealthCondition(targetPawn);
			this.Item.SplitOff(1).Destroy(DestroyMode.Vanish);
			}
		}
	}
