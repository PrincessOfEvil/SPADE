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
		public new readonly int      ticks                    = GenTicks.TickLongInterval;
		protected           IntRange ticksBetweenWandersRange = new IntRange(20, 100);

		protected override Job TryGiveJob(Pawn pawn)
			{
			// This isn't equal to !=: they're opposite when it comes to the null check.
			if ((!pawn.ownership.OwnedBed?.Position.Equals(pawn.Position)) ?? false)
				{
				return JobMaker.MakeJob(JobDefOf.Goto, pawn.ownership.OwnedBed.Position);
				}
			else if (pawn.ownership.OwnedBed == null)
				{
				IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(pawn.Position, pawn, vec => Validator(vec, pawn));
				if (intVec.IsValid && intVec != pawn.Position)
					{
					return JobMaker.MakeJob(JobDefOf.Goto, intVec);
					}
				}
			Job job                = JobMaker.MakeJob(JobDefOf.Wait);
			job.expiryInterval = this.ticks;
			job.overrideFacing = pawn.mindState?.duty?.overrideFacing ?? Rot4.Invalid;
			return job;
			}

		protected static bool Validator(IntVec3 vec, Pawn pawn) 
			{
			Map map = pawn.Map;
			if (DebugViewSettings.drawDestSearch) 
				{
				var text =
					(!(map.thingGrid.ThingAt(vec, ThingCategory.Building)?.Equals(pawn.ownership.OwnedBed) ?? true) ? "b" : " ") +
					(map.thingGrid.CellContains(vec, ThingCategory.Item) ? "i" : " ") +
					(!(map.thingGrid.ThingAt(vec, ThingCategory.Pawn)?.Equals(pawn) ?? true) ? "p" : " ") +
					(map.thingGrid.CellContains(vec, ThingCategory.Plant) ? "a" : " ");
				map.debugDrawer.FlashCell(vec, text: text, colorPct: String.IsNullOrWhiteSpace(text) ? 0.5f : 0f);
				}
			return !(map.thingGrid.CellContains(vec, ThingCategory.Building) || map.thingGrid.CellContains(vec, ThingCategory.Item) || !(map.thingGrid.ThingAt(vec, ThingCategory.Pawn)?.Equals(pawn) ?? true ) || map.thingGrid.CellContains(vec, ThingCategory.Plant));
			}
		}
    }
