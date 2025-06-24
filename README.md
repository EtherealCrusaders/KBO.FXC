# KBO.FXC

## What is this? 

It's an effects compiler, similar to fxcompiler, fxcompiler_reach or easyxnb.

It compiles effects (`.fx`) files into `.fxc`, using the shader model `fx_2_0` (which includes `vs_3_0` and `ps_3_0`)

## Why?
All the compilers mentioned above cannot `#include` files, and sometimes using functions like atan2 causes an unknown error.

[fxc](https://learn.microsoft.com/en-us/windows/win32/direct3dtools/fxc) solved these issues, but if a file or header contains an [UTF8 BOM](https://en.wikipedia.org/wiki/Byte_order_mark) it throws an `error X3000: Illegal character in shader file` error.

So this tool aims to fix all the issues above and to compile shaders with Shader Model 3.

## How do I use this? 
There are 2 ways to use this compiler:
- Through the MSBuild task (preferred approach)
- Through the CLI tool 

But you can have both in your mod just in case.

### CLI Tool
This tool is an executable, it will compile all shaders in the current directory and it's subdirectories when run. <br/>
The command line args are:
```
--file/-f (file) : Specifies the single file to compile.
--no-recursion : inspect only the current directory for .fx files
--no-wait-exit : Closes immediately after compilation succeeds or fails, if not specified (default) it waits for a key press before closing.
```

### Setting up the msbuild task
1. Download the KBOFXCTask.zip from Releases.
2. Extract into any directory in your mod, maybe in an Assets/Effects/Compiler folder.
3. Add to your mod's .csproj:
	```xml
	<Import Project=".\Assets\Effects\Compiler\KBOFXCTask.props" />
	<Import Project=".\Assets\Effects\Compiler\KBOFXCTask.targets" />
	``` 
	(Where `\Assets\Effects\Compiler\` is the directory you extracted the zip) 

That's all, when you build the mod it should compile all `.fx` files and produce an `.fxc` file next to them.

However if you used to compile effects to xnbs, you will get an error while the mod is loading. <br/>
To fix this, you could simply delete the xnbs **after confirming you get an `.fxc` binary with identical behaviour and parameters**. <br/>
Or you could blacklist `.fx` files by adding to your .csproj:
```
<ItemGroup>
	<KBOEffects Remove="path/to/file.fx"/>
	<KBOEffects Remove="path/to/folder/*.fx"/> <!-- you can also blacklist an entire folder -->
	<KBOEffects Remove="path/to/folder/**/*.fx"/> <!-- or a folder and all of its subfolders -->
</ItemGroup>
```

You can also whitelist `.fx` files by doing `Include` instead of `Remove` (e.g. `<KBOEffects Include="path/to/file.fx"/>`)

If you want to configure a specific file that was already added (e.g. to disable optimizations), you can do `Update` `<KBOEffects Update="path/to/file.fx" Metadata="NewValue"/>`

For more information, see [MSBuildTask.md](./MSBuildTask.md).


### Project structure
- `KBO.FXC/` the D3DCompiler bindings as a ns2.0/net8.0 library.
- `KBO.FXC.Tests/` tests for making sure the library works.
- `KBOFXC/`: the command line executable similar to fxcompiler.
- `KBOFXCTask/`: the source for the MSBuild task.
