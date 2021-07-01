using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class CompTargetable_SingleCorpse_SPADE_Resurrectable : CompTargetable_SingleCorpse
		{
		protected override TargetingParameters GetTargetingParameters()
			{
			return new TargetingParameters
				{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo x) => x.Thing is Corpse && BaseTargetValidator(x.Thing) && localValidator(x.Thing))
				};
			}
		protected bool localValidator(Thing thing)
			{
			Corpse corpse = thing as Corpse;
			if (corpse != null)
				{
				return corpse.InnerPawn.HasModExtension<DefExtension_SPADE_Resurrectable>();
				}
			return false;
			}
        }
	public class DefExtension_SPADE_Resurrectable : DefModExtension
		{
		public DefExtension_SPADE_Resurrectable() { }
		}
	}
