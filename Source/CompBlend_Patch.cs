using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BrewingEnhanced
{
    static class CompBlend_Patch
    {
		[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
		public class AddBlendItems
		{
			[HarmonyPostfix]
			public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, RecipeDef recipeDef, List<Thing> ingredients)
			{
				foreach(Thing thing in values)
				{
					CompBlend blend = thing.TryGetComp<CompBlend>();
					if(blend != null)
					{
						foreach(Thing ingredient in ingredients)
						{
							CompBlend ingredient_blend = ingredient.TryGetComp<CompBlend>();
							if(ingredient_blend != null)
							{
								foreach(var item in ingredient_blend.BlendIngredients)
								{
									blend.AddIngredient(item.Key, item.Value);
								}
							}
							else
							{
								blend.AddIngredient(ingredient.def, ingredient.stackCount);
							}
						}
					}
					yield return thing;
				}
				yield break;
			}
		}
    }
}
