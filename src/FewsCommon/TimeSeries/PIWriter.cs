using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace FewsCommon.TimeSeriesNS
{
    /// <summary>
    /// Write TS memory data structure to FEWS PI file
    /// </summary>
    public class PIWriter
    {
        /// <summary>
        /// Write PI file
        /// </summary>
        /// <param name="filePath">PI file path</param>
        /// <param name="pis">TS memory data structure</param>
        public static void Write(string filePath, List<PI> pis)
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            using (var xmlWriter = XmlWriter.Create(filePath, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("TimeSeries", "http://www.wldelft.nl/fews/PI");
                xmlWriter.WriteAttributeString("xmlns", null, "http://www.wldelft.nl/fews/PI");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                xmlWriter.WriteAttributeString("xsi", "schemaLocation", null, "http://www.wldelft.nl/fews/PI http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_timeseries.xsd");
                xmlWriter.WriteAttributeString("version", "1.8");

                if (pis.Count > 0 && null != pis.FirstOrDefault(x => x.TimeSeries.Count > 0))
                {
                    xmlWriter.WriteStartElement("timeZone");
                    xmlWriter.WriteString(pis[0].TimeZone.ToString(CultureInfo.InvariantCulture));
                    xmlWriter.WriteEndElement();
                }

                foreach (var pi in pis)
                {
                    // TODO: check TimeZone of pis

                    foreach (var ts in pi.TimeSeries)
                    {
                        xmlWriter.WriteStartElement("series");
                        xmlWriter.WriteStartElement("header");

                        xmlWriter.WriteElementString("type", ts.Type);
                        xmlWriter.WriteElementString("locationId", ts.LocationId);
                        xmlWriter.WriteElementString("parameterId", ts.ParameterId);
                        if (!string.IsNullOrEmpty(ts.EnsembleId))
                        {
                            xmlWriter.WriteElementString("ensembleId", ts.EnsembleId);
                            if (!string.IsNullOrEmpty(ts.EnsembleMemberId))
                            {
                                xmlWriter.WriteElementString("ensembleMemberId", ts.EnsembleMemberId);
                            }
                            else
                            {
                                xmlWriter.WriteElementString("ensembleMemberIndex", ts.EnsembleMemberIndex.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                        if (!ts.TimeStep.HasValue)
                            throw new Exception($"TimeStep attribute is compulsory. Location '{ts.LocationId}', parameter '{ts.ParameterId}'");
                        xmlWriter.WriteStartElement("timeStep");
                        xmlWriter.WriteAttributeString("unit", "second");
                        xmlWriter.WriteAttributeString("multiplier", ts.TimeStep.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                        xmlWriter.WriteEndElement();

                        if (!ts.StartDate.HasValue)
                            throw new Exception($"StartDate attribute is compulsory. Location '{ts.LocationId}', parameter '{ts.ParameterId}'");
                        xmlWriter.WriteStartElement("startDate");
                        xmlWriter.WriteAttributeString("date", ts.StartDate.Value.ToString("yyyy-MM-dd"));
                        xmlWriter.WriteAttributeString("time", ts.StartDate.Value.ToString("HH:mm:ss"));
                        xmlWriter.WriteEndElement();

                        if (!ts.EndDate.HasValue)
                            throw new Exception($"EndDate attribute is compulsory. Location '{ts.LocationId}', parameter '{ts.ParameterId}'");
                        xmlWriter.WriteStartElement("endDate");
                        xmlWriter.WriteAttributeString("date", ts.EndDate.Value.ToString("yyyy-MM-dd"));
                        xmlWriter.WriteAttributeString("time", ts.EndDate.Value.ToString("HH:mm:ss"));
                        xmlWriter.WriteEndElement();

                        if (ts.ForecastDate.HasValue)
                        {
                            xmlWriter.WriteStartElement("forecastDate");
                            xmlWriter.WriteAttributeString("date", ts.ForecastDate.Value.ToString("yyyy-MM-dd"));
                            xmlWriter.WriteAttributeString("time", ts.ForecastDate.Value.ToString("HH:mm:ss"));
                            xmlWriter.WriteEndElement();
                        }

                        if (!ts.MissVal.HasValue)
                            throw new Exception($"MissVal attribute is compulsory. Location '{ts.LocationId}', parameter '{ts.ParameterId}'");
                        xmlWriter.WriteElementString("missVal", ts.MissVal.Value.ToString(CultureInfo.InvariantCulture));
                        if (!string.IsNullOrEmpty(ts.Units))
                        {
                            xmlWriter.WriteElementString("units", ts.Units);
                        }

                        xmlWriter.WriteEndElement(); // /header                     
                        // values
                        var sortedValues = ts.Values.OrderBy(x => x.Key);
                        foreach (var value in sortedValues)
                        {
                            xmlWriter.WriteStartElement("event");
                            xmlWriter.WriteAttributeString("date", value.Key.ToString("yyyy-MM-dd"));
                            xmlWriter.WriteAttributeString("time", value.Key.ToString("HH:mm:ss"));
                            xmlWriter.WriteAttributeString("value", value.Value.Value.ToString(CultureInfo.InvariantCulture));
                            if (value.Value.Flag.HasValue)
                            {
                                xmlWriter.WriteAttributeString("flag", value.Value.Flag.Value.ToString(CultureInfo.InvariantCulture));
                            }
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement(); // /series
                    }
                }

                xmlWriter.WriteEndElement(); // /TimeSeries
                xmlWriter.Close();
            }
        }
    }
}
