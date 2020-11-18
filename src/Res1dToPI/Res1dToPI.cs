using FewsCommon;
using FewsCommon.TimeSeriesNS;
using Res1DHandeling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Res1dToPI
{
    class Res1dToPI
    {
        static int Main(string[] args)
        {
            Logger.Initialize("Res1dToPI.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"Res1dToPI started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length < 2)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 2. \n Syntax: \n InputRes1dFilePath\n OutputPIFilePath\n [OutputConfigurationXMLFile]");
                Logger.Write();
                return -1;
            }
            string inputRes1dPath = args[0].Replace("/", "\\"); ;
            string outputPIPath = args[1].Replace("/", "\\"); ;
            string configXMLPath = string.Empty;
            if (args.Count() > 2)
            {
                configXMLPath = args[2];
            }          
            var res1dReader = new Res1DReader(inputRes1dPath);
            if (!string.IsNullOrEmpty(configXMLPath))
            {
                res1dReader.LoadSelectionXML(configXMLPath);
            }
            var pi = res1dReader.AddAllTS();
            var pis = new List<PI>();
            pis.Add(pi);
            PIWriter.Write(outputPIPath, pis);
            Logger.Write();
            return 0;
        }
    }
}
