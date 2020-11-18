using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FewsCommon.TimeSeriesNS
{
	/// <summary>
	/// Time series data structure
	/// </summary>
    public class TimeSeries
    {
		/// <summary>
		/// FEWS TS type. Default instantaneous.
		/// </summary>
        public string Type { get; set; }
		/// <summary>
		/// TS location id
		/// </summary>
        public string LocationId { get; set; }
		/// <summary>
		/// FEWS Parameter ID
		/// </summary>
        public string ParameterId { get; set; }
		/// <summary>
		/// Time step for equidistant TS
		/// </summary>
        public TimeSpan? TimeStep { get; set; }
		/// <summary>
		/// Start date
		/// </summary>
        public DateTime? StartDate { get; set; }
		/// <summary>
		/// End date/time (last time step)
		/// </summary>
        public DateTime? EndDate { get; set; }
		/// <summary>
		/// TS missing value
		/// </summary>
        public double? MissVal { get; set; }
		/// <summary>
		/// Station ID. Created during RES1D import.
		/// </summary>
        public string StationName { get; set; }
		/// <summary>
		/// Latitude point coordinate
		/// </summary>
        public double? Lat { get; set; }
		/// <summary>
		/// Longitudinal point coordinate
		/// </summary>
        public double? Lon { get; set; }
		/// <summary>
		/// X coordinate
		/// </summary>
        public double? X { get; set; }
		/// <summary>
		/// Y coordinate
		/// </summary>
        public double? Y { get; set; }
		/// <summary>
		/// Z coordinate
		/// </summary>
        public double? Z { get; set; }
		/// <summary>
		/// TS units
		/// </summary>
        public string Units { get; set; }
		/// <summary>
		/// Time of forecast (time0 in FEWS)
		/// </summary>
        public DateTime? ForecastDate { get; set; }
		/// <summary>
		/// Ensemble Id identifying TS
		/// </summary>
        public string EnsembleId { get; set; }
		/// <summary>
		/// Ensemble index of  TS
		/// </summary>
        public int EnsembleMemberIndex { get; set; }
		/// <summary>
		/// Ensemble Member Id identifying TS
		/// </summary>
        public string EnsembleMemberId { get; set; }
		/// <summary>
		/// Info if TS was created as merge of more TS from PI file
		/// </summary>
        public bool HasBeenMerged { get; set; }

		/// <summary>
		/// Dictionary of time step and TS value
		/// </summary>
        public Dictionary<DateTime, TSValue> Values { get; set; }


        public TimeSeries()
        {
            Values = new Dictionary<DateTime, TSValue>();

        }

		/// <summary>
		/// Get value for given time
		/// </summary>
		/// <param name="time">Time to obtain value for</param>
		/// <param name="extrapolate">Indicate if extrapolate outside start, end time
		/// interval</param>
        public double GetValue(DateTime time, bool extrapolate = false)
        {
            if (Values.ContainsKey(time))
            {
                if (!extrapolate || (Values[time].Value != MissVal))
                {
                    return Values[time].Value;
                }
            }
            
                SortedList times = new SortedList(Values);
               
                if (times.Count == 0)
                {
                    return MissVal.Value;
                }

            
            var time_fromList = times.GetKey(0) as DateTime?;
            var value = times.GetByIndex(0) as TSValue;

            var firstNonEmpty = time_fromList.Value;
            var lastNonEmpty = time_fromList.Value;
            int j = 0;
            while ((j < times.Count - 1) && ((times.GetByIndex(j) as TSValue).Value == MissVal.Value))
            {
                j++;
                firstNonEmpty = (times.GetKey(j) as DateTime?).Value;
            }

            if (j == times.Count - 1)
            {
                return MissVal.Value;
            }

            j = times.Count - 1;
            while ((j > 1) && ((times.GetByIndex(j) as TSValue).Value == MissVal.Value))
            {
                j--;
                lastNonEmpty = (times.GetKey(j) as DateTime?).Value;
            }

            if (time <= time_fromList.Value)
                {
                if (extrapolate)
                {
                    return (times[lastNonEmpty] as TSValue).Value;
                }
                else
                {
                    value = times.GetByIndex(0) as TSValue;
                    return value.Value;
                }
            }

            time_fromList = times.GetKey(times.Count - 1) as DateTime?;

            if (time >= time_fromList.Value)
            {
                if (extrapolate)
                {
                    return (times[firstNonEmpty] as TSValue).Value;
                }
                else
                {
                    return value.Value;
                }
            }

            return 0;
        }

		/// <summary>
		/// Create copy of TS
		/// </summary>
        public TimeSeries CreateCopy()
        {
            var newTs = new TimeSeries();
            newTs.Type = Type;
            newTs.LocationId = LocationId;
            newTs.ParameterId = ParameterId;
            newTs.TimeStep = TimeStep;
            newTs.StartDate = StartDate;
            newTs.EndDate = EndDate;
            newTs.MissVal = MissVal;
            newTs.StationName = StationName;
            newTs.Lat = Lat;
            newTs.Lon = Lon;
            newTs.X = X;
            newTs.Y = Y;
            newTs.Z = Z;
            newTs.Units = Units;
            newTs.ForecastDate = ForecastDate;
            newTs.HasBeenMerged = HasBeenMerged;

            foreach (var value in Values)
            {
                newTs.Values.Add(value.Key, new TSValue(value.Value.Value, value.Value.Flag));
            }

            return newTs;
        }
    }
}
