using FewsCommon.TimeSeries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace FewsCommon.TimeSeriesNS
{
    /// <summary>
    /// Memory data structure containing list of time series. Structure follows the
    /// FEWS PI file structure.
    /// </summary>
    public class PI
    {
        /// <summary>
        /// Shift to GMT
        /// </summary>
        public double TimeZone { get; set; }
        /// <summary>
        /// List of Time series
        /// </summary>
        public List<TimeSeries> TimeSeries { get; private set; } 

        /// <summary>
        /// Thresholds used in TS
        /// </summary>
        public Thresholds ThresholdsValues { get; }

        public PI()
        {
            TimeSeries = new List<TimeSeries>();
            ThresholdsValues = new Thresholds();
        }

        /// <summary>
        /// Get list of TS based on TsIdentification
        /// </summary>
        /// <param name="identification">TsIdentification used for selection</param>
        public IList<TimeSeries> GetTS(TsIdentification identification)
        {
            return GetTS(identification.LocationId, identification.ParmId, identification.TsType, identification.EnsembleId, identification.EnsembleMemberId, -1);
        }

        /// <summary>
        /// Get TS based on properties. TS are be queried by location eventually parameter,
        /// type and ensemble identification
        /// </summary>
        /// <param name="locationId">Location Id</param>
        /// <param name="parmId">ParameterId</param>
        /// <param name="type">TS type (instantaneous, ...)</param>
        /// <param name="ensembleId">EnsembleId</param>
        /// <param name="ensembleMemberId">Ensemble MemberId</param>
        /// <param name="ensembleMemberIndex">Ensemble Member Index</param>
        public IList<TimeSeries> GetTS(string locationId, string parmId = "", string type = "", string ensembleId = "", string ensembleMemberId = "", int ensembleMemberIndex = -1)
        {
            var res = new List<TimeSeries>();
            foreach (var ts in TimeSeries)
            {
                if (string.Compare(ts.LocationId, locationId, true) == 0)
                {
                    if (string.IsNullOrEmpty(parmId) || (string.Compare(parmId, ts.ParameterId, true) == 0))
                    {
                        if (string.IsNullOrEmpty(type) || (string.Compare(type, ts.Type, true) == 0))
                        {
                            if (string.IsNullOrEmpty(ensembleId) || (string.Compare(ensembleId, ts.EnsembleId, true) == 0))
                            {
                                if (string.IsNullOrEmpty(ensembleMemberId) || (string.Compare(ensembleMemberId, ts.EnsembleMemberId, true) == 0))
                                {
                                    res.Add(ts);
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get all Distinct date / time values.
        /// </summary>
            public List<DateTime> GetDistinctDts()
        {
            var result = new List<DateTime>();
            foreach (var ts in TimeSeries)
            {
                for (int i = 0; i < ts.Values.Count; i++)
                {
                    var dt = ts.StartDate.Value.AddSeconds(i * ts.TimeStep.Value.TotalSeconds);
                    if (!result.Contains(dt))
                    {
                        result.Add(dt);
                    }
                }
            }

            result.Sort();
            return result;
        }

        /// <summary>
        /// Get all Distinct date / time values from TS selected by list of IdParameters
        /// </summary>
        /// <param name="items"></param>
        public List<DateTime> GetDistinctDts(IList<string> items)
        {
            var result = new List<DateTime>();
            foreach (var ts in TimeSeries)
            {
                if (items.Contains(ts.ParameterId))
                {
                    for (int i = 0; i < ts.Values.Count; i++)
                    {
                        var dt = ts.StartDate.Value.AddSeconds(i * ts.TimeStep.Value.TotalSeconds);
                        if (!result.Contains(dt))
                        {
                            result.Add(dt);
                        }
                    }
                }
            }
            result.Sort();
            return result;
        }

    }
}
