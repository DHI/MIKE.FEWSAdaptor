using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHI.Generic.MikeZero;
using DHI.Generic.MikeZero.DFS;
using FewsCommon.TimeSeriesNS;

namespace Dfs0Handeling
{
	/// <summary>
	/// Read data from dfs0 file to FEWS PI memory data structure
	/// </summary>
    public class ReadDfs0
    {
		/// <summary>
		/// Read Time series data DFS0 to FEWS PI memory data structure
		/// </summary>
		/// <param name="pi">Memory data structure where content of the DFS0 file will be
		/// add</param>
		/// <param name="rootPath">Root directory where dfs0 files are placed</param>
		/// <param name="relativePath">Full file path relative to rootPath</param>
		/// <param name="ensembleId">Ensemble Id identifying where to put data from
		/// file</param>
		/// <param name="ensembleMemberId">Ensemble member Id identifying where to put data
		/// from file</param>
		/// <param name="ensembleMemberIndex">Ensemble member index</param>
        public bool ReadDfs0File(ref PI pi, string rootPath, string relativePath, string ensembleId, string ensembleMemberId, int ensembleMemberIndex)
        {
            var dfs0File = DfsFileFactory.DfsGenericOpen(Path.Combine(rootPath, relativePath));
            IDfsFileInfo fileInfo = dfs0File.FileInfo;
            int numberOfTimeSteps = fileInfo.TimeAxis.NumberOfTimeSteps;
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            if (fileInfo.TimeAxis.IsCalendar())
                    {
                if (dfs0File.FileInfo.TimeAxis.IsEquidistant())
                {
                    start = (dfs0File.FileInfo.TimeAxis as IDfsEqCalendarAxis).StartDateTime;
                }
                else
                {
                    start = (dfs0File.FileInfo.TimeAxis as IDfsNonEqCalendarAxis).StartDateTime;
                }
            }
            for (int itemIndex = 0; itemIndex < dfs0File.ItemInfo.Count; itemIndex++)
            {
                var ts = new TimeSeries();
                switch (dfs0File.ItemInfo[itemIndex].ValueType = DataValueType.Instantaneous)
                {
                    case DataValueType.Instantaneous:
                        ts.Type = "instantaneous";
                        break;
                    default:
                        ts.Type = "instantaneous";
                        break;
                }                
                ts.X = dfs0File.ItemInfo[itemIndex].ReferenceCoordinateX;
                ts.Y = dfs0File.ItemInfo[itemIndex].ReferenceCoordinateY;
                ts.Z = dfs0File.ItemInfo[itemIndex].ReferenceCoordinateZ;
                ts.LocationId = relativePath;
                if (!string.IsNullOrEmpty(ensembleId))
                {
                    ts.EnsembleId = ensembleId;
                    ts.EnsembleMemberId = ensembleMemberId;
                    ts.EnsembleMemberIndex = ensembleMemberIndex;
                }
                if (dfs0File.ItemInfo.Count > 1)
                {
                    ts.LocationId = $"{ts.LocationId}|{dfs0File.ItemInfo[itemIndex].Name}";
                }
                ts.TimeStep = new TimeSpan(1, 0, 0);
                ts.StationName = ts.LocationId;
                ts.ParameterId = dfs0File.ItemInfo[itemIndex].Quantity.Item.ToString()+";" + dfs0File.ItemInfo[itemIndex].Quantity.Unit.ToString();
                ts.MissVal = -999999.9;
                ts.StartDate = start;
                ts.EndDate = start;
                ts.Units = "";
                var deleteVal = dfs0File.FileInfo.DeleteValueFloat;
                DateTime step1 = DateTime.MinValue;
                DateTime step2 = DateTime.MinValue;
                for (int timeStepIndex = 0; timeStepIndex < fileInfo.TimeAxis.NumberOfTimeSteps; timeStepIndex++)
                {
                    double value = ts.MissVal.Value;                    
                    
                    var values = dfs0File.ReadItemTimeStep(itemIndex + 1, timeStepIndex);
                    float fvalue = (float)(values.Data.GetValue(0));
                    if (Math.Abs(fvalue - deleteVal) > float.Epsilon)
                    {
                        value = fvalue;
                    }
                    var time = values.TimeAsDateTime(dfs0File.FileInfo.TimeAxis);

                    ts.Values.Add(time, new TSValue(value));
                    ts.EndDate = time;
                    if (step1 == DateTime.MinValue)
                    {
                        step1 = time;
                    }
                    else if (step2 == DateTime.MinValue)
                    {
                        ts.TimeStep = time - step1;
                        step2 = time;
                    }

                }
                pi.TimeSeries.Add(ts);
            }
            return true;
        }
    }
}
