# Mono.Cecil.Inject

### [Download the latest version](https://github.com/denikson/Mono.Cecil.Inject/releases)
### [Documentation](http://denikson.github.io/Mono.Cecil.Inject/)

## Overview
Mono.Cecil.Inject (Cecil.Inject from now on) is a simple library that adds a few convenience methods to the original Mono.Cecil library.
Added functionality includes method injection, System.Reflection-style methods to search other methods and class members by name and parameters and fast accessibility modification.

The library is almost a seamless addition to Mono.Cecil that should've been in the original library from the beginnig.
The library is compact, easy-to-use and well documented.

## Documentation
Online documentation (along with a library overview) can be found [here](http://denikson.github.io/Mono.Cecil.Inject/).
If you have used ReiPatcherPlus before, you can grab the migration guide from `Migration` directory.

## Building from source
To build from source, you will need the latest version of MSBuild that is capable of compiling C# 6 source code and Mono.Cecil.
Simply place `Mono.Cecil.dll` into `lib` folder and run `build.bat` or use MSBuild.

