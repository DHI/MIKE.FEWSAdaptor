using FewsCommon;
using Res1DHandeling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace ModifyMIKESetupFile
{
    enum enumFileType
    {
        mikeHydro,
        mike1D,
        mike11,
        mike11FF,
        mike11DA,
        couple,
        mikeFM
    }

    class ModifyMIKESetupFile
    {
        static string hotStartName = "";
        static DateTime hotstartStart = DateTime.MinValue;
        static DateTime hotstartEnd = DateTime.MaxValue;

        static int Main(string[] args)
        {
            Logger.Initialize("ModifyMIKESetupFile.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"ModifyMIKESetupFile started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length < 2)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 2. Syntax\n mhydroPath\n RunTimeConfigPath\n hotstartfile");
                Logger.Write();
                return -1;
            }

            string mikeSetupPath = args[0].Replace("/", "\\"); ;
            string runInfoPath = args[1].Replace("/", "\\"); ;
            if (!File.Exists(runInfoPath))
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"RunTimeConfig File {runInfoPath} did not exist");
                Logger.Write();
                return -1;
            }
            
            if (args.Count() > 2)
            {
                hotStartName = args[2].Replace("/", "\\");
                if (File.Exists(hotStartName))
                {
                    var res1dReader = new Res1DReader(hotStartName);
                    res1dReader.GetStartEnd(ref hotstartStart, ref hotstartEnd);
                }
            }
            DateTime start;
            DateTime end;
            DateTime tof;
            enumFileType fileType = enumFileType.mikeHydro;
            _GetTimes(runInfoPath, out start, out end, out tof);
            if (start == end)
            {
                end = start.AddDays(5);
            }
            if (File.Exists(mikeSetupPath))
            {
                string ext = Path.GetExtension(mikeSetupPath).ToUpper();
                switch (ext)
                {
                    case ".SIM11":
                        fileType = enumFileType.mike11;
                        break;
                    case ".M1DX":
                        fileType = enumFileType.mike1D;
                        break;
                    case ".MHYDRO":
                        fileType = enumFileType.mikeHydro;
                        break;
                    case ".COUPLE":
                        fileType = enumFileType.mikeHydro;
                        break;
                    case ".FF11":
                        fileType = enumFileType.mike11FF;
                        break;
                    case ".DA11":
                        fileType = enumFileType.mike11DA;
                        break;
                }
                _ModifyFile(mikeSetupPath, start, end, tof, fileType);
            }
            else
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"File {mikeSetupPath} did not exist");
                Logger.Write();
                return -1;
            }
            Logger.Write();
            return 0;
        }

        private static void _ModifyLineContent(ref string line, DateTime start, DateTime end, DateTime tof, enumFileType modelType)
        {
            try 
            {
                switch (modelType)
                {
                    case enumFileType.mikeHydro:
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
                        }
                        break;
                    case enumFileType.mike11:
                        {
                            if (line.ToUpper().Contains("START = "))
                            {
                                int pos = line.ToUpper().IndexOf("START = ", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}START = {start.Year}, {start.Month}, {start.Day}, {start.Hour}, {start.Minute}, {start.Second}";
                            }
                            if (line.ToUpper().Contains("END = "))
                            {
                                int pos = line.ToUpper().IndexOf("END = ", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}END = {end.Year}, {end.Month}, {end.Day}, {end.Hour}, {end.Minute}, {start.Second}";
                            }
                            var splited = line.Split('|');
                            if (splited.Length == 3)
                            {
                                if (!string.IsNullOrEmpty(hotStartName))
                                {
                                    string shortHotstart = splited[1].TrimStart('.');
                                    shortHotstart = shortHotstart.TrimStart('\\');
                                    if (hotStartName.IndexOf(shortHotstart) > 0)
                                    {
                                        char[] separators = new char[] {',' };
                                        var splited1 = splited[2].Split(separators);
                                        try
                                        {
                                            if (splited1.Count() == 8)
                                            {
                                                var list = splited1.ToList();
                                                var intList = list.ConvertAll(new Converter<string, int>(StringToInt));
                                                var hot = new DateTime(intList[2], intList[3], intList[4], intList[5], intList[6], intList[7]);
                                                var hotOrig = hot;
                                                if (hot != null)
                                                {
                                                    if (hot < hotstartStart)
                                                    {
                                                        hot = hotstartStart;
                                                    }
                                                    else if (hot > hotstartEnd)
                                                    {
                                                        hot = hotstartEnd;
                                                    }
                                                    if (hot != hotOrig)
                                                    {
                                                        line = splited[0] + '|' + splited[1] + '|' + splited1[0] + ',' + splited1[1];
                                                        line = line + $",{hotstartStart.Year},{hotstartStart.Month}, {hotstartStart.Day}, {hotstartStart.Hour},{hotstartStart.Minute},{hotstartStart.Second}";
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }                                   
                                }
                            }
                        }
                        break;
                    case enumFileType.mike11DA:
                        {
                            if (line.ToUpper().Contains("Time_of_forecast = ".ToUpper()))
                            {
                                int pos = line.IndexOf("Time_of_forecast", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}Time_of_forecast = {tof.Year}, {tof.Month}, {tof.Day}, {tof.Hour}, {tof.Minute}, {tof.Second}";
                            }                           
                        }
                        break;
                    case enumFileType.mike11FF:
                        {
                            if (line.ToUpper().Contains("FC_length = ".ToUpper()))
                            {
                                int pos = line.IndexOf("FC_length", 0, StringComparison.InvariantCultureIgnoreCase);
                                line = $"{line.Substring(0, pos)}FC_length = {(end - tof).TotalHours}";
                            }
                            if (line.ToUpper().Contains("FC_unit = ".ToUpper()))
                            {
                                int pos = line.IndexOf("FC_unit", 0, StringComparison.InvariantCultureIgnoreCase);
                                line = $"{line.Substring(0, pos)}FC_unit = 0";
                            }
                        }
                        break;
                    case enumFileType.mike1D:
                        {
                            if (line.ToUpper().Contains("<SimulationStart>".ToUpper()))
                            {
                                int pos = line.ToUpper().IndexOf("<SimulationStart>", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}<SimulationStart>{start.ToString("s")}</SimulationStart>";
                            }
                            if (line.ToUpper().Contains("<Simulationend>".ToUpper()))
                            {
                                int pos = line.ToUpper().IndexOf("<Simulationend>", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}<Simulationend>{end.ToString("s")}</Simulationend>";
                            }
                            if (line.ToUpper().Contains("<StartTime>".ToUpper()))
                            {
                                int pos = line.ToUpper().IndexOf("<StartTime>", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}<StartTime>{start.ToString("s")}</StartTime>";
                            }
                            if (line.ToUpper().Contains("<Time>".ToUpper()))
                            {
                                int pos = line.ToUpper().IndexOf("<Time>", 0, StringComparison.InvariantCultureIgnoreCase);

                                line = $"{line.Substring(0, pos)}<Time>{start.ToString("s")}</Time>";
                            }
                        }
                        break;                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Line <{line}> exception {ex.Message}");
            }            
        }

        public static int StringToInt(string str)
        {
            int res = int.MaxValue;
            var str1 = str.Trim(' ');
            if (int.TryParse(str1, out res))
            {
                return res;
            }
            return int.MaxValue;
        } 

        private static int _ModifyFile(string path, DateTime start, DateTime end, DateTime tof, enumFileType modelType = enumFileType.mikeHydro)
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
                            _ModifyLineContent(ref line, start, end, tof, modelType);
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
