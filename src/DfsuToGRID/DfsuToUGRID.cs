using System;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.NetCDF4;
using System.Linq;
using Microsoft.Research.Science.Data.Imperative;
using System.IO;
using DHI.Generic.MikeZero.DFS;
using DHI.Generic.MikeZero.DFS.mesh;
using DHI.Generic.MikeZero.DFS.dfsu;
using System.Net;
using NetCDFInterop;
using NetTopologySuite.Geometries;
using System.Text;
using System.Linq.Expressions;

class DfsuToUGRID
{
    static void Main(String[] args)
    {
        // Path for the NetCDF file to be created
        string ncFileName = "example.nc";
        string dfsuFilePath = "example.dfsu";
        string logFilePath = "example.log";
        // Open the DFSU file
        
        if (args.Count() < 2)
        {
            Console.WriteLine("WriteDfsu.exe have following parameters");
            Console.WriteLine("Input Dfsu file name");
            Console.WriteLine("Input NetCdf UGRID file name");
            Console.WriteLine("Write geometry UGRID [optional] 0 -  write full file, 1 - write only geometry to be used as geometry definition in FEWS. Default 0");
            Console.WriteLine("Item number (1 - based) to be written from Dfsu file [optional] Default 1");
            Console.WriteLine("Index (0 based) of the first time step from Dfsu to be saved in NetCdf [optional] Default 0");
            Console.WriteLine("Load every n step [optional] Default 1");
            Console.WriteLine("Maximun number of values in one commit. Can be used to optimize memory use / speed [optional] Default 10000000");
            return;
        }
        dfsuFilePath = args[0];
        ncFileName = args[1];
        using (var _dfsuFile = DfsFileFactory.DfsuFileOpen(dfsuFilePath))
        {
            var factory = new DfsFactory();
            var proj = factory.CreateProjection(_dfsuFile.Projection.WKTString);

            var _numElements = _dfsuFile.NumberOfElements;
            int _numNodes = _dfsuFile.NumberOfNodes;
            int geometry_only = 0;
            int itemNum = 1;
            int firstTimeStep = 0;
            int stepsToLoad = 1;
            ////int fillConectionValue = -999;
            float fillFloatValue = 1E20f;
            ////double fillDouble = 1E20;
            int maxNumNumbers = 10000000;

            if (args.Length > 2)
            {
                geometry_only = int.Parse(args[2]);
            }

            if (args.Length > 3)
            {
                itemNum = int.Parse(args[3]);
            }
            if (args.Length > 4)
            {
                firstTimeStep = int.Parse(args[4]);
            }
            if (args.Length > 5)
            {
                stepsToLoad = int.Parse(args[5]);
            }


            logFilePath = ncFileName.Replace(".nc", ".log");
            var sb = new StringBuilder();
            DateTime writeStart = DateTime.Now;
            sb.AppendLine($"DFSu file name = {dfsuFilePath}");
            sb.AppendLine($"Result UGRID (NetCdf) file = {ncFileName}");
            if (geometry_only == 1)
            {
                sb.AppendLine("Only iregular definition file UGRID created. To be placed in MapLayerFiles dir");
            }

            // Open a NetCDF file (create or overwrite it)
            if (File.Exists(ncFileName))
            {
                File.Delete(ncFileName);
            }
            using (var dataset = Microsoft.Research.Science.Data.NetCDF4.NetCDFDataSet.Open(ncFileName, ResourceOpenMode.CreateNew))
            {
                try
                {
                    var TopologyVar = dataset.AddVariable<int>("mesh2d");
                    dataset.PutAttr("mesh2d", "cf_role", "mesh_topology");
                    dataset.PutAttr("mesh2d", "long_name", "Topology data of 2D mesh");
                    dataset.PutAttr("mesh2d", "topology_dimension", 2);
                    dataset.PutAttr("mesh2d", "node_coordinates", "mesh2d_node_x mesh2d_node_y");
                    dataset.PutAttr("mesh2d", "node_dimension", "mesh2d_nNodes");
                    dataset.PutAttr("mesh2d", "face_node_connectivity", "mesh2d_face_nodes");
                    dataset.PutAttr("mesh2d", "max_face_nodes_dimension", "mesh2d_nMax_face_nodes");
                    dataset.PutAttr("mesh2d", "face_dimension", "nmesh2d_face");
                    dataset.PutAttr("mesh2d", "face_coordinates", "mesh2d_face_x mesh2d_face_y");

                    var dataVarX = dataset.AddVariable<double>("mesh2d_node_x", "mesh2d_nNodes");
                    dataset.PutAttr("mesh2d_node_x", "units", "m");
                    dataset.PutAttr("mesh2d_node_x", "standard_name", "projection_x_coordinate");
                    dataset.PutAttr("mesh2d_node_x", "long_name", "x -coordinate of mesh nodes");
                    dataset.PutAttr("mesh2d_node_x", "mesh", "mesh2d");
                    dataset.PutAttr("mesh2d_node_x", "location", "node");
                    double[] xData = new double[_numNodes];

                    for (int i = 0; i < _numNodes; i++)
                    {
                        xData[i] = _dfsuFile.X[i];
                    }
                    dataVarX.PutData(xData);

                    var dataVarY = dataset.AddVariable<double>("mesh2d_node_y", "mesh2d_nNodes");
                    double[] yData = new double[_numNodes];
                    for (int i = 0; i < _numNodes; i++)
                    {
                        yData[i] = _dfsuFile.Y[i];
                    }
                    dataVarY.PutData(yData);
                    dataset.PutAttr("mesh2d_node_y", "units", "m");
                    dataset.PutAttr("mesh2d_node_y", "standard_name", "projection_y_coordinate");
                    dataset.PutAttr("mesh2d_node_y", "long_name", "y -coordinate of mesh nodes");
                    dataset.PutAttr("mesh2d_node_y", "mesh", "mesh2d");
                    dataset.PutAttr("mesh2d_node_y", "location", "node");

                    var dataVarConnection = dataset.AddVariable<int>("mesh2d_face_nodes", "nmesh2d_face", "mesh2d_nMax_face_nodes");
                    int maxNodesInElement = 3;
                    for (int i = 0; i < _numElements; i++)
                    {
                        maxNodesInElement = Math.Max(maxNodesInElement, _dfsuFile.ElementTable[i].Length);

                    }

                    var faceNode = new int[_numElements, maxNodesInElement];
                    for (int i = 0; i < _numElements; i++)
                    {
                        for (int j = 0; j < maxNodesInElement; j++)
                        {
                            if (j < _dfsuFile.ElementTable[i].Length)
                            {
                                faceNode[i, j] = _dfsuFile.ElementTable[i][j];
                            }
                            else
                            {
                                //faceNode[i, j] = fillConectionValue;
                                faceNode[i, j] = _dfsuFile.ElementTable[i][0];
                            }
                        }
                    }
                    dataVarConnection.PutData(faceNode);
                    dataset.PutAttr("mesh2d_face_nodes", "cf_role", "face_node_connectivity");
                    dataset.PutAttr("mesh2d_face_nodes", "long_name", "Maps every face to its corner nodes");
                    ////dataset.PutAttr("mesh2d_face_nodes", "_FillValue", fillConectionValue);
                    dataset.PutAttr("mesh2d_face_nodes", "start_index", 1);
                    dataset.PutAttr("mesh2d_face_nodes", "location", "face");
                    double[] faceX = new double[_numElements];
                    double[] faceY = new double[_numElements];
                    double[,] faceBoundsX = new double[_numElements, maxNodesInElement];
                    double[,] faceBoundsY = new double[_numElements, maxNodesInElement];

                    for (int i = 0; i < _numElements; i++)
                    {

                        double x = 0;
                        double y = 0;
                        for (var j = 0; j < _dfsuFile.ElementTable[i].Count(); j++)
                        //for (var j = 0; j < maxNodesInElement; j++)
                        {
                            x = x + _dfsuFile.X[_dfsuFile.ElementTable[i][j] - 1] / _dfsuFile.ElementTable.Count();
                            x = x + _dfsuFile.Y[_dfsuFile.ElementTable[i][j] - 1] / _dfsuFile.ElementTable.Count();
                        }
                        faceX[i] = x;
                        faceY[i] = y;
                        for (int j = 0; j < maxNodesInElement; j++)
                        {
                            if (j < _dfsuFile.ElementTable[i].Length)
                            {
                                faceBoundsX[i, j] = _dfsuFile.X[_dfsuFile.ElementTable[i][j] - 1];
                                faceBoundsY[i, j] = _dfsuFile.Y[_dfsuFile.ElementTable[i][j] - 1];
                            }
                            else
                            {
                                //faceBoundsX[i, j] = fillDouble;
                                //faceBoundsY[i, j] = fillDouble;
                                faceBoundsX[i, j] = _dfsuFile.X[_dfsuFile.ElementTable[i][0] - 1];
                                faceBoundsY[i, j] = _dfsuFile.Y[_dfsuFile.ElementTable[i][0] - 1];
                            }
                        }
                    }
                    var dataVarFaceX = dataset.AddVariable<double>("mesh2d_face_x", "nmesh2d_face");
                    dataVarFaceX.PutData(faceX);
                    dataset.PutAttr("mesh2d_face_x", "standard_name", "projection_x_coordinate");
                    dataset.PutAttr("mesh2d_face_x", "long_name", "x - coordinate of mesh face");
                    dataset.PutAttr("mesh2d_face_x", "units", "m");
                    dataset.PutAttr("mesh2d_face_x", "mesh", "mesh2d");
                    dataset.PutAttr("mesh2d_face_x", "location", "face");
                    dataset.PutAttr("mesh2d_face_x", "bounds", "mesh2d_face_xbnds");

                    var dataVarFaceY = dataset.AddVariable<double>("mesh2d_face_y", "nmesh2d_face");
                    dataVarFaceY.PutData(faceY);
                    dataset.PutAttr("mesh2d_face_y", "standard_name", "projection_y_coordinate");
                    dataset.PutAttr("mesh2d_face_y", "long_name", "y - coordinate of mesh face");
                    dataset.PutAttr("mesh2d_face_y", "units", "m");
                    dataset.PutAttr("mesh2d_face_y", "mesh", "mesh2d");
                    dataset.PutAttr("mesh2d_face_y", "location", "face");
                    dataset.PutAttr("mesh2d_face_y", "bounds", "mesh2d_face_ybnds");

                    var dataVarBoundsFaceX = dataset.AddVariable<double>("mesh2d_face_xbnds", "nmesh2d_face", "nMax_mesh2d_face_nodes");
                    dataVarBoundsFaceX.PutData(faceBoundsX);
                    dataset.PutAttr("mesh2d_face_xbnds", "standard_name", "projection_x_coordinate");
                    dataset.PutAttr("mesh2d_face_xbnds", "long_name", "x - coordinate bounds of 2D mesh face (i.e. corner coordinates).");
                    dataset.PutAttr("mesh2d_face_xbnds", "units", "m");
                    dataset.PutAttr("mesh2d_face_xbnds", "location", "face");
                    ////dataset.PutAttr("mesh2d_face_xbnds", "FillValue", fillDouble);


                    var dataVarBoundsFaceY = dataset.AddVariable<double>("mesh2d_face_ybnds", "nmesh2d_face", "nMax_mesh2d_face_nodes");
                    dataVarBoundsFaceY.PutData(faceBoundsY);
                    dataset.PutAttr("mesh2d_face_ybnds", "standard_name", "projection_y_coordinate");
                    dataset.PutAttr("mesh2d_face_ybnds", "long_name", "y - coordinate bounds of 2D mesh face (i.e. corner coordinates).");
                    dataset.PutAttr("mesh2d_face_ybnds", "units", "m");
                    dataset.PutAttr("mesh2d_face_ybnds", "location", "face");
                    ////dataset.PutAttr("mesh2d_face_ybnds", "FillValue", fillDouble);
                    dataset.Commit();
                    if (geometry_only == 0)
                    {
                        var time = dataset.AddVariable<double>("time", "time");
                        firstTimeStep = Math.Min(firstTimeStep, _dfsuFile.NumberOfTimeSteps - 1);
                        var start = _dfsuFile.GetDateTimes()[firstTimeStep];
                        var year = start.Year;
                        var month = start.Month;
                        var day = start.Day;

                        int stepsToRead = 0;
                        stepsToLoad = Math.Min(stepsToLoad, _dfsuFile.NumberOfTimeSteps);
                        for (int i = firstTimeStep; i < _dfsuFile.NumberOfTimeSteps; i = i + stepsToLoad)
                        {
                            stepsToRead++;
                        }
                        double totalHours = 1.0;
                        double[] dfsTimeSteps = new double[stepsToRead];
                        if (stepsToLoad > 1)
                        {
                            totalHours = (_dfsuFile.GetDateTimes()[stepsToLoad] - _dfsuFile.GetDateTimes()[0]).TotalHours;
                        }

                        sb.AppendLine($"time step = {totalHours} hours");
                        dataset.PutAttr("time", "units", $"hours since {year}-{month}-{day} 00:00:00");
                        sb.AppendLine($"time unit = hours since {year}-{month}-{day} 00:00:00");

                        dataset.IsAutocommitEnabled = false;
                        for (int i = 0; i < dfsTimeSteps.Length; i++)
                        {
                            dfsTimeSteps[i] = (_dfsuFile.GetDateTimes()[i * stepsToLoad + firstTimeStep] - new DateTime(year, month, day, 0, 0, 0)).TotalHours;
                        }

                        var dataItem = _dfsuFile.ItemInfo[itemNum - 1];

                        dataset.PutAttr("time", "standard_name", "time");

                        string dfsuItem = dataItem.Name.Trim().Replace(' ', '_').ToLower();
                        sb.AppendLine($"Item name = {dfsuItem}");
                        IDfsItemData dfsItemData = _dfsuFile.ReadItemTimeStep(itemNum, 0);
                        bool convertToFloat = dataItem.DataType != DfsSimpleType.Float;
                        float noValue = _dfsuFile.DeleteValueFloat;
                        var dataVarTimeStepsValues = dataset.AddVariable<float>(dfsuItem, "time", "nmesh2d_face");

                        int saved = 0;
                        for (int i = 0; i < dfsTimeSteps.Length; i++)
                        {
                            _dfsuFile.ReadItemTimeStep(dfsItemData, i * stepsToLoad + firstTimeStep);
                            double currentTime = (_dfsuFile.GetDateTimes()[i * stepsToLoad + firstTimeStep] - new DateTime(year, month, day, 0, 0, 0)).TotalHours;
                            float val = 0;
                            time.Append(currentTime);
                            float[] dfsValues = new float[_numElements];
                            for (int j = 0; j < dfsItemData.Data.Length; j++)
                            {
                                if (convertToFloat)
                                {
                                    val = Convert.ToSingle((double)dfsItemData.Data.GetValue(j));
                                }
                                else
                                {
                                    val = (float)dfsItemData.Data.GetValue(j);
                                    //if (Math.Abs(val - noValue) < float.Epsilon)
                                    //{
                                    //    val = fillFloatValue;
                                    //}
                                }
                                dfsValues[j] = val;
                            }
                            dataVarTimeStepsValues.Append(dfsValues);
                            saved = saved + _numElements;
                            if (saved > maxNumNumbers)
                            {
                                saved = 0;
                                dataset.Commit();
                            }
                        }
                        dataset.Commit();
                        dataset.PutAttr(dfsuItem, "mesh", "mesh2d");
                        dataset.PutAttr(dfsuItem, "location", "face");
                        dataset.PutAttr(dfsuItem, "coordinates", "mesh2d_face_x mesh2d_face_y");
                        dataset.PutAttr(dfsuItem, "cell_methods", "nmesh2d_face: mean");
                        dataset.PutAttr(dfsuItem, "standard_name", $"{dataItem.Quantity.ItemDescription.Trim().Replace(' ', '_')}");
                        dataset.PutAttr(dfsuItem, "long_name", $"{dataItem.Name}");
                        dataset.PutAttr(dfsuItem, "units", $"{dataItem.Quantity.UnitAbbreviation}");
                        dataset.PutAttr(dfsuItem, "grid_mapping", "");
                        sb.AppendLine($"units = {dataItem.Quantity.UnitAbbreviation}");
                        dataset.Commit();
                    }

                }
                catch (Exception e)
                {
                    sb.Append($"Error = {e.ToString()}");
                    File.WriteAllText(logFilePath, sb.ToString());
                }
            }
            sb.AppendLine($"StartTime = {writeStart} EndTime = {DateTime.Now}");
            sb.AppendLine($"Total Time = {(DateTime.Now - writeStart).TotalMinutes} minutes");
            File.WriteAllText(logFilePath, sb.ToString());
        }
        Console.WriteLine("NetCDF file created and data written successfully.");
    }
}
