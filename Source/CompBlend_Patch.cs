using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BrewingEnhanced
{
    static class CompBlend_Patch
    {
		[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
		public class AddRecipeBlendItems
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
								blend.Add(ingredient_blend);
							} else
							{
								blend.Add(ingredient.def, ingredient.stackCount);
							}
						}
					}
					yield return thing;
				}
				yield break;
			}
		}

		[HarmonyPatch(typeof(Building_FermentingBarrel), nameof(Building_FermentingBarrel.AddWort), new Type[] { typeof(Thing) })]
		public class AddWortBlendToBarrel
		{
			[HarmonyPrefix]
			public static void Prefix(Thing wort, Building_FermentingBarrel __instance)
			{
				CompBlend blend = __instance.TryGetComp<CompBlend>();
				CompBlend wortBlend = wort.TryGetComp<CompBlend>();
				if( blend != null && wortBlend != null )
				{
					int numAdded = Mathf.Min(wort.stackCount, __instance.SpaceLeftForWort);
					if( numAdded > 0 )
					{
						blend.Add(wortBlend);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Building_FermentingBarrel), "Reset")]
		public class ResetBlendItems
		{
			[HarmonyPostfix]
			public static void Postfix(Building_FermentingBarrel __instance)
			{
				CompBlend blend = __instance.TryGetComp<CompBlend>();
				if( blend != null )
				{
					blend.Reset();
				}
			}
		}

		[HarmonyPatch(typeof(Building_FermentingBarrel), nameof(Building_FermentingBarrel.TakeOutBeer))]
		public class AddBlendToBeer
		{
			[HarmonyPrefix]
			public static void Prefix(Building_FermentingBarrel __instance)
			{
				CompBlend barrelBlend = __instance.TryGetComp<CompBlend>();
				if(  barrelBlend != null )
				{
					barrelBlend.DelayReset = true;
				}
			}

			[HarmonyPostfix]
			public static Thing Postfix(Thing beer, Building_FermentingBarrel __instance)
			{
				CompBlend beerBlend = beer.TryGetComp<CompBlend>();
				CompBlend barrelBlend = __instance.TryGetComp<CompBlend>();
				if( null != barrelBlend )
				{
					if( null != beerBlend ) { beerBlend.Add(barrelBlend); }
					barrelBlend.DelayReset = false;
					if( null != beer ){ barrelBlend.Reset(); } // Only actually reset if beer was made - original method can "fail".
				}
				
				return beer;
			}
		}
    }
}
