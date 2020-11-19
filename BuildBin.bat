IF NOT EXIST bin MKDIR bin

if NOT EXIST bin\dfs0ToPI         MKDIR bin\dfs0ToPI
if NOT EXIST bin\GenerateAllXML   MKDIR bin\GenerateAllXML
if NOT EXIST bin\MHydro           MKDIR bin\MHydro
if NOT EXIST bin\ModifyMHydroFile MKDIR bin\ModifyMHydroFile 
if NOT EXIST bin\PIToDfs0         MKDIR bin\PIToDfs0
if NOT EXIST bin\Res1DToPI        MKDIR bin\Res1DToPI

del /Q bin\dfs0ToPI\*.*
del /Q bin\GenerateAllXML\*.*
del /Q bin\MHydro\*.*
del /Q bin\ModifyMHydroFile\*.*
del /Q bin\PIToDfs0\*.*
del /Q bin\Res1DToPI\*.*

copy src\Dfs0ToPI\bin\Release\*.*           bin\dfs0ToPI\
copy src\GenerateAllXML\bin\Release\*.*     bin\GenerateAllXML\
copy src\ModifyMHydroFile\bin\Release\*.*   bin\ModifyMHydroFile\
copy src\PIToDfs0\bin\Release\*.*           bin\PIToDfs0\
copy src\Res1DToPI\bin\Release\*.*          bin\Res1DToPI\

del /Q bin\Dfs0ToPI\*.pdb
del /Q bin\GenerateAllXML\*.pdb
del /Q bin\ModifyMHydroFile\*.pdb
del /Q bin\PIToDfs0\*.pdb
del /Q bin\Res1DToPI\*.pdb

copy src\MHydro\*.py                        bin\MHydro\