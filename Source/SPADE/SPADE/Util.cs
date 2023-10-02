using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    [StaticConstructorOnStartup]
    public static class Util
        {
        static Util() 
            {
            }

        public static bool HasModExtension<T>(this Pawn pawn) where T : DefModExtension
            {
            return pawn.def.HasModExtension<T>() || pawn.kindDef.HasModExtension<T>();
            }
        public static T GetModExtension<T>(this Pawn pawn) where T : DefModExtension
            {
            T ext = null;
            if (pawn.HasModExtension<T>())
                {
                ext = pawn.kindDef.GetModExtension<T>() ?? pawn.def.GetModExtension<T>();
                }
            return ext;
            }
        }
    }
