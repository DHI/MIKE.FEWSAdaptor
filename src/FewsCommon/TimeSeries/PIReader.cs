using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace FewsCommon.TimeSeriesNS
{
    /// <summary>
    /// Reader FEWS Time series format file to memory data structure
    /// </summary>
    public class PIReader
    {
        /// <summary>
        /// Pars FEWS PI file to memory time series data structure
        /// </summary>
        /// <param name="path">PI file path</param>
        public static PI Read(string path)
        {
            var pi = new PI();

            bool inHeaderSection = false;
            TimeSeries currTS = null;
            var currentPath = new List<string>();

            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        currentPath.Add(reader.Name);
                        if (!inHeaderSection)
                        {
                            switch (reader.Name)
                            {
                                case "TimeSeries":
                                    break;

                                case "timeZone":
                                    reader.Read();
                                    pi.TimeZone = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;

                                case "series":
                                    currTS = new TimeSeries();
                                    break;

                                case "header":
                                    inHeaderSection = true;
                                    break;
                                case "event":
                                    _parseTsValue(reader, currTS, currentPath, pi);
                                    break;

                                default:
                                    throw new Exception($"Unknown element {_getCurrElemPath(currentPath)}.");
                            }

                            if (reader.IsEmptyElement)
                            {
                                currentPath.RemoveAt(currentPath.Count - 1);
                            }
                        }
                        else
                        {
                            // TS header section

                            switch (reader.Name)
                            {
                                case "type":
                                    reader.Read();
                                    currTS.Type = reader.Value.Trim();
                                    break;

                                case "locationId":
                                    reader.Read();
                                    currTS.LocationId = reader.Value.Trim();
                                    break;

                                case "parameterId":
                                    reader.Read();
                                    currTS.ParameterId = reader.Value.Trim();
                                    break;

                                case "timeStep":
                                    currTS.TimeStep = _parseTimeStep(reader, currentPath);
                                    break;

                                case "startDate":
                                    currTS.StartDate = _parseDate(reader, currentPath);
                                    break;

                                case "endDate":
                                    currTS.EndDate = _parseDate(reader, currentPath);
                                    break;

                                case "missVal":
                                    reader.Read();
                                    currTS.MissVal = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;

                                case "stationName":
                                    reader.Read();
                                    currTS.StationName = reader.Value.Trim();
                                    break;

                                case "lat":
                                    reader.Read();
                                    currTS.Lat = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;

                                case "lon":
                                    reader.Read();
                                    currTS.Lon = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;

                                case "x":
                                    reader.Read();
                                    currTS.X = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;
                                case "y":
                                    reader.Read();
                                    currTS.Y = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;
                                case "z":
                                    reader.Read();
                                    currTS.Z = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;

                                case "units":
                                    reader.Read();
                                    currTS.Units = reader.Value.Trim();
                                    break;

                                case "forecastDate":
                                    currTS.ForecastDate = _parseDate(reader, currentPath);
                                    break;
                                case "ensembleId":
                                    reader.Read();
                                    currTS.EnsembleId = reader.Value.Trim();
                                    break;
                                case "ensembleMemberId":
                                    reader.Read();
                                    currTS.EnsembleMemberId = reader.Value.Trim();
                                    break;
                                case "ensembleMemberIndex":
                                    reader.Read();
                                    currTS.EnsembleMemberIndex = int.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                    break;
                                case "thresholds":
                                    break;
                                case "creationDate":
                                    break;
                                case "creationTime":
                                    break;
                                case "highLevelThreshold":
                                    var threshoald = _ParseThresholdValue(reader, currentPath, currTS.LocationId ,currTS.ParameterId);
                                    pi.ThresholdsValues.AddThreshold(threshoald);
                                    break;
                                default:
                                    throw new Exception($"Unknown header element {_getCurrElemPath(currentPath)}.");

                            }

                            if (reader.IsEmptyElement)
                            {
                                currentPath.RemoveAt(currentPath.Count - 1);
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        currentPath.RemoveAt(currentPath.Count - 1);

                        switch (reader.Name)
                        {
                            case "series":
                                pi.TimeSeries.Add(currTS);
                                currTS = null;
                                break;

                            case "header":
                                inHeaderSection = false;
                                break;
                        }
                    }
                }
            }

            return pi;
        }

        // -----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string _getCurrElemPath(List<string> currentPath)
        {
            return string.Join("/", currentPath);
        }

        // -----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string _getRequiredAttribute(XmlReader reader, string name, List<string> currentPath)
        {
            string attr = reader[name];
            if (attr == null)
                throw new Exception($"Attribute '{name}' of element {_getCurrElemPath(currentPath)} not found.");

            return attr;
        }

        // -----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TimeSpan _parseTimeStep(XmlReader reader, List<string> currentPath)
        {
            string unit = _getRequiredAttribute(reader, "unit", currentPath);
            int multiplier = int.Parse(_getRequiredAttribute(reader, "multiplier", currentPath), CultureInfo.InvariantCulture);

            switch (unit)
            {
                case "second":
                    return new TimeSpan(0, 0, multiplier);
                case "minute":
                    return new TimeSpan(0, multiplier, 0);
                case "hour":
                    return new TimeSpan(multiplier, 0, 0);
                case "week":
                    return new TimeSpan(7 * multiplier, 0, 0, 0);
                default:
                    throw new Exception($"Unknown unit attribute value '{unit}' of element {_getCurrElemPath(currentPath)}.");
            }
        }

        // -----------------------------------------------------------------------------------------------------------------------------------------------------

        private static FewsCommon.ThresholdValue _ParseThresholdValue(XmlReader reader, List<string> currentPath, string currentLocationId, string currentParameterId)
        {
            var result = new ThresholdValue();
            result.LocationId = currentLocationId;
            result.ParameterId = currentParameterId;
            result.ThresholdId = _getRequiredAttribute(reader, "id", currentPath);
            string thresholdStrValue = _getRequiredAttribute(reader, "value", currentPath);
            double value = double.NaN;
            double.TryParse(thresholdStrValue, out value);
            result.Value = value;
            return result;
        }

        private static DateTime _parseDate(XmlReader reader, List<string> currentPath)
        {
            string date = _getRequiredAttribute(reader, "date", currentPath);
            string time = _getRequiredAttribute(reader, "time", currentPath);

            return DateTime.ParseExact(date + time, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture);
        }

        // -----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void _parseTsValue(XmlReader reader, TimeSeries currTS, List<string> currentPath, PI pi)
        {
            DateTime dt = _parseDate(reader, currentPath);
            double value = double.Parse(_getRequiredAttribute(reader, "value", currentPath), CultureInfo.InvariantCulture);
            int flag = 0;
            string attr = reader["flag"];
            if (attr != null)
            {
                int.TryParse(attr, out flag);
            }
            ////int.Parse(_getRequiredAttribute(reader, "flag", currentPath), CultureInfo.InvariantCulture);

            currTS.Values.Add(dt, new TSValue(value, flag));
        }
    }
}
