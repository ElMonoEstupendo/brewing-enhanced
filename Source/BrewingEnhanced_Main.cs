using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace BrewingEnhanced
{
	[StaticConstructorOnStartup]
    public class BrewingEnhanced_Main
    {
		static BrewingEnhanced_Main()
		{
			Log.Message("Enhancing the brewing, Jim...");
			Harmony harmony = new Harmony("ElMonoEstupendo.brewingenhanced");
			harmony.PatchAll();
		}
    }
}
