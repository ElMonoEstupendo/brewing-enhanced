using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrewingEnhanced
{
    public class MinMaxInt
    {
		public int min;
		public int max;
		bool inclusive = true;

		public bool IsInRange(int val)
		{
			if( inclusive )
			{
				return (val >= min) && (val <= max);
			}
			return (val > min) && (val < max);
		}
    }
}
