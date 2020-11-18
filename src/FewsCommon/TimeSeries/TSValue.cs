using System;
using System.Collections.Generic;
using System.Text;

namespace FewsCommon.TimeSeriesNS
{
	/// <summary>
	/// Memory data structure of one Time step
	/// </summary>
    public class TSValue
    {
		/// <summary>
		/// TS value
		/// </summary>
        public double Value { get; set; }
		/// <summary>
		/// FEWS flag. See FEWS documentation
		/// </summary>
        public int? Flag { get; private set; }

		/// <summary>
		/// TS value + flag
		/// </summary>
		/// <param name="value"></param>
		/// <param name="flag"></param>
        public TSValue(double value, int? flag = null)
        {
            Value = value;
            Flag = flag;
        }
    }
}
