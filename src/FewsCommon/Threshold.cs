using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FewsCommon
{
	/// <summary>
	/// Threshold value for given location a ParameterID
	/// </summary>
    public class ThresholdValue
    {
		/// <summary>
		/// Location ID
		/// </summary>
        public string LocationId { get; set; }
		/// <summary>
		/// ParameterID (ItemInfo.Quantity.Item + ItemInfo.Quantity.Unit in MZ))
		/// </summary>
        public string ParameterId { get; set; }
		/// <summary>
		/// Threshold Id (Name)
		/// </summary>
        public string ThresholdId { get; set; }
        public double Value { get; set; }
    }
}
