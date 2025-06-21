using D3DBindings;
using KBO.FXC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace KBO.FXC
{
    public static partial class Program
    {
        // allow for it to be collected in case of assembly unload
        private static readonly LPCSTRObj StrProfile_fx_2_0 = new("fx_2_0");
        //private static readonly LPCSTRObj StrDefineEHFXC = new("EHFXC");
        private static readonly DefineMacros EHFXCMacros = new(("EHFXC", "1"));

        //private static readonly RelativePathInclude ehfxcinclude = new RelativePathInclude();

        private const string DiagnosticFormatRegexStr = @"\s*(?<filepath>(\w:[\w_\\\/. ]+)\((?<row>\d+)(\-(?<rowend>\d+))?\,(?<column>\d+)(\-(?<columnend>\d+))?\)\:\s*)?(?<kind>warning|error)\s*(?<code>X?\d+)\s*\:\s*(?<diagmsg>[\w\d\ :',.]*)";
        private const RegexOptions DiagnosticFormatRegexOptions = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex DiagnosticFormatRegex =
#if !NET8_0_OR_GREATER
            new Regex(DiagnosticFormatRegexStr, DiagnosticFormatRegexOptions);
#else
            DiagnosticFormatRegexF();
        [GeneratedRegex(DiagnosticFormatRegexStr, DiagnosticFormatRegexOptions)]
        private static partial Regex DiagnosticFormatRegexF();
#endif

        internal static CompilerFlags DefaultEffectFlags = CompilerFlags.None;
        // problem:
        // #include does not work properly when the current directory is not the directory of the file 
        // meaning files can only be compiled in parallel after grouping them by directory
        // but compilation is usually very fast even doing them sequentially should be fine
        // update: 

        public static int Main(string[] args)
        {
            bool recursive = false;
            string targetFile = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--file") || args[i].Equals("-f"))
                {
                    if (args.Length < i + 1)
                        Console.Error.WriteLine("Missing file path for argument '-f'");
                    else
                        targetFile = args[i + 1];
                }
                if (args[i].Equals("--recursive", StringComparison.Ordinal))
                {
                    recursive = true;
                }
            }
            string oldCurrentDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            bool anyErrors = false;
            if (targetFile == "")
            {
                foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.fx", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    anyErrors |= !Compile(file);
                }
            }
            else
            {
                anyErrors = Compile(targetFile);
            }
            bool Compile(string file)
            {
                Console.WriteLine($"Compiling: {file.Substring(oldCurrentDir.Length)}");
                try
                {
                    //string folder = Environment.CurrentDirectory = Path.GetDirectoryName(file)!;
                    var result = CompileShaderFromFile(file, DefaultEffectFlags, false);
                    Environment.CurrentDirectory = oldCurrentDir;

                    Console.WriteLine(result.diagnosticsStr);
                    if (result.hresult != 0)
                    {
                        return false;
                    }
                    if (result.effectData != null)
                    {
                        File.WriteAllBytes(Path.ChangeExtension(file, ".fxc"), result.effectData!);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: compilation succeeded but no effect binary was produced");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected exception: {ex}");
                    return false;
                }
                return true;
            }
            if (anyErrors)
                return D3DCompiler.E_FAIL;
            return 0;
        }

        private struct ID3DIncludeExContext
        {
            public Stack<string> includeDirectories;
            public string rootFile;
            public ID3DIncludeExContext(string initialDirectory)
            {
                includeDirectories = new Stack<string>();
                includeDirectories.Push(Path.GetDirectoryName(rootFile = initialDirectory)!);
            }
        }
        private unsafe ref struct ID3DIncludeEx
        {
            public ID3DInclude.Vtbl* lpVtbl;
            public void* pID3DIncludeExContext;
        }

        private const int S_OK = 0;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ID3DIncludeOpen(ID3DInclude* self, D3D_INCLUDE_TYPE includeType, LPCSTR fileName, void* parentData, void** data, uint* bytes);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ID3DIncludeClose(ID3DInclude* self, void* pData);

        private static unsafe readonly ID3DIncludeOpen OpenDelegate = Open;
        private static unsafe readonly ID3DIncludeClose CloseDelegate = Close;
        private static unsafe readonly delegate* unmanaged[Stdcall]<ID3DInclude*, D3D_INCLUDE_TYPE, LPCSTR, void*, void**, uint*, int> POpen = (delegate* unmanaged[Stdcall]<ID3DInclude*, D3D_INCLUDE_TYPE, LPCSTR, void*, void**, uint*, int>)Marshal.GetFunctionPointerForDelegate(OpenDelegate);
        private static unsafe readonly delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int> PClose = (delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int>)Marshal.GetFunctionPointerForDelegate(CloseDelegate);

        // assumes a stack-ish behaviour for Open and Close
        // <source file>
        // +- #include "header.h"
        //    +- Open("header.h")
        //    \- Close("header.h")
        // +- #include "otherheader.h"     
        //      +- Open("otherheader.h")
        //      +- #include "anotherheader.h"
        //      |  +- Open("header.h")
        //      |  \- Close("header.h")
        //      \- Close("otherheader.h")
        // 
        //     
        private static unsafe int Open(ID3DInclude* _self, D3D_INCLUDE_TYPE includeType, LPCSTR fileName, void* parentData, void** data, uint* bytes)
        {
            ref ID3DIncludeExContext context = ref Unsafe.AsRef<ID3DIncludeExContext>((*(ID3DIncludeEx*)_self).pID3DIncludeExContext);
            try
            {
                string filePath = fileName.ToString();
                string newIncludeDirectory;
                if (!Path.IsPathRooted(filePath))
                {
                    string previousDirectory = context.includeDirectories.Peek();
                    filePath = Path.Combine(previousDirectory, filePath);
                    newIncludeDirectory = Path.GetDirectoryName(filePath)!;
                }
                else
                {
                    newIncludeDirectory = Path.GetDirectoryName(filePath)!;
                }

                string text = File.ReadAllText(filePath);
                byte[] contents = Encoding.ASCII.GetBytes(text);
                byte* pContents = (byte*)Marshal.AllocHGlobal(contents.Length + 1);
                Marshal.Copy(contents, 0, (IntPtr)pContents, contents.Length);
                pContents[contents.Length] = 0;
                *data = pContents;
                *bytes = (uint)contents.Length;
                context.includeDirectories.Push(newIncludeDirectory);
                return S_OK;
            }
            catch (FileNotFoundException e)
            {
                return e.HResult;
            }
        }
        private static unsafe int Close(ID3DInclude* _self, void* pData)
        {
            ref ID3DIncludeExContext context = ref Unsafe.AsRef<ID3DIncludeExContext>((*(ID3DIncludeEx*)_self).pID3DIncludeExContext);
            context.includeDirectories.Pop();

            Marshal.FreeHGlobal((nint)pData);
            return S_OK;
        }
        internal static unsafe CompileResult CompileShaderFromFile(string fullFileName, CompilerFlags flags, bool tryParseDiagnostics)
        {
            using LPCSTR fileName = new LPCSTR(fullFileName.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));
            ID3DIncludeExContext context = new ID3DIncludeExContext(fullFileName);

            ID3DIncludeEx include;
            ID3DInclude.Vtbl vtbl;
            include.lpVtbl = &vtbl;
            vtbl.Open = POpen;
            vtbl.Close = PClose;
            include.pID3DIncludeExContext = Unsafe.AsPointer(ref context);

            ID3DIncludePtr includePtr = new ID3DIncludePtr((ID3DInclude*)&include);
            int result;
            ID3D10BlobPtr shaderCode;
            ID3D10BlobPtr errorMessages;
            string text = File.ReadAllText(fullFileName);
            byte[] asciiContents = Encoding.ASCII.GetBytes(text);
            fixed (void* ptr = asciiContents)
                result = D3DCompiler.D3DCompile(ptr, (uint)asciiContents.Length, fileName, EHFXCMacros.Macros, includePtr, default, StrProfile_fx_2_0.LPCSTR, flags, 0, out shaderCode, out errorMessages);
            GC.KeepAlive(StrProfile_fx_2_0);
            GC.KeepAlive(EHFXCMacros);
            GC.KeepAlive(CloseDelegate);
            GC.KeepAlive(OpenDelegate);
            using (shaderCode)
            using (errorMessages)
            {
                // https://learn.microsoft.com/en-us/windows/win32/direct3d11/how-to--compile-a-shader
                // https://learn.microsoft.com/es-es/windows/win32/api/debugapi/nf-debugapi-outputdebugstringa
                GetDiagnostics(errorMessages, tryParseDiagnostics, out var diagnostics, out string diagnosticsStr);

                return new CompileResult()
                {
                    hresult = result,
                    effectData = shaderCode.IsNull ? null : shaderCode.ToArray(),
                    diagnostics = diagnostics,
                    anyErrors = result != 0,
                    diagnosticsStr = diagnosticsStr,
                };
            }
        }
        private static void GetDiagnostics(ID3D10BlobPtr errorMessages, bool parseDiagnostics, out CompileDiagnostic[] diagnostics, out string diagnosticsStr)
        {
            diagnosticsStr = "";
            if (!errorMessages.IsNull)
                diagnosticsStr = Marshal.PtrToStringAnsi(errorMessages.BufferPointer, errorMessages.BufferSize);
            diagnostics = Array.Empty<CompileDiagnostic>();
            if (parseDiagnostics)
            {
                diagnostics = GetDiagnostics(diagnosticsStr);
                diagnosticsStr = "";
            }
        }
        private static CompileDiagnostic[] GetDiagnostics(string diagnosticsStr)
        {
            List<CompileDiagnostic> diagnostics = new();
            string[] diagnosticsLines = diagnosticsStr?.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (string diagnostic in diagnosticsLines)
            {
                if (diagnostic == "\0")
                    continue;

                // 1. warning X4717: Effects deprecated for D3DCompiler_47
                // 2. <cut full path>\EtherealHorizons\Assets\Effects\ScreenShaders\Assets\Effects\ScreenShaders\Test.fx(2,10-14): error X1507: failed to open source file: 'n.h'
                // filepath(column-endcolumn,row-endrow): error 
                // if the message does not start with "warning" or "error", its assumed  to be the 2nd format
                var match = DiagnosticFormatRegex.Match(diagnostic);
                if (!match.Success)
                {
                    diagnostics.Add(new CompileDiagnostic(true, 0, 0, 0, 0, diagnostic, "", ""));
                }
                var groups = match.Groups;
                string? diagnosticFilePath = groups["filepath"].Value;
                if (string.IsNullOrWhiteSpace(diagnosticFilePath))
                    diagnosticFilePath = null; //relativeName;
                int row = int.TryParse(groups["row"].Value, out int res) ? res : 0;
                int rowEnd = int.TryParse(groups["rowend"].Value, out res) ? res : row;
                int column = int.TryParse(groups["column"].Value, out res) ? res : 0;
                int columnEnd = int.TryParse(groups["columnend"].Value, out res) ? res : column;
                bool isWarning = groups["kind"].Value.Equals("warning", StringComparison.OrdinalIgnoreCase);
                string code = groups["code"].Value;
                string message = groups["diagmsg"].Value;

                diagnostics.Add(new CompileDiagnostic(isWarning, column, columnEnd, row, rowEnd, message, code, diagnosticFilePath));

            }
            return diagnostics.ToArray();
        }
    }
}
