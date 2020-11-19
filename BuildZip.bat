set zipDir=testdata\config\ModuleDataSetFiles
IF NOT EXIST %zipDir% MKDIR %zipDir%
del /Q %zipDir%\*.*

zip -j "%zipDir%\importDfs0 Default.zip"     bin\dfs0ToPI\*.*
zip -j "%zipDir%\modify_and_run Default.zip" bin\ModifyMhydroFile\*.*
zip -j "%zipDir%\modify_py Default.zip"      bin\MHydro\*.*
zip -j "%zipDir%\PI2Dfs0 Default.zip"        bin\PITodfs0\*.*
zip -j "%zipDir%\Res1d2PI Default.zip"       bin\Res1DToPI\*.*
