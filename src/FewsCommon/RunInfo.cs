using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FewsCommon
{
    /// <summary>
    /// Get T0 (TOF) from Runtime.Info as GMT
    /// </summary>
    public class RunInfo
    {
        public static DateTime GetT0InLocalTime(string runInfoFilePath)
        {
            if (!File.Exists(runInfoFilePath))
                throw new Exception($"Run info file '{runInfoFilePath}' does not exist.");

            try
            {
                var xmlDoc = new XmlDocument();
                using (var xtr = new XmlTextReader(runInfoFilePath) { Namespaces = false })
                {

                    xmlDoc.Load(xtr);
                    string timeZone = xmlDoc.SelectSingleNode("/Run/timeZone").InnerText;
                    double timeZoneD = double.Parse(timeZone, CultureInfo.InvariantCulture);
                    string date = xmlDoc.SelectSingleNode("/Run/time0").Attributes["date"].InnerText;
                    string time = xmlDoc.SelectSingleNode("/Run/time0").Attributes["time"].InnerText;

                    return DateTime.ParseExact(date + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                        .AddHours(-timeZoneD)
                        .Add(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now));
                }
            }
            catch (Exception)
            {
                throw new Exception("Unable to parse time0 from RunInfo file.");
            }


        }
    }
}
