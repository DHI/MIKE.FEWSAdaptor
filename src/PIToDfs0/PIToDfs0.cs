using Dfs0Handeling;
using FewsCommon;
using FewsCommon.TimeSeriesNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIToDfs0
{
    class PIToDfs0
    {
        static int Main(string[] args)
        {
            Logger.Initialize("PIToDfs0.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"Dfs0ToPI started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length < 2)
            {
                ////Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 2. \n Syntax: \n InputPIFilePath\n ModelRoot\n [StartDate (yyyy-MM-dd HH:mm:ss)]\n [EndDate (yyyy-mm-dd HH:MM:SS)]\n [EnsembleId]\n [EnsembleMemberId]\n  [EnsembleMemberIndex]\n [prefix]\n [OutputDfs0]");
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must be at least 2. \n Syntax: \n InputPIFilePath\n ModelRoot\n [OutputDfs0] [parameterType]");
                Logger.Write();
                return -1;
            }
            string inputPIFilePath = args[0].Replace("/", "\\"); 
            string modelRoot = args[1].Replace("/", "\\");

            string relativePath = string.Empty;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MaxValue;
            string parameterType = "";
            string ensembleId = "";
            string ensembleMemberId = "";
            string prefix = "";
            if (args.Count() > 2)
            {
                relativePath = args[2];
            }

            if (args.Count() > 3)
            {
                parameterType = args[3];
            }

            Logger.AddLog(Logger.TypeEnum.Info, $"Reading wrapper input file {inputPIFilePath}.");
            var pi = PIReader.Read(inputPIFilePath);

            var dfsWriter = new Dfs0Writer();
            try
            {
                //// public bool WriteDfs0File(PI pi, string rootPath, DateTime startTime, DateTime endTime, string relativePath = "", string ensembleId = "", string ensembleMemberId ="", string prefix = "")
                dfsWriter.WriteDfs0File(pi, modelRoot, startTime, endTime, relativePath, ensembleId, ensembleMemberId, prefix);
            }
            catch (Exception ex)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Program error {ex.Message}.");
                Logger.Write();
                return -1;
            }
            Logger.Write();
            return 0;
        }
    }
}
