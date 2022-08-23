using DHI.Generic.MikeZero;
using DHI.Generic.MikeZero.DFS;
using FewsCommon;
using FewsCommon.TimeSeries;
using FewsCommon.TimeSeriesNS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfs0Handeling
{
    /// <summary>
    /// Write Time series data from FEWS PI memory data structure to DFS0
    /// </summary>
    public class Dfs0Writer
    {
        /// 
        /// <param name="pi">List of FEWS TimeSeries</param>
        /// <param name="rootPath">Root path where dfs0 files will be placed to. </param>
        /// <param name="startTime">Start time to be written to dfs0</param>
        /// <param name="endTime">End time to be written to dfs0</param>
        /// <param name="relativePath">Full file path relative to rootPath. If empty file
        /// name generated based on PI content.</param>
        /// <param name="ensembleId">Ensemble Id identifying what TS write to file.
        /// </param>
        /// <param name="ensembleMemberId">Identification of ensemble to be
        /// exported</param>
        /// <param name="prefix">Prefix added to the TS location</param>
        /// <param name="parameterType">DFS0 Parameter type. Available values instantaneous,
        /// accumulated, meanstepbackward, meanstepforward, stepaccumulated, Default -
        /// instantaneous, </param>
        public bool WriteDfs0File(PI pi, string rootPath, DateTime startTime, DateTime endTime, string relativePath = "", string ensembleId = "", string ensembleMemberId ="", string prefix = "", string parameterType = "")
        {
            var fileNames = new Dictionary<string, IList<TsIdentification>>();
            if (!string.IsNullOrEmpty(relativePath))
            {
                fileNames.Add(Path.Combine(rootPath, relativePath), new List<TsIdentification>());
            }
           
                foreach (var ts in pi.TimeSeries)
                {
                    var array = ts.LocationId.Split('|');
                    var location = ts.LocationId;
                    var itemName = ts.LocationId;
                    string fullName = Path.Combine(rootPath, relativePath);
                    if (array.Length > 1)
                    {
                        fullName = Path.Combine(rootPath, relativePath, array[0]);
                        ts.LocationId = $"{array[1]}_{array[0]}";
                        itemName = array[1];
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            fullName = Path.Combine(rootPath, relativePath);
                        }
                    }
                    var tsIdentification = new TsIdentification(ts, itemName);
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        tsIdentification.LocationId = $"{prefix}_{tsIdentification.LocationId}";
                    }
                    if (!fileNames.ContainsKey(fullName))
                    {
                        fileNames.Add(fullName, new List<TsIdentification>());
                    }
                    fileNames[fullName].Add(tsIdentification);
                }

            foreach (var file in fileNames)
            {
                _WriteOneFile(file.Key, pi, file.Value, startTime, endTime, parameterType);
            }
            return true;
        }

        /// 
        /// <param name="fileName">Output DFS0 file path</param>
        /// <param name="pi">Memory data structure containing TS</param>
        /// <param name="list">List of TS identification selecting  TS to write in file.
        /// </param>
        /// <param name="startTime">Date/time of first time step to be written to
        /// file</param>
        /// <param name="endTime">Date/time of last time step to be written to
        /// file</param>
        /// <param name="parameterType"></param>
        private void  _WriteOneFile(string fileName, PI pi, IList<TsIdentification> list, DateTime startTime, DateTime endTime, string parameterType)
        {
            var allTimeSteps = new List<DateTime>();
            foreach (var ident in list)
            {
                var tsList = pi.GetTS(ident);
                foreach (var ts in tsList)
                {
                    for (int i = 0; i < ts.Values.Count; i++)
                    {
                        foreach (var value in ts.Values.Keys)
                        {
                            if ((value >= startTime) && (value <= endTime))
                            {
                                if (!allTimeSteps.Contains(value))
                                {
                                    allTimeSteps.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            allTimeSteps.Sort();

            if (allTimeSteps.Count > 0)
            {
                var fullTsList = new List<TimeSeries>();
                DfsFactory factory = new DfsFactory();
                DfsBuilder builder = DfsBuilder.Create("FEWS Adaptor", "FEWS Adaptor - Dfs0 writer", 10000);
                builder.SetItemStatisticsType(StatType.RegularStat);
                builder.SetDataType(0);
                builder.SetGeographicalProjection(factory.CreateProjectionUndefined());
                builder.SetItemStatisticsType(StatType.RegularStat);
                IDfsTemporalAxis temporalAxis = factory.CreateTemporalNonEqCalendarAxis(DHI.Generic.MikeZero.eumUnit.eumUsec, allTimeSteps[0]);
                builder.SetTemporalAxis(temporalAxis);
                foreach (var ident in list)
                {
                    var tsList = pi.GetTS(ident);
                    foreach (var ts in tsList)
                    {
                        DfsDynamicItemBuilder item = builder.CreateDynamicItemBuilder();
                        var itemEnum = eumItem.eumIItemUndefined;
                        var unitEum = eumUnit.eumUUnitUndefined;
                        var array = ts.ParameterId.Split(';');
                        var itemStr = array[0];

                        var unitStr = string.Empty;
                        if (array.Count() > 1)
                        {
                            unitStr = array[1];
                        }
                        if (!Enum.TryParse(itemStr, true, out itemEnum))
                        {
                            itemEnum = eumItem.eumIItemUndefined;
                        }
                        if (!Enum.TryParse(unitStr, true, out unitEum))
                        {
                            unitEum = eumUnit.eumUUnitUndefined;
                        }
                        item.Set(ident.ItemName, eumQuantity.Create(itemEnum, unitEum), DfsSimpleType.Float);
                        if (string.IsNullOrEmpty(parameterType))
                        {
                            item.SetValueType(DataValueType.Instantaneous);
                        }
                        else
                        {
                            switch (parameterType.ToLower())
                            {
                                case "instantaneous":
                                    item.SetValueType(DataValueType.Instantaneous);
                                    break;
                                case "accumulated":
                                    item.SetValueType(DataValueType.Accumulated);
                                    break;
                                case "meanstepbackward":
                                    item.SetValueType(DataValueType.MeanStepBackward);
                                    break;
                                case "meanstepforward":
                                    item.SetValueType(DataValueType.MeanStepForward);
                                    break;
                                case "stepaccumulated":
                                    item.SetValueType(DataValueType.StepAccumulated);
                                    break;
                                default:
                                    item.SetValueType(DataValueType.Instantaneous);
                                    break;
                            }
                        }
                        item.SetAxis(factory.CreateAxisEqD0());
                        builder.AddDynamicItem(item.GetDynamicItemInfo());
                        fullTsList.Add(ts);
                    }
                }


                builder.CreateFile(fileName);
                using (IDfsFile file = builder.GetFile())
                {
                    if (fullTsList[0].MissVal.HasValue)
                    {
                        file.FileInfo.DeleteValueFloat = (float)fullTsList[0].MissVal.Value;
                        file.FileInfo.DeleteValueDouble = fullTsList[0].MissVal.Value;
                    }
                    else
                    {
                        file.FileInfo.DeleteValueFloat = -9999.9f;
                        file.FileInfo.DeleteValueDouble = -9999.9;
                    }
                    float[] oneStepValues = new float[1];
                    double noValue = file.FileInfo.DeleteValueFloat;
                    for (int i = 0; i < allTimeSteps.Count; i++)
                    {
                        double doubleTime = (allTimeSteps[i] - allTimeSteps[0]).TotalSeconds;
                        bool writeTimeStep = false;
                        int k = 0;
                        while (k < fullTsList.Count && !writeTimeStep)
                        {
                            double value = fullTsList[k].GetValue(allTimeSteps[i]);
                            writeTimeStep = (Math.Abs(value - noValue) > double.Epsilon);
                            k++;
                        }
                        if (writeTimeStep)
                        {
                            for (int j = 0; j < fullTsList.Count; j++)
                            {
                                double value = file.FileInfo.DeleteValueFloat;
                                value = fullTsList[j].GetValue(allTimeSteps[i]);
                                oneStepValues[0] = (float)value;
                                file.WriteItemTimeStepNext(doubleTime, oneStepValues);
                            }
                        }
                    }
                }
                Logger.AddLog(Logger.TypeEnum.Info, $"File {fileName} written successfully");
            }
        }
    }
}
