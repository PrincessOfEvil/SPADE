using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public static class Util
        {
        public static bool HasModExtension<T>(this Pawn pawn) where T : DefModExtension
            {
            return pawn.def.HasModExtension<T>() || pawn.kindDef.HasModExtension<T>();
            }
        public static T GetModExtension<T>(this Pawn pawn) where T : DefModExtension
            {
            T ext = null;
            if (pawn.HasModExtension<T>())
                {
                ext = pawn.kindDef.GetModExtension<T>();
                if (ext == null)
                    {
                    ext = pawn.def.GetModExtension<T>();
                    }
                }
            return ext;
            }

        }
    }
