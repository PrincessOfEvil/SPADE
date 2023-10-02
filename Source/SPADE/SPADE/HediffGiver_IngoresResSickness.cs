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
        public override bool OnHediffAdded(Pawn pawn, Hediff hed)
            {
            if (ignoredHediffs.Contains(hed.def))
                {
                pawn.health.RemoveHediff(hed);
                }
            return false;
            }
        }
    }
