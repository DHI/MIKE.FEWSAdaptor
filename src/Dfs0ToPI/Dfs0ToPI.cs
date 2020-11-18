using FewsCommon;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FewsCommon.TimeSeriesNS;
using Dfs0Handeling;

namespace Dfs0ToPI
{
    class Dfs0ToPI
    {
        static int Main(string[] args)
        {
            Logger.Initialize("Dfs0ToPI.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"Dfs0ToPI started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length < 6)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 6. \n Syntax: \n OutputPIFilePath\n ModelRoot\n EnsembleId\n EnsembleMemberId\n  EnsembleMemberIndex\n InputDfs0FilePath\n [InputDfs0FilePath]\n...");
                Logger.Write();
                return -1;
            }
            string ensembleId = "";
            string ensembleMemberId = "";
            int ensembleMemberIndex = -1;
            string outputFileName = "..\\pi-output\\pi-output.xml";
            if (!string.IsNullOrEmpty(args[0]))
            {
                outputFileName = args[0];
            }
            string modelRootPath = args[1];
            ensembleId = args[2];
            ensembleMemberId = args[3];
            if (!int.TryParse(args[4], out ensembleMemberIndex))
            {
                ensembleMemberIndex = -1;
            }
            var dfs0FileList = new List<string>();
            for (int i = 5; i < args.Length; i++)
            {
                dfs0FileList.Add(args[i]);
            }

            var pis = new List<PI>();           
            Logger.Write();
            foreach (var filePath in dfs0FileList)
            {
                if (File.Exists(Path.Combine(modelRootPath, filePath)))
                {
                    var pi_dfs0 = new PI();
                    var dfs0Reader = new ReadDfs0();
                    try
                    {
                        dfs0Reader.ReadDfs0File(ref pi_dfs0, modelRootPath, filePath, ensembleId, ensembleMemberId, ensembleMemberIndex);
                    }
                    catch (Exception ex)
                    {                        
                        Logger.AddLog(Logger.TypeEnum.Error, $"File {Path.Combine(modelRootPath, filePath)} read error {ex.Message}");
                        Logger.Write();
                        return -1;
                    }
                    Logger.AddLog(Logger.TypeEnum.Info, $"File {Path.Combine(modelRootPath, filePath)} parsed.");
                    pis.Add(pi_dfs0);
                }
                else
                {
                    Logger.AddLog(Logger.TypeEnum.Error, $"File {Path.Combine(modelRootPath, filePath)} did not exist");
                    Logger.Write();
                    return -1;
                }
            }
            PIWriter.Write(outputFileName, pis);
            Logger.Write();            
            return 0;
        }
    }
}
