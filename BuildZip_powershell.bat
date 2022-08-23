set zipDir=testdata\config\ModuleDataSetFiles
IF NOT EXIST %zipDir% MKDIR %zipDir%
del /Q %zipDir%\*.*

powershell.exe Compress-Archive bin\dfs0ToPI\*.* '%zipDir%\importDfs0 Default.zip'     
powershell.exe Compress-Archive bin\ModifyMhydroFile\*.* '%zipDir%\modify_and_run Default.zip'
powershell.exe Compress-Archive bin\MHydro\*.* '%zipDir%\modify_py Default.zip'
powershell.exe Compress-Archive bin\PITodfs0\*.* '%zipDir%\PI2Dfs0 Default.zip'
powershell.exe Compress-Archive bin\Res1DToPI\*.* '%zipDir%\Res1d2PI Default.zip' 
powershell.exe Compress-Archive bin\ModifyMIKESetupFile\*.* '%zipDir%\modify_and_runM11FF Default.zip' 
