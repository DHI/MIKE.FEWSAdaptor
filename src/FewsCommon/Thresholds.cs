using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FewsCommon
{
    /// <summary>
    /// All thresholds used in the system
    /// </summary>
    public class Thresholds
    {
        internal IDictionary<string, ThresholdValue> _thresholds = new Dictionary<string, ThresholdValue>();

        /// <summary>
        /// Get all thresholds as Dictionary
        /// </summary>
        public IDictionary<string, ThresholdValue> GetAll()
        {
            return _thresholds;
        }
        internal string _CreateId(string locationId, string parameterId, string thresholdId)
        {
            string result = $"<{locationId}><{parameterId}><{thresholdId}>";
            return result;
        }
        /// <summary>
        /// Add one threshold to Dictionary. ComposedId is created as composition of
        /// Location, Parametr Id and  individual ThresholdId
        /// </summary>
        /// <param name="hresholdValue"></param>
        public bool AddThreshold(ThresholdValue hresholdValue)
        {
            string id = _CreateId(hresholdValue.LocationId, hresholdValue.ParameterId, hresholdValue.ThresholdId);
            if (_thresholds.ContainsKey(id))
            {
                return false;
            }
            else
            {
                _thresholds.Add(id, hresholdValue);
                return true;
            }            
        }
        /// <summary>
        /// Get all unique thresholdComposed Ids
        /// </summary>
        public IList<string> GethresholdIds()
        {
            var result = new List<string>();
            foreach (var threshold in _thresholds)
            {
                if (!result.Contains(threshold.Value.ThresholdId))
                {
                    result.Add(threshold.Value.ThresholdId);
                }
            }
            return result;
        }

        /// <summary>
        /// Get value based on Composed ThresholdId.
        /// </summary>
        /// <param name="id"></param>
        public double GetThresholdValue(string id)
        {
            if (_thresholds.ContainsKey(id))
            {
                return _thresholds[id].Value;
            }
            else
            {
                return double.MaxValue;
            }
        }
        /// <summary>
        /// Get threshold value based on LocationId, parameterId and tresholdId
        /// </summary>
        /// <param name="locationId">Location ID</param>
        /// <param name="parameterId">ParameterId (ItemInfo.Quantity.Item + ItemInfo.
        /// Quantity.Unit in MZ)</param>
        /// <param name="thresholdId">TresholdId (name)</param>
        public double GetThresholdValue(string locationId, string parameterId, string thresholdId)
        {
            var id = _CreateId(locationId, parameterId, thresholdId);
            if (_thresholds.ContainsKey(id))
            {
                return _thresholds[id].Value;
            }
            else
            {
                return double.MaxValue;
            }
        }
    }
}
