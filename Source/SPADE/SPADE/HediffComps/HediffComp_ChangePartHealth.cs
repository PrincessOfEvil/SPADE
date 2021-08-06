using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_ChangePartHealth : HediffComp
        {
        public HediffCompProperties_ChangePartHealth Props => (HediffCompProperties_ChangePartHealth)props;
        }

    public class HediffCompProperties_ChangePartHealth : HediffCompProperties
        {
        public int health = 0;

        public HediffCompProperties_ChangePartHealth() 
            {
            compClass = typeof(HediffComp_ChangePartHealth);
            }
        }
    }