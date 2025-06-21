# KBO.FXC

## What is this? 

It's an effects compiler, similar to fxcompiler, fxcompiler_reach or easyxnb.

It compiles effects (`.fx`) files into `.fxc`, using the shader model `fx_2_0` (which includes `vs_3_0` and `ps_3_0`)

## Why?
All the compilers mentioned above cannot `#include` files, and sometimes using functions like atan2 causes an unknown error.

[fxc](https://learn.microsoft.com/en-us/windows/win32/direct3dtools/fxc) solved these issues, but if a file or header contains an [UTF8 BOM](https://en.wikipedia.org/wiki/Byte_order_mark) it throws an `error X3000: Illegal character in shader file` error.

So this tool aims to fix all the issues above and to compile shaders with Shader Model 3.

## Project structure
- `KBO.FXC/` contains the D3DCompiler bindings as a .net standard library.
- `KBOFXC/` is a command line executable similar to fxcompiler. <br/>
  - It compiles all .fx files in the current directory **and subdirectories** into .fxc.
- `KBOFXC.Task` is an MSBuild task you can integrate into your mod for automatically compiling shaders when you compile your mod (from anywhere EXCEPT in-game).

