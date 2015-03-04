
ECHO ON

SET PackageName=%~1
SET ProjectDir=%~2
SET SolutionDir=%~3
SET SolutionName=%~4
SET TargetDir=%~5
SET TargetPath=%~6

REM -- Clean
@DEL "%ProjectDir%..\installer\*.*" /f /s /q
@RMDIR "%ProjectDir%..\installer\" /s /q

REM -- Binaries
@XCOPY "%TargetPath%" "%ProjectDir%..\installer\%PackageName%\" /y /i /q

REM -- Package
@XCOPY "%ProjectDir%Package\package.xml" "%ProjectDir%..\installer\%PackageName%\" /y /i /q

REM -- Installer
@XCOPY "%ProjectDir%App_Plugins\Installer\Installer.ascx" "%ProjectDir%..\installer\%PackageName%\App_Plugins\Installer\" /y /i /q /s
@XCOPY "%TargetDir%App_Code\Installer\Setup.ascx" "%ProjectDir%..\installer\%PackageName%\App_Plugins\Installer\" /y /i /q /s

REM -- App_Plugins
REM @XCOPY "%ProjectDir%App_Plugins\*.*" "%ProjectDir%..\installer\%PackageName%\App_Plugins\" /y /i /q /s
@XCOPY "%ProjectDir%App_Plugins\package.manifest" "%ProjectDir%..\installer\%PackageName%\" /y /i /q /s

REM -- Create package's zip file
"C:\Program Files\7-Zip\7z.exe" a -tzip "%ProjectDir%..\installer\%PackageName%.zip" "%ProjectDir%..\installer\%PackageName%\*.*" -r

REM -- If the project is part of solution with an Umbraco website then copy files directly into destination website to avoid installing the package
SET UmbracoSandbox=%SolutionDir%%SolutionName%
REM -- Check whether the directory exists
IF NOT EXIST "%UmbracoSandbox%" GOTO NOUMBRACOSANDBOX
REM -- Check whether the directory is not the current solution
IF "%UmbracoSandbox%\"=="%ProjectDir%" GOTO NOUMBRACOSANDBOX
	@XCOPY "%TargetPath%" "%UmbracoSandbox%\bin\" /y /i /q
	@XCOPY "%ProjectDir%App_Plugins\package.manifest" "%UmbracoSandbox%\App_Plugins\%PackageName%\" /y /i /q /s
	@XCOPY "%ProjectDir%App_Plugins\*.*" "%UmbracoSandbox%\App_Plugins\%PackageName%\" /y /i /q /s
:NOUMBRACOSANDBOX