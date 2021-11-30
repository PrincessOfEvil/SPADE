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
    public class ThinkNode_JobGiver_Idle_NotOnABuilding : JobGiver_Idle
		{
		protected override Job TryGiveJob(Pawn pawn)
			{
			IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(pawn.Position, pawn, vec => Validator(vec, pawn.Map));
			if (intVec.IsValid && intVec != pawn.Position)
				{
				return JobMaker.MakeJob(JobDefOf.Goto, intVec);
				}
			Job job = JobMaker.MakeJob(JobDefOf.Wait);
			job.expiryInterval = this.ticks;
			job.overrideFacing = pawn.mindState?.duty?.overrideFacing ?? Rot4.Invalid;
			return job;
			}

		protected static bool Validator(IntVec3 vec, Map map) 
			{
			return (map.thingGrid.CellContains(vec, ThingCategory.Building) || map.thingGrid.CellContains(vec, ThingCategory.Item) || map.thingGrid.CellContains(vec, ThingCategory.Pawn) || map.thingGrid.CellContains(vec, ThingCategory.Plant));
			}
		}
    }
