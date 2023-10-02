using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class Verb_ShootStatic : Verb_LaunchProjectileStatic
		{
		public override void WarmupComplete()
			{
			base.WarmupComplete();
			if (currentTarget.Thing is not Pawn {Downed: false } pawn || !CasterIsPawn || CasterPawn.skills == null) return;
			float num  = pawn.HostileTo(caster) ? 170f : 20f;
			float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
			CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
			}

		protected override bool TryCastShot()
			{
			bool num = base.TryCastShot();
			if (num && CasterIsPawn)
				{
				CasterPawn.records.Increment(RecordDefOf.ShotsFired);
				}
			return num;
			}
		}
    }
