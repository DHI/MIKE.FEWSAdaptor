using FewsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ModifyMhydroFile
{
    class ModifyMhydroFile
    {
        static int Main(string[] args)
        {
            Logger.Initialize("ModifyMhydroFile.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"ModifyMhydroFile started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length < 2)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 2. Syntax\n mhydroPath\n RunTimeConfigPath");
                Logger.Write();
                return -1;
            }

            string mhydroPath = args[0].Replace("/", "\\"); ;
            string runInfoPath = args[1].Replace("/", "\\"); ;
            if (!File.Exists(runInfoPath))
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"RunTimeConfig File {runInfoPath} did not exist");
                Logger.Write();
                return -1;
            }
            DateTime start;
            DateTime end;
            DateTime tof;
            _GetTimes(runInfoPath, out start, out end, out tof);
            if (start == end)
            {
                end = start.AddDays(5);
            }
            if (File.Exists(mhydroPath))
            {
                _ModifyFile(mhydroPath, start, end, tof);
            }
            else
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"File {mhydroPath} did not exist");
                Logger.Write();
                return -1;
            }
            Logger.Write();
            return 0;
        }

        private static int _ModifyFile(string path, DateTime start, DateTime end, DateTime tof)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                
                using (StreamReader reader = new StreamReader(path))
                {
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.ToUpper().Contains("TOF = "))
                            {
                                int pos = line.ToUpper().IndexOf("TOF = ", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}TOF = '{tof.Year} {tof.Month} {tof.Day} {tof.Hour}:{tof.Minute}:{tof.Second}'";
                            }
                            else if (line.ToUpper().Contains("STARTTIME"))
                            {
                                int pos = line.ToUpper().IndexOf("STARTTIME", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}StartTime = '{start.Year} {start.Month} {start.Day} {start.Hour}:{start.Minute}:{start.Second}'";
                            }
                            else if (line.ToUpper().Contains("ENDTIME"))
                            {
                                int pos = line.ToUpper().IndexOf("ENDTIME", 0, StringComparison.InvariantCultureIgnoreCase);
                                line = $"{line.Substring(0, pos)}EndTime = '{end.Year} {end.Month} {end.Day} {end.Hour}:{end.Minute}:{end.Second}'";
                            }
                            writer.WriteLine(line);
                        }
                    }
                }
                if (File.Exists(path + "_backup"))
                {
                    File.Delete(path + "_backup");
                }
                File.Move(path, path + "_backup");
                File.Move(tempFile, path);
            }
            catch (Exception ex)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"File {path} modification error = {ex.Message}");
                Logger.Write();
                return -1;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            return 0;
        }

        private static void _GetTimes(string runTimeConfig, out DateTime start, out DateTime end, out DateTime tof)
        {

            if (!File.Exists(runTimeConfig))
                throw new Exception($"Run info file '{runTimeConfig}' does not exist.");

            try
            {
                var xmlDoc = new XmlDocument();
                using (var xtr = new XmlTextReader(runTimeConfig) { Namespaces = false })
                {

                    xmlDoc.Load(xtr);
                    string timeZone = xmlDoc.SelectSingleNode("/Run/timeZone").InnerText;
                    double timeZoneD = double.Parse(timeZone, CultureInfo.InvariantCulture);
                    string date = xmlDoc.SelectSingleNode("/Run/startDateTime").Attributes["date"].InnerText;
                    string time = xmlDoc.SelectSingleNode("/Run/startDateTime").Attributes["time"].InnerText;
                    start = DateTime.ParseExact(date + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    date = xmlDoc.SelectSingleNode("/Run/endDateTime").Attributes["date"].InnerText;
                    time = xmlDoc.SelectSingleNode("/Run/endDateTime").Attributes["time"].InnerText;                    
                    end = DateTime.ParseExact(date + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    date = xmlDoc.SelectSingleNode("/Run/time0").Attributes["date"].InnerText;
                    time = xmlDoc.SelectSingleNode("/Run/time0").Attributes["time"].InnerText;
                    tof = DateTime.ParseExact(date + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                throw new Exception("Unable to parse time0 from RunInfo file.");
            }
        }
    }
}
