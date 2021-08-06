using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_IgnoresTerrain : HediffComp
		{
		public override string CompTipStringExtra
			{
			get
				{
				return "IgnoresTerrain".Translate();
				}
			}
		}
    }