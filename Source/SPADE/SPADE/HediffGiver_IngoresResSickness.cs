using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffGiver_IngoresResSickness : HediffGiver
        {
        IEnumerable<HediffDef> listToCheck = new List<string>() { "ResurrectionSickness", "Dementia", "Blindness", "ResurrectionPsychosis" }.Select(hediff => { return DefDatabase<HediffDef>.GetNamed(hediff); });
        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
            {
            if (listToCheck.Contains(hediff.def))
                {
                pawn.health.RemoveHediff(hediff);
                }
            return false;
            }
        }
    }
