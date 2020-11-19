IF NOT EXIST bin MKDIR bin

if NOT EXIST bin\dfs0ToPI         MKDIR bin\dfs0ToPI
if NOT EXIST bin\GenerateAllXML   MKDIR bin\GenerateAllXML
if NOT EXIST bin\Mhydro           MKDIR bin\Mhydro
if NOT EXIST bin\ModifyMhydroFile MKDIR bin\ModifyMhydroFile 
if NOT EXIST bin\PIToDfs0         MKDIR bin\PIToDfs0
if NOT EXIST bin\Res1dToPI        MKDIR bin\Res1dToPI

del /Q bin\dfs0ToPI\*.*
del /Q bin\GenerateAllXML\*.*
del /Q bin\Mhydro\*.*
del /Q bin\ModifyMhydroFile\*.*
del /Q bin\PIToDfs0\*.*
del /Q bin\Res1dToPI\*.*

copy src\Dfs0ToPI\bin\Release\*.*           bin\dfs0ToPI\
copy src\GenerateAllXML\bin\Release\*.*     bin\GenerateAllXML\
copy src\ModifyMhydroFile\bin\Release\*.*   bin\ModifyMhydroFile\
copy src\PIToDfs0\bin\Release\*.*           bin\PIToDfs0\
copy src\Res1dToPI\bin\Release\*.*          bin\Res1dToPI\

del /Q bin\Dfs0ToPI\*.pdb
del /Q bin\GenerateAllXML\*.pdb
del /Q bin\ModifyMhydroFile\*.pdb
del /Q bin\PIToDfs0\*.pdb
del /Q bin\Res1dToPI\*.pdb
