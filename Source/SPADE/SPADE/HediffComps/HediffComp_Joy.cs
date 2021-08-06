using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_Joy : HediffComp
        {
        public HediffCompProperties_Joy Props => (HediffCompProperties_Joy)props;
        public override void CompPostTick(ref float severityAdjustment)
            {
            Pawn.needs.joy.GainJoy(parent.Severity * Props.joyPerTick, Props.joyKind);
            }
        }

    public class HediffCompProperties_Joy : HediffCompProperties
        {
        public float joyPerTick = 3.5E-05f;
        public JoyKindDef joyKind;

        public HediffCompProperties_Joy() 
            {
            compClass = typeof(HediffComp_Joy);
            }
        }
    }