using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FewsCommon.TimeSeriesNS
{
    public class GeneralReader
    {
        protected static string _readNonEmptyLine(StreamReader reader)
        {
            string line;

            do
            {
                line = reader.ReadLine();
            }
            while (line != null && line.Length <= 1);

            return line;
        }
    }
}
