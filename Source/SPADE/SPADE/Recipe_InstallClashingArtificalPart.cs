using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
	public class Recipe_InstallClashingArtificalPart : Recipe_InstallArtificialBodyPart
		{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
			{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
				{
					IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == record);
					if (source.Count() == 1 && source.First().def == recipe.addsHediff)
						{
						return false;
						}
					if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
						{
						return false;
						}
					if  (pawn.health.hediffSet.hediffs.Any((Hediff x) => !recipe.CompatibleWithHediff(x.def)))
						{
						return false;
						}
				return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
				});
			}
		}
    }
