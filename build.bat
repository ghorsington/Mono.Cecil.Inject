@echo off
rem Basic build script for Cecil.Inject
rem Requires MSBuild with C# 6 compatible compiler
rem Place Mono.Cecil.dll to the "lib" folder in this directory
rem You may specify the build configuration as an argument to this batch
rem If no arguments are specified, will build the Release version

echo Building Mono.Cecil.Inject

set msbuildpath=%ProgramFiles%\MSBuild\14.0\Bin
set cecilpath=%cd%\lib
set projectpath=%cd%\Mono.Cecil.Inject
set buildconf=Release

if not -%1-==-- (
	echo Using %1 as building configuration
	set buildconf=%1
)
if -%1-==-- (
	echo No custom build configuration specified. Using Release
)

if not exist %msbuildpath%\msbuild.exe (
	set msbuildpath=%ProgramFiles(x86)%\MSBuild\14.0\Bin
)

if not exist "%msbuildpath%\msbuild.exe" (
	echo Failed to locate MSBuild.exe
	exit /b 1
)

if not exist %cecilpath%\Mono.Cecil.dll (
	echo Failed to locate Mono.Cecil.dll from %cecilpath%
	exit /b 1
)

"%msbuildpath%\msbuild.exe" %projectpath%\Mono.Cecil.Inject.csproj /p:Configuration=%buildconf%