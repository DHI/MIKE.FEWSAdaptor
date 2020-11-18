using FewsCommon;
using Res1DHandeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAllXML
{
    class GenerateAllXML
    {
        static void Main(string[] args)
        {
            Logger.Initialize("GenerateAllXML.log", true);
            Logger.AddLog(Logger.TypeEnum.Info, $"GenerateAllXML started with parameters \"{string.Join("\", \"", args)}\".");
            if (args.Length !=  2)
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"Wrong number of arguments must 2. \n Syntax: \n InputRes1dFilePath\n OutputXML");
                Logger.Write();
                Console.WriteLine("GenerateAllXML Syntax");
                Console.WriteLine("  input Res1d file path");
                Console.WriteLine("  output XML file path");

                return;
            }
            string inputRes1dPath = args[0];
            string outputXMLPath = args[1];
            var res1dReader = new Res1DReader(inputRes1dPath);
            res1dReader.LoadResults();
            res1dReader.ExportAllResultSelection(outputXMLPath);
            Logger.Write();
        }
    }
}
