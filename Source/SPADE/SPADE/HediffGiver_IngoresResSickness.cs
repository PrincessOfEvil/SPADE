using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffGiver_IngoresHediffs : HediffGiver
        {
        List<HediffDef> ignoredHediffs = new List<HediffDef>();
        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
            {
            if (ignoredHediffs.Contains(hediff.def))
                {
                pawn.health.RemoveHediff(hediff);
                }
            return false;
            }
        }
    }
