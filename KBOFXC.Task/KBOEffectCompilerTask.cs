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

namespace KBOFXC.Task
{
    public class KBOEffectCompilerTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[]? EffectFiles { get; set; }

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
                    Environment.CurrentDirectory = Path.GetDirectoryName(fullFxFilePath)!;
                    var result = EffectCompiler.CompileShaderFromFile(fullFxFilePath, DefaultEffectFlags, true);

                    string outputFile = item.GetMetadata("OutputFilePath");
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
                                BuildEngine.LogWarningEvent(new BuildWarningEventArgs("EHFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", ""));
                            }
                            else
                            {
                                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("EHFXC", diag.code, diagnosticFilePath, diag.row, diag.column, diag.rowEnd, diag.columnEnd, diag.message, "", ""));
                                anyErrorDiagnostics = true;
                            }
                        }
                        const int E_FAIL = unchecked((int)0x80004005);
                        if (result.hresult == E_FAIL)
                        {
                            if (!anyErrorDiagnostics)
                                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("EHFXC", "EHFXC04", fullFxFilePath, 0, 0, 0, 0, "Compilation failed", "", ""));
                            anyErrors = true;
                            continue;
                        }
                        if (result.effectData == null)
                        {
                            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("EHFXC", "EHFXC05", fullFxFilePath, 0, 0, 0, 0, "Compilation did not error but did not produce an effect", "", ""));
                            anyErrors = true;
                        }
                        else
                        {
                            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("EHFXC", "EHFXC06", fullFxFilePath, 0, 0, 0, 0, $"Writing output to {outputFile}", "", "", MessageImportance.Low));
                            File.WriteAllBytes(outputFile, result.effectData);
                        }
                        Marshal.ThrowExceptionForHR(result.hresult);
                    }
                    catch (Exception e)
                    {
                        BuildEngine.LogErrorEvent(new BuildErrorEventArgs("EHFXC", "EHFXC07", fullFxFilePath, 0, 0, 0, 0, $"An unexpected exception occured: {e}", "", ""));
                        anyErrors = true;
                        continue;
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = startDirectory;
                }

            }
            return !anyErrors;
        }

    }
}
