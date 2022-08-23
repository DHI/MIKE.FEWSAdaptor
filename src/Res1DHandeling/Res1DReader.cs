using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DHI.Mike1D.Generic;
using DHI.Mike1D.ResultDataAccess;
using FewsCommon;
using FewsCommon.TimeSeriesNS;

namespace Res1DHandeling
{
    public class Res1DReader
    {
        private string _resFileName = string.Empty;
        private IResultData _resultData = new ResultData();
        private bool _XmlLoaded = false;
        private IList<string> _selectedResults = new List<string>();
        
        public Res1DReader(string resFileName)
        {
            _resFileName = resFileName;
        }

        public string ResFileName { get => _resFileName; }

        public bool Loaded { get; private set; } = false;

        public IList<string> SelectedResults { get => _selectedResults; private set => _selectedResults = value; }

        public bool XmlLoaded { get => _XmlLoaded; set => _XmlLoaded = value; }

        public bool LoadSelectionXML(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(path))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "OneResult":
                                        var location = reader["LocationId"];
                                        var atribute = reader["AtributeId"];
                                        SelectedResults.Add($"{location}_{atribute}");
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLog(Logger.TypeEnum.Error, $"File {path} read error {ex.Message}");
                    return false;
                }
                XmlLoaded = true;
            }
            else
            {
                Logger.AddLog(Logger.TypeEnum.Error, $"XML selection file {path} did not exist");
                return false;
            }
            return true;
        }

        public bool LoadResults()
        {
            
            if (_resultData.Connection.FilePath.BaseFilePath== null)
            {
                _resultData.Connection = Connection.Create(_resFileName);               
            }
            Diagnostics resultDiagnostics = new Diagnostics("Import1DResult");
            _resultData.Load(resultDiagnostics);
            if (resultDiagnostics.Errors.Count != 0)
            {
                foreach (var errorItem in resultDiagnostics.Errors)
                {
                    Logger.AddLog(Logger.TypeEnum.Error, errorItem.Message);                    
                }
                return false;
            }
            else
            {
                Loaded = true;
            }
            return true;
        }

        public IList<string> GetNodeNames()
        {
            var nodeList = new List<string>();
            if (!Loaded)
            {
                if (!LoadResults())
                {
                    return nodeList;
                }
            }
            foreach (var node in _resultData.Nodes)
            {
                nodeList.Add(node.Id);
            }
            return nodeList;
        }
        public void GetStartEnd(ref DateTime start, ref DateTime end)
        {
            
            if (!Loaded)
            {
                if (!LoadResults())
                {
                    return;
                }
            }
            start = _resultData.StartTime;
            end = _resultData.EndTime;
        }

        public IList<string> GetBrancheGridPointNames()
        {
            var nodeList = new List<string>();
            if (!Loaded)
            {
                if (!LoadResults())
                {
                    return nodeList;
                }
            }
            foreach (var reach in _resultData.Reaches)
            {
                nodeList.Add(reach.Id);
                foreach (var item in reach.DataItems)
                {
                    var num = item.NumberOfElements;
                }
            }
            return nodeList;
        }

        private bool _PrepareTimeAxis()
        {
            if (!Loaded)
            {
                if (!LoadResults())
                {
                    return false;
                }
            }
            return true;
        }

        private void _AddAllGridPointTS2PI(ref PI pi, IRes1DReach reach, int itemPos)
        {
            for (int i = 0; i < reach.DataItems[itemPos].NumberOfElements; i++)           
            {
                _AddOneTS(ref pi, reach, itemPos, i, reach.DataItems[itemPos].IndexList[i]);
            }
        }

        private void _AddOneTS(ref PI pi, IRes1DCatchment catchment, int itemPos)
        {
            if (SelectedResults.Count > 0)
            {
                var fullId = $"Catchment;{catchment.ID}_{catchment.DataItems[itemPos].Quantity.EumQuantity.Item.ToString()}";
                if (!SelectedResults.Contains(fullId))
                {
                    return;
                }
            }
            var ts = new TimeSeries();
            ts.Type = "instantaneous";
            ts.X = catchment.CenterPoint.X;
            ts.Y = catchment.CenterPoint.Y;
            ts.Z = catchment.CenterPoint.Y;
            ts.LocationId = $"Catchment;{catchment.ID}";
            ts.StationName = catchment.ID;
            ts.ParameterId = catchment.DataItems[itemPos].Quantity.EumQuantity.Item.ToString() + ";" + catchment.DataItems[itemPos].Quantity.EumQuantity.Unit.ToString(); ;
            ts.MissVal = -99999.999;
            ts.StartDate = _resultData.StartTime;
            ts.EndDate = _resultData.EndTime;
            ts.Units = "";
            var deleteVal = _resultData.DeleteValue;
            for (int i = 0; i < _resultData.NumberOfTimeSteps; i++)
            {
                double value = ts.MissVal.Value;
                float fvalue = Convert.ToSingle(ts.MissVal.Value);
                fvalue = catchment.DataItems[itemPos].GetValue(i, 0);
                if (Math.Abs(fvalue - deleteVal) > float.Epsilon)
                {
                    value = fvalue;
                }
                ts.Values.Add(_resultData.TimesList[i], new TSValue(value));
            }
            pi.TimeSeries.Add(ts);
        }

        private void _AddOneTS(ref PI pi, IRes1DReach reach, int itemPos, int index, int elementIndex)
        {
            if (SelectedResults.Count > 0)
            {
                var fullId = $"GridPoint;{reach.Name};{reach.GridPoints[elementIndex].Chainage}_{reach.DataItems[itemPos].Quantity.EumQuantity.Item.ToString()}";
                if (!SelectedResults.Contains(fullId))
                {
                    return;
                }
            }
            var ts = new TimeSeries();
            ts.Type = "instantaneous";
            var resultGridPoint = reach.GridPoints[elementIndex] as Res1DGridPoint;
            ts.X = resultGridPoint.X;
            ts.Y = resultGridPoint.Y;
            ts.Z = resultGridPoint.Z;
            var typeName = resultGridPoint.PointType.ToString();
            ts.LocationId = $"{reach.Name} - {reach.GridPoints[elementIndex].Chainage} - {typeName} - {reach.DataItems[itemPos].ItemId}";
            ts.TimeStep = new TimeSpan(1, 0, 0);
            if (_resultData.NumberOfTimeSteps > 1)
            {
                ts.TimeStep = _resultData.TimesList[1] - _resultData.TimesList[0];
            }
            ts.StationName = ts.LocationId;
            ts.ParameterId = reach.DataItems[itemPos].Quantity.EumQuantity.Item.ToString() + ";" + reach.DataItems[itemPos].Quantity.EumQuantity.Unit.ToString();
            ts.MissVal = -99999.999;
            ts.StartDate = _resultData.StartTime;
            ts.EndDate = _resultData.EndTime;
            ts.Units = "";
            var deleteVal = _resultData.DeleteValue;
            for (int i = 0; i < _resultData.NumberOfTimeSteps; i++)
            {
                double value = ts.MissVal.Value;
                float fvalue = Convert.ToSingle(ts.MissVal.Value);
                fvalue = reach.DataItems[itemPos].GetValue(i, index);
                if (Math.Abs(fvalue - deleteVal) > float.Epsilon)
                {
                    value = fvalue;
                }
                ts.Values.Add(_resultData.TimesList[i], new TSValue(value));
            }
            pi.TimeSeries.Add(ts);
        }

        private void _AddOneTS(ref PI pi, IRes1DNode node, int itemPos)
        {

            if (SelectedResults.Count > 0)
            {
                var fullId = $"{node.DataItems[itemPos].Quantity.EumQuantity.Item.ToString()}_Node;{node.Id}";
                if (!SelectedResults.Contains(fullId))
                {
                    return;
                }
            }
            var ts = new TimeSeries();
            ts.Type = "instantaneous";
            ts.X = node.XCoordinate;
            ts.Y = node.YCoordinate;
            ts.Z = 0;
            ts.LocationId = $"Node;{node.Id}";
            ts.TimeStep = new TimeSpan(1, 0, 0);
            if (_resultData.NumberOfTimeSteps > 1)
            {
                ts.TimeStep = _resultData.TimesList[1] - _resultData.TimesList[0];
            }
            ts.StationName = node.ID;
            ts.ParameterId = node.DataItems[itemPos].Quantity.EumQuantity.Item.ToString() + ";" + node.DataItems[itemPos].Quantity.EumQuantity.Unit.ToString();
            ts.MissVal = -99999.999;
            ts.StartDate = _resultData.StartTime;
            ts.EndDate = _resultData.EndTime;
            ts.Units = "";
            var deleteVal =_resultData.DeleteValue;
            for (int i = 0; i < _resultData.NumberOfTimeSteps; i++)
            {
                double value = ts.MissVal.Value;
                float fvalue = Convert.ToSingle(ts.MissVal.Value);
                fvalue = node.DataItems[itemPos].GetValue(i, 0);
                if (Math.Abs(fvalue - deleteVal) > float.Epsilon)
                {
                    value = fvalue;
                }
                ts.Values.Add(_resultData.TimesList[i], new TSValue(value));
            }
            pi.TimeSeries.Add(ts);
        }

        public PI AddAllTS()
        {
            PI pi = new PI();
            var nodeList = new List<string>();
            if (!Loaded)
            {
                if (!LoadResults())
                {
                    return null;
                }
            }
            foreach (var node in _resultData.Nodes)
            {
                _AddNodeTS2PI(ref pi, node);
            }
            foreach (var reach in _resultData.Reaches)
            {
                _AddReachTS2PI(ref pi, reach);
            }            
           return pi;
        }


        private void _AddReachTS2PI(ref PI pi, IRes1DReach reach, int itemPos = -1)
        {
            if (pi == null)
            {
                pi = new PI();
            }
            if (itemPos == -1)
            {
                for (int i = 0; i < reach.DataItems.Count(); i++)
                {
                    _AddAllGridPointTS2PI(ref pi, reach, i);
                }
            }
            else
            {
                _AddAllGridPointTS2PI(ref pi, reach, itemPos);
            }
        }

        private void _AddNodeTS2PI(ref PI pi, IRes1DNode node, int itemPos = -1)
        {
            if (pi == null)
            {
                pi = new PI();
            }
            if (itemPos == -1)
            {
                for (int i = 0; i < node.DataItems.Count(); i++)
                {
                    _AddOneTS(ref pi, node, i);
                }
            }
            else
            {
                _AddOneTS(ref pi, node, itemPos);
            }            
        }

        public void ExportAllResultSelection(string xmlPath)
        {
            if (!Loaded)
            {
                LoadResults();                
            }

            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            using (var xmlWriter = XmlWriter.Create(xmlPath, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("SelectedResults");
                foreach (var node in _resultData.Nodes)
                {
                    for (int i = 0; i < node.DataItems.Count(); i++)
                    {
                        xmlWriter.WriteStartElement("OneResult");
                        xmlWriter.WriteAttributeString("LocationId", $"Node;{node.Id}");
                        xmlWriter.WriteAttributeString("AtributeId", node.DataItems[i].Quantity.EumQuantity.Item.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                foreach (var reach in _resultData.Reaches)
                {
                    for (int itemPos = 0; itemPos < reach.DataItems.Count(); itemPos++)
                    {
                        for (int i = 0; i < reach.DataItems[itemPos].NumberOfElements; i++)
                        {                        
                            int elementIndex = reach.DataItems[itemPos].IndexList[i];
                            xmlWriter.WriteStartElement("OneResult");
                            string locationId = $"GridPoint;{reach.Name};{reach.GridPoints[elementIndex].Chainage}";
                            xmlWriter.WriteAttributeString("LocationId", locationId);
                            xmlWriter.WriteAttributeString("AtributeId", reach.DataItems[itemPos].Quantity.EumQuantity.Item.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
            }
        }
    }
}
