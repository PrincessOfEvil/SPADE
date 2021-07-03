using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class DefExtension_DoesNotNeed : DefModExtension
        {
        public List<NeedDef> needs;
        public DefExtension_DoesNotNeed() { }
        }

    public class DefExtension_CannotTalk : DefModExtension
        {
        public DefExtension_CannotTalk() { }
        }
    }