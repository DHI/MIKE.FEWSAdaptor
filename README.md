# MIKE FEWS adaptor
The MIKE FEWS adaptor consist of a set of components for supporting integration of the MIKE models into the FEWS system.

The adaptor is currently designed and tested for supporting MIKE Hydro River models and the MIKE 1D engine. Integration of MIKE+ and MIKE 11 models is also possible. It can also be used as a starting point for integration of other MIKE models into FEWS.

## Disclaimer
The MIKE FEWS adaptor is provided as is. Issues and questions can be posted here on GitHub. Any support from DHI can be provided on a consultancy basis, i.e. it is not included in an SMA/subscription.

## Background
A MIKE Hydro River setup consist of several files. In the FEWS environment, the following steps 
are supported:

* Creating Time series in DFS0 format, from TS values from the FEWS system.
  These time series typically defining boundary conditions and external forcings. 
* Editing of model setup file. For the MHYDRO setup file, the following can be modified:
    * Start time
    * End time
    * Time of forecast (for Data Assimilation runs)
* Selected results (time series) computed by the engine and stored in a .res1d result file 
  can be imported to FEWS.

The following components are provided:
* Libraries in form of .NET DLL's, providing support for
  conversion between FEWS PI (xml) files and DHI files DFS0 and Res1D.
* Tools (exe files) for:
    * Conversion between DFS0 and PI files
    * Conversion from RES1D to PI file (import to FEWS)
    * Modification of MHYDRO setup file, in the form of a simple Python script
    * Generating XML file containing definition of all time series in a Res1D file
* Examples of how to use the exe files in FEWS (ModuleConfigFiles)
* Simple FEWS configuration showing how to include the MIKE 1D engine in FEWS

A setup in FEWS can be stored in two ways
* Checked out model setup. In this case the MIKE Hydro setup is stored in the directory structure
  and the adaptor will change only selected files. In this case ModuleDataSetFiles (zip files) is used,
  containing the required tools and scripts.
* Model setup in FEWS database. In this case the whole MIKE Hydro setup is included as a zip file in the ModuleDataSetFile.

The adaptor tools supports working with ensemble time series in FEWS. For ensemble handling see FEWS documentation.

## Tools included
* dfs0ToPI : Convert dfs0 to FEWS PI format
* PIToDfs0 : Convert FEWS PI format to dfs0
* Res1dToPI : Convert res1d to FEWS PI format
* GenerateAllXML : Generate XML file for all data in res1d file
* Mhydro : Modify MHYDRO file
* ModifyMhydroFile : Modify MHYDRO file
* ModifyMIKESetupFile : Modify MIKE11, MHYDRO and 1D engine (MIKE+)

## Building
Build all in Visual Studio. Then run the BuildBin.bat to create binary folders and 
BuildZip.bat to build zip files of each of the tools and place it to the FEWS config ModuleDataSetFiles folder.

## Installing
The tools are self contained and independent of MIKE software. I.e. it is not required to install any MIKE software to use the tools. It does require that the [Microsoft Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) for Visual Studio 2019 or later is installed. They are often already installed, but if not, they must be installed.

## Testing
Install FEWS. Create FEWS project dir and copy there fews.exe and content of testdata dir. Modify sa_global.properties file and set MODEL_ROOT_DIR and FEWSMIKEHYDRO_DIR to the proper dir. Run FEWS and set FEWS Current system time to 12-1-2020. Test individual workflows. 

# Tools

## Res1dTOPI
This program converts DHI Res1d file to FEWS PI format. 
A configuration xml file can be used to specify the selection of the time series to be exported. 
If this configuration xml is not provided, all time series are exported.

The FEWS ParameterId is created from ItemInfo.Quantity.Item and ItemInfo.Quantity.Unit in the Res1D file. 
The Location is created by combination of the TS location in Res1D file (river name, chainage etc.) 
and point type (Node, HPoint etc.). 

The program has the following parameters
```
InputRes1dFilePath[mandatory]        – full result (res1d) file path.
OutputPIFilePath [mandatory]         – full output (PI syntax) file path
OutputConfigurationXMLFile[optional] – XML file used for the result TS selection.
```

The Configuration xml file has the syntax:
```xml
<SelectedResults>
  <OneResult LocationId="Node;'GRONAA-TM', 1601" AtributeId="eumIWaterLevel" />
</SelectedResults>
```
A configuration xml file containing all TS identifications can be created by the tool GenerateAllXML.exe.

## GenerateAllXML
This program provides possibility to generate all TS names from DHI Res1d file and write it to xml. 
After editing (removing some lines), the configuratin xml can be used for TS selection in Res1dToPI.

The program has the following parameters
```
InputRes1dFilePath[mandatory]  – full result (res1d) file path.
OutputXmlFilePath [mandatory]  – full output file path
```

