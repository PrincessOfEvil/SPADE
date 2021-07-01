using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_EnableWorkTypes : HediffComp
        {
        public HediffCompProperties_EnableWorkTypes Props => (HediffCompProperties_EnableWorkTypes)props;
        }

    public class HediffCompProperties_EnableWorkTypes : HediffCompProperties
        {
        public List<WorkTypeDef> workTypes = new List<WorkTypeDef>();
        public WorkTags workTags = WorkTags.None;

        public HediffCompProperties_EnableWorkTypes() 
            {
            compClass = typeof(HediffComp_EnableWorkTypes);
            }
        }
    }