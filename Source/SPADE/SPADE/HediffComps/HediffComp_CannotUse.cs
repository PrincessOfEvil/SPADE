using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_CannotUseMelee : HediffComp
		{
		public override string CompTipStringExtra
			{
			get
				{
				return "CannotUseMelee".Translate();
				}
			}
		}
    public class HediffComp_CannotUseRanged : HediffComp
		{
		public override string CompTipStringExtra
			{
			get
				{
				return "CannotUseRanged".Translate();
				}
			}
		}
    }