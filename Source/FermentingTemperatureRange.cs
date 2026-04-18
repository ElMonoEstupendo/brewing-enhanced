using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrewingEnhanced
{
    public class FermentingTemperatureRange
    {
		public string key;
		public int MinTemp;
		public int MaxTemp;

		public static FermentingTemperatureRange OffRange = new FermentingTemperatureRange(){ key="Off", MinTemp = -274, MaxTemp = 1000 };
    }
}
