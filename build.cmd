@ECHO OFF

if not exist bin mkdir bin

:: Note: We're not using parallel build (/m) because it doesn't seem to
::       work with Sandcastle Help File Builder.

"%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe" /nologo /m /v:m /nr:false /flp:verbosity=normal;LogFile=bin\msbuild.log %*
