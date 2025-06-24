using D3DBindings;
using KBO.FXC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace KBOFXC
{
    public static partial class Program
    {
        internal static CompilerFlags DefaultEffectFlags = CompilerFlags.None;

        public static int Main(string[] args)
        {
            bool recursive = true;
            bool waitForExitConfirmation = true;
            string targetFile = "";
            CompilerFlags flags = CompilerFlags.None;
            List<string> includePaths = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--file", StringComparison.Ordinal) || args[i].Equals("-f", StringComparison.Ordinal))
                {
                    if (args.Length < i + 1)
                        Console.Error.WriteLine($"Missing file path for argument '{args[i]}'");
                    else
                        targetFile = args[i + 1];
                }
                if (args[i].Equals("--include", StringComparison.Ordinal) || args[i].Equals("-I", StringComparison.Ordinal))
                {
                    if (args.Length < i + 1)
                        Console.Error.WriteLine($"Missing file path for argument '-{args[i]}'");
                    else
                        targetFile = args[i + 1];
                }
                else if (args[i].Equals("--no-recursion", StringComparison.Ordinal))
                {
                    recursive = false;
                }
                else if (args[i].Equals("--no-wait-exit", StringComparison.Ordinal))
                {
                    waitForExitConfirmation = false;
                }
            }
            string oldCurrentDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            bool anyErrors = false;
            bool anyFiles = false;
            bool errorSpecifiedFileMissing = false;
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.fx", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    anyFiles = true;
                    anyErrors |= !Compile(file);
                }
            }
            else
            {
                if (File.Exists(targetFile))
                {
                    anyErrors = Compile(targetFile);
                }
                else
                {
                    errorSpecifiedFileMissing = true;
                    anyErrors = true;
                }
            }
            bool Compile(string file)
            {
                Console.WriteLine($"Compiling: {file.Substring(oldCurrentDir.Length)}");
                try
                {

                    HResult hresult = EffectCompiler.CompileEffect(file, null, null, DefaultEffectFlags, out byte[] effectCode, out string? diagnostics);
                    //HResult hresult = D3DCompiler.D3DCompile(new ArraySegment<byte>(File.ReadAllBytes(file)), file, new D3DShaderMacros(new Dictionary<string, string?>()
                    //{
                    //    ["KBOFXC"] = "1"
                    //}), new ID3DIncludeNewHandler(file), null, "fx_2_0", flags, 0, out byte[] shaderCode, out string? errors);
                    //var result = EffectCompiler.CompileShaderFromFile(file, DefaultEffectFlags, false, includePaths.ToArray());

                    if (!string.IsNullOrWhiteSpace(diagnostics))
                        Console.WriteLine(diagnostics);
                    if (hresult.code != 0)
                    {
                        return false;
                    }
                    if (effectCode != null && effectCode.Length != 0)
                    {
                        string outputFile = Path.ChangeExtension(file, ".fxc");
                        Console.WriteLine($"Output: {outputFile}");
                        File.WriteAllBytes(outputFile, effectCode!);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: compilation succeeded but no effect binary was produced");
                    }

                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected exception: {ex}");
                    return false;
                }
                return true;
            }
            if (anyErrors)
            {
                if (errorSpecifiedFileMissing)
                    Console.WriteLine("Compilation failed for one or more files");
                else
                    Console.WriteLine($"Specified file: {targetFile} does not exist or is not accessible.");
            }
            else
            {
                if (anyFiles)
                    Console.WriteLine("Compilation successful for all files");
                else
                    Console.WriteLine("No fx files were found in the current or children directories.");
            }
            if (waitForExitConfirmation)
            {
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
            if (anyErrors)
                return HResult.E_FAIL;
            return 0;
        }

    }

}
