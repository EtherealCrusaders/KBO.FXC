using D3DBindings;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using KBO.FXC;

namespace KBOFXCTask
{
    public class KBOEffectCompilerTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[]? EffectFiles { get; set; }

        public string[] AdditionalSearchPaths { get; set; } = Array.Empty<string>();

        public bool IgnoreErrors { get; set; }

        private static CompilerFlags DefaultEffectFlags = CompilerFlags.None;

        public override bool Execute()
        {
            EffectFiles ??= Array.Empty<ITaskItem>();
            //Debugger.Launch();

            bool anyErrors = false;
            string startDirectory = Environment.CurrentDirectory;
            foreach (var item in EffectFiles)
            {
                try
                {
                    string relativeFxFilePath = item.ItemSpec;
                    string fullFxFilePath = Path.GetFullPath(relativeFxFilePath);

                    void WarnMetadataInvalidValue(string metadataName, string value)
                    {
                        BuildEngine.LogWarningEvent(new BuildWarningEventArgs("KBOFXC", "KBOFXC03", fullFxFilePath, 0, 0, 0, 0, $"Invalid value '{value}' for metadata '{metadataName}'", "", ""));
                    }
                    bool GetBoolMetadata(string metadataName)
                    {
                        string value = item.GetMetadata(metadataName);
                        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (!value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            WarnMetadataInvalidValue(metadataName, value);
                        }
                        return false;
                    }

                    CompilerFlags compilerFlags = CompilerFlags.None;
                    if (GetBoolMetadata("NoPreshader")) compilerFlags |= CompilerFlags.NoPreshader;
                    if (GetBoolMetadata("WarningsAreErrors")) compilerFlags |= CompilerFlags.WarningsAreErrors;
                    if (GetBoolMetadata("SkipOptimization")) compilerFlags |= CompilerFlags.SkipOptimization;
                    if (GetBoolMetadata("Debug")) compilerFlags |= CompilerFlags.Debug;
                    string metadataOptimizationLevel = item.GetMetadata("OptimizationLevel");
                    switch (metadataOptimizationLevel)
                    {
                        case "0": compilerFlags |= CompilerFlags.OptimizationLevel0; break;
                        case "1": compilerFlags |= CompilerFlags.OptimizationLevel1; break;
                        case "2": compilerFlags |= CompilerFlags.OptimizationLevel2; break;
                        case "3": compilerFlags |= CompilerFlags.OptimizationLevel3; break;
                        default:
                            if (!string.IsNullOrEmpty(metadataOptimizationLevel))
                                WarnMetadataInvalidValue("OptimizationLevel", metadataOptimizationLevel);
                            break;
                    }
                    string metadataPackMatrix = item.GetMetadata("PackMatrix");
                    if (metadataPackMatrix.Equals("rowmajor", StringComparison.OrdinalIgnoreCase)
                        || metadataPackMatrix.Equals("row_major", StringComparison.OrdinalIgnoreCase))
                    {
                        compilerFlags |= CompilerFlags.PackMatrixRowMajor;
                    }
                    else if (metadataPackMatrix.Equals("columnmajor", StringComparison.OrdinalIgnoreCase)
                        || metadataPackMatrix.Equals("column_major", StringComparison.OrdinalIgnoreCase))
                    {
                        compilerFlags |= CompilerFlags.PackMatrixColumnMajor;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(metadataPackMatrix))
                            WarnMetadataInvalidValue("OptimizationLevel", metadataOptimizationLevel);
                    }

                    CompileResult result = EffectCompiler.CompileShaderFromFile(fullFxFilePath, compilerFlags, true, AdditionalSearchPaths);

                    string outputFile = item.GetMetadata("OutputPath");
                    if (string.IsNullOrWhiteSpace(outputFile))
                        outputFile = Path.ChangeExtension(fullFxFilePath, ".fxc");

                    try
                    {
                        bool anyErrorDiagnostics = result.anyErrors;
                        foreach (var diag in result.diagnostics)
                        {
                            string? diagnosticFilePath = diag.file;
                            if (string.IsNullOrWhiteSpace(diagnosticFilePath))
                                diagnosticFilePath = fullFxFilePath;
                            if (diag.isWarning)
                            {
                                BuildEngine.LogWarningEvent(new BuildWarningEventArgs("KBOFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", ""));
                            }
                            else
                            {
                                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("KBOFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", ""));
                                anyErrorDiagnostics = true;
                            }
                        }
                        const int E_FAIL = unchecked((int)0x80004005);
                        if (result.hresult == E_FAIL)
                        {
                            if (!anyErrorDiagnostics)
                                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("KBOFXC", "KBOFXC04", fullFxFilePath, 0, 0, 0, 0, "Compilation failed", "", ""));
                            anyErrors = true;
                            continue;
                        }
                        if (result.effectData == null)
                        {
                            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("KBOFXC", "KBOFXC05", fullFxFilePath, 0, 0, 0, 0, "Compilation did not error but did not produce an effect", "", ""));
                            anyErrors = true;
                        }
                        else
                        {
                            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("KBOFXC", "", fullFxFilePath, 0, 0, 0, 0, $"Writing output to {outputFile}", "", "", MessageImportance.Low));
                            File.WriteAllBytes(outputFile, result.effectData);
                        }
                        Marshal.ThrowExceptionForHR(result.hresult);
                    }
                    catch (Exception e)
                    {
                        BuildEngine.LogErrorEvent(new BuildErrorEventArgs("KBOFXC", "KBOFXC06", fullFxFilePath, 0, 0, 0, 0, $"An unexpected exception occured: {e}", "", ""));
                        anyErrors = true;
                        continue;
                    }
                }
                finally
                {
                    //Environment.CurrentDirectory = startDirectory;
                }

            }
            return !anyErrors;
        }

    }
}
