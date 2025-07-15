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

        public string[]? AdditionalSearchPaths { get; set; }

        public bool IgnoreErrors { get; set; }

        public override bool Execute()
        {
            AdditionalSearchPaths ??= Array.Empty<string>();
            //Debugger.Launch();
            bool anyErrorDiagnostics = false;
            bool anyErrors = false;
            foreach (var item in EffectFiles ?? Array.Empty<ITaskItem>())
            {
                string relativeFxFilePath = item.ItemSpec;
                string fullFxFilePath = Path.GetFullPath(relativeFxFilePath);
                string outputFile = item.GetMetadata("OutputPath");
                if (string.IsNullOrWhiteSpace(outputFile))
                    outputFile = Path.ChangeExtension(fullFxFilePath, ".fxc");

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
                        if (!string.IsNullOrEmpty(value))
                            WarnMetadataInvalidValue(metadataName, value);
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

                HResult result = EffectCompiler.CompileEffect(fullFxFilePath, null, AdditionalSearchPaths, compilerFlags, out byte[] effectCode, out string? errors);

                var diagnostics = CompileDiagnostic.GetDiagnostics(errors);

                foreach (var diag in diagnostics)
                {
                    string? diagnosticFilePath = diag.file ?? fullFxFilePath;
                    if (string.IsNullOrWhiteSpace(diagnosticFilePath))
                        diagnosticFilePath = fullFxFilePath;
                    if (diag.isWarning == true)
                    {
                        BuildEngine.LogWarningEvent(new BuildWarningEventArgs("KBOFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", ""));
                    }
                    else
                    {
                        LogErrorIfErrorsEnabled("KBOFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", "");
                        anyErrorDiagnostics = true;
                    }
                }


                if (result.code == HResult.E_FAIL)
                {
                    if (!anyErrorDiagnostics)
                        LogErrorIfErrorsEnabled("KBOFXC", "KBOFXC04", fullFxFilePath, 0, 0, 0, 0, "Compilation failed", "", "");
                    anyErrors = true;
                    continue;
                }
                if (effectCode == null || effectCode.Length == 0)
                {
                    LogErrorIfErrorsEnabled("KBOFXC", "KBOFXC05", fullFxFilePath, 0, 0, 0, 0, "Compilation did not error but did not produce an effect", "", "");
                    anyErrors = true;
                }
                else
                {
                    BuildEngine.LogMessageEvent(new BuildMessageEventArgs("KBOFXC", "", fullFxFilePath, 0, 0, 0, 0, $"Writing output to {outputFile}", "", "", MessageImportance.Low));
                    File.WriteAllBytes(outputFile, effectCode);
                }
            }

            return !(anyErrorDiagnostics || anyErrors) || IgnoreErrors;
        }
        void LogErrorIfErrorsEnabled(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName)
        {
            if (!IgnoreErrors)
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName));
            else
                BuildEngine.LogWarningEvent(new BuildWarningEventArgs(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName));
        }
    }
}
