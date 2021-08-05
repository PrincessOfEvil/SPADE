using RimWorld;
using Verse;

namespace SPADE
	{
	public class IngredientValueGetter_NutritionMixed : IngredientValueGetter_Nutrition
		{
		public override float ValuePerUnitOf(ThingDef t)
			{
			if (!t.IsNutritionGivingIngestible)
				{
				return 1f;
				}
			return t.GetStatValueAbstract(StatDefOf.Nutrition);
			}
		}
	}
