
## MSBuild Properties 
- `KBOEffectsIgnoreErrors`: Allows the mod to compile even if there are errors in the shader files. (`true` or `false` (default)) 


For more information on how #include works, see 

## MSBuild Items

### KBOEffects metadata
You can configure compilation for effect files, example:
```
<ItemGroup>
	<KBOEffects Include="myshader.fx" OptimizationLevel="3" PackMatrix="ColumnMajor"/> <!-- This adds a file and it will be compiled with O3 optimization level -->
	<KBOEffects Update="Effects/ScreenShaders/*.fx" OptimizationLevel="0"/> <!-- This makes all files in Effects/ScreenShaders be compiled without optimizations -->
</ItemGroup>
```
- `OptimizationLevel`: Valid values are `0`, `1` (default), `2` or `3`. 3 Being most optimized and 0 being no optimization.
- `PackMatrix`: `RowMajor` (default, as is `Matrix` in FNA/XNA) or `ColumnMajor`.
- `NoPreshader`: `true` or `false` (default).
- `WarningsAreErrors`: `true` or `false` (default), treats warnings in the effect as errors.
- `SkipOptimization`: `true` or `false` (default).
- `OutputPath`: a string where to place the compiled `.fxc` binary. An empty or whitespace string will place the binary next to the source (the .fx file).
- `Debug`: `true` or `false` (default).

