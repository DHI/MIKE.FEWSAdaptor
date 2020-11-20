using FewsCommon.TimeSeriesNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FewsCommon
{
    /// <summary>
    /// Can be used for the cutting out manually edited values on the end of TS
    /// </summary>
    public class InputManipulator
    {
        public InputManipulator()
        {

        }

        /// <summary>
        /// Truncate manually added  values on the end of given TS
        /// </summary>
        /// <param name="pi"></param>
        public void TruncateTsManualEnd(PI pi)
        {
            for (int i = 0; i < pi.TimeSeries.Count; i++)
            {
                var ts = pi.TimeSeries[i];
                var oldPiars = ts.Values.OrderBy(x => x.Key).ToList();
                oldPiars.Reverse();

                ts.Values = new Dictionary<DateTime, TSValue>();
                bool found = false;
                foreach(var pair in oldPiars)
                {
                    if (pair.Value.Flag >= 8 && pair.Value.Flag <= 9)
                    {
                        if (found) // 8-9 in the middle of ts
                        {
                            ts.Values.Add(pair.Key, new TSValue(ts.MissVal.Value, 9));
                            Logger.AddLog(Logger.TypeEnum.Info, $"Value for {pair.Key.ToString("yyyy-MM-dd HH:mm:ss")} to be removed for statnion id {ts.LocationId} with name {ts.StationName}.");
                        }

                        continue;
                    }

                    if (pair.Value.Flag >= 2 && pair.Value.Flag <= 7)
                    {
                        found = true;
                        ts.Values.Add(pair.Key, new TSValue(ts.MissVal.Value, 9));
                        Logger.AddLog(Logger.TypeEnum.Info, $"Value for {pair.Key.ToString("yyyy-MM-dd HH:mm:ss")} to be removed for statnion id {ts.LocationId} with name {ts.StationName}.");
                    }

                    if (pair.Value.Flag == 0 || pair.Value.Flag == 1) // original or rewritten
                        break;
                }

                if (ts.Values.Count == 0)
                {
                    pi.TimeSeries.RemoveAt(i);
                    i--;
                }
                else
                {
                    ts.StartDate = ts.Values.Keys.Min();
                    ts.EndDate = ts.Values.Keys.Max();
                }
            }
        }
    }
}
