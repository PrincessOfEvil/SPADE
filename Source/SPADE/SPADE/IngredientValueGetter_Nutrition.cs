using RimWorld;
using Verse;

namespace SPADE
	{
	public class IngredientValueGetter_NutritionMixed : IngredientValueGetter_Nutrition
		{
		public override float ValuePerUnitOf(ThingDef t)
			{
			return !t.IsNutritionGivingIngestible ? 1f : t.GetStatValueAbstract(StatDefOf.Nutrition);
			}
		}
	}
