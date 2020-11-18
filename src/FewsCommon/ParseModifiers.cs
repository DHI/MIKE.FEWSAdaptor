using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FewsCommon
{
	/// <summary>
	/// Parse FEWS RuntimeInfo .xml and provide list of Modifiers.
	/// </summary>
    public class ParseModifiers
    {
        private string _fileName;
        private IDictionary<string, object> _parameters;
		/// 
		/// <param name="fileName">RuntimeInfo file name / path</param>
        public ParseModifiers(string fileName)
        {
            _fileName = fileName;
            _parameters = new Dictionary<string, object>();
        }
		/// <summary>
		/// Get modifiers (name and value) dictionaty
		/// </summary>
        public IDictionary<string, object> Getparameters()
        {
            return _parameters;
        }

		/// <summary>
		/// Load RuntimeInfo file
		/// </summary>
        public bool LoadFile()
        {
            using (XmlReader reader = XmlReader.Create(_fileName))
            {
                var currentPath = new List<string>();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (string.Compare(reader.Name, "parameter", true) == 0)
                        {
                            string attr = reader["id"];
                            object obj = null;

                            reader.Read();
                            bool cont = true;
                            while (cont)
                            {
                                if (reader.NodeType != XmlNodeType.Element)
                                {
                                    reader.Read();
                                }
                                else
                                {
                                    switch (reader.Name)
                                    {
                                        case "intValue":
                                            reader.Read();
                                            obj = int.Parse(reader.Value);
                                            cont = false;
                                            break;
                                        case "dblValue":
                                            reader.Read();
                                            obj = double.Parse(reader.Value.Trim(), CultureInfo.InvariantCulture);
                                            cont = false;
                                            break;
                                        case "stringValue":
                                            reader.Read();
                                            obj = reader.Value.Trim();
                                            cont = false;
                                            break;
                                        default:
                                            reader.Read();
                                            break;

                                    }
                                    if (obj != null)
                                    {
                                        _parameters.Add(attr, obj);
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