## Dfs0ToPI
This program converts DHI DFS0 file to FEWS PI format. The executable is used in ModuleConfigurationFile importDFS0.xml. 
The program has following parameters
```
OutputPIFilePath[mandatory]     – path is relative to the module work dir defined in workDir variable (ModuleConfigFile)
ModelRoot[mandatory]            – root directory of MIKE Hydro river setup
EnsembleId[mandatory]           – ensemble Id used for imported TS in PI file. If empty – no ensemble time series is created
EnsembleMemberId[mandatory]     – ensemble member Id used for imported TS in PI file.
EnsembleMemberIndex[mandatory]  – ensemble member index used for imported TS in PI file.
InputDfs0FilePath[mandatory] [InputDfs0FilePath..][optional] - path of the files to be imported. Path is relative to ModelRoot.
```
FEWS ParameterId is created from Item type (InfoInfo.Quantity.Item) and Item unit (ItemInfo.Quantity.Unit). 
FEWS Location Id is created as the Input-Dfs0-File-Path in case of a dfs0 file contain only one item. 
If the DFS0 file contain more items, Location Id is created by combination of file name and Item Name.

## PIToDfs0
This program converts FEWS PI format to DHI DFS0 file to. 
Executable is used for boundary condition modification defined ModuleConfigurationFile importDFS0.xml. 

The program has following parameters
```
InputPIFilePath [mandatory]   – path is relative to the module work dir defined in workDir variable (ModuleConfigFile)
ModelRoot[mandatory]          – root directory of MIKE Hydro river setup
OutputDfs0FilePath [optional] – path of the file to be exported. 
ParameterType[optional]       – string containing DFS DataValueType (Instantaneous, Accumulated, StepAccumulated, MeanStepBackward, MeanStepForward). Default is Instantaneous.
```

The OutputDfs0FilePath is relative to ModelRoot. If empty, the file name is taken from FEWS LocationId. 
LocationId is parsed using the ```|``` character, first element is used as file name and second as new location. 
Location is after that used as DFS0 item name. Item Quantity (Item ad units) are created by parsing of the FEWS PrameterId. 

The DFS DataValueType has the values Instantaneous, Accumulated, StepAccumulated, MeanStepBackward, MeanStepForward. Default is Instantaneous.


#	Module configuration files
A set of module configuration files is provided as a template for the FEWS configuration.

The Module configuration files (xml) uses variables. 
Variable values are defined in FEWS workflows, using this module. 
Variables are typically used as exe file parameters. 
Some FEWS global parameters are also used – for example %REGION_HOME%. 
Variable names are typically self explaining. 
For an introduction on how to create ModuleSetupFile and how to use variables, see FEWS documentation.

##	PI2DFS0.xml
Used for export from FEWS database to DFS0 file. Variables
```
$map_name$            - name of mapId file
$parameterId$         - FEWS parameter Id
$locationsetId$       - used Location setId
$timeSeriesType$      - FEWS timeSeriesType (external historical….)
$mike_model_dir$      - MIKE Hydro river setup root directory.
$dfs0_relative_path$  - dfs0 path relative to MIKE Hydro river setup root directory
$module_instance$     - module instance used in exportTimeSeriesActivity
```

## importDFS0.xml
Used for import DFS0 to FEWS database. Variables
```
$mike_model_dir$      - MIKE Hydro river setup root directory.
$dfs0_relative_path$  - dfs0 path relative to MIKE Hydro river setup root directory
$timeSeriesType$      - FEWS timeSeriesType (external historical….)
$locationId$          - used LocationId used for imported TS
$parameterId$         - FEWS parameter Id
$pi_name$             - name of FEWS import file
```

## Res1d2PI
Import of TS from Res1D to FEWS database. Variables
```
$res1d_file$          - path to res1d file
$pi-output$           - name of FEWS output file.
$selection_file$      - parth to TS selection file
```

## modify_and_run.xml
Modify mhydro file and execute computation. Variables
```
$mike_exe$            - full path to DHI.Mhydro.Application.exe
$mike_model_dir$      - MIKE Hydro river setup root directory.
$relative_setup_path$ - path to the mhydro file relative to the MIKE Hydro river setup root directory.
```

## modify_and_runM11FF.xml
Modify mike11 setup files and execute computation. Variables
```
$mike_exe$            - full path to mike11 executeble
$mike_model_dir$      - MIKE11 setup root directory.
$relative_setup_path$ - path to the sim11 file relative to the MIKE11 setup root directory.
$relative_hotstart_path$ - path to the hotstart file relative to the MIKE11 setup root directory.
$relative_FF_path$ path to the ff (flood forecasting) file relative to the MIKE11 setup root directory.
$relative_DA_path$ path to the da (data asimilation) file relative to the MIKE11 setup root directory.
```

# Map files
Map file defining mapping between MIKE and FEWS parameters and locations. For example 
```xml
<?xml version="1.0" encoding="UTF-8"?>
<idMap version="1.1" xmlns="http://www.wldelft.nl/fews" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.wldelft.nl/fews http://fews.wldelft.nl/schemas/version1.0/idMap.xsd">
	<parameter internal="H.obs" external="eumIWaterLevel;eumUmeter"/>
	<location external="tide.dfs0" internal="Napa Tide boundary"/>
</idMap>
```

