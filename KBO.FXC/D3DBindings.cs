using KBO.FXC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

// some definitions are generated from clangsharp

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace D3DBindings
{
    /// <summary>
    /// Object wrapper for <see cref="D3DBindings.LPCSTR"/>.
    /// </summary>
    public sealed class LPCSTRObj : IDisposable
    {
        private LPCSTR lpcstr;

        public LPCSTR LPCSTR => lpcstr;
        public nint Data => lpcstr.data;
        public LPCSTRObj(string str)
        {
            lpcstr = new(str);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            lpcstr.Dispose();
            lpcstr = default;
        }

        ~LPCSTRObj()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A <see href="https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-dtyp/f8d4fe46-6be8-44c9-8823-615a21d17a61">LPCSTR</see> is a 32-bit pointer to a constant null-terminated string of 8-bit Windows (ANSI) characters. <br/>
    /// </summary>
    public unsafe struct LPCSTR : IDisposable
    {
        public nint data;
        public readonly bool IsNull => data == 0;

        public LPCSTR(string? str)
        {
            data = str == null ? 0 : Marshal.StringToHGlobalAnsi(str);
        }
        public LPCSTR(nint existingPtr)
        {
            data = existingPtr;
        }
        public void Free()
        {

            if (data == default)
                return;
            Marshal.FreeHGlobal(data);
            data = default;
        }
        public void Dispose()
        {
            Free();
        }

        internal readonly string? DebugDisplayString => data == 0 ? null : Marshal.PtrToStringAnsi(data);
        public readonly override string ToString()
        {
            if (data == default)
                return "";
            return Marshal.PtrToStringAnsi(data)!;
        }
    }

    /// <summary>
    /// A <see href="https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-dtyp/76f10dd8-699d-45e6-a53c-5aefc586da20">LPCWSTR</see> is a 32-bit pointer to a constant string of 16-bit Unicode characters, which MAY be null-terminated. <br/>
    /// </summary>
    public unsafe struct LPCWSTR : IDisposable
    {
        public nint data;

        public LPCWSTR(string str)
        {
            data = Marshal.StringToHGlobalUni(str);
        }

        public void Dispose()
        {
            if (data == default)
                return;
            Marshal.FreeHGlobal(data);
            data = default;
        }
    }

    public enum D3D_INCLUDE_TYPE
    {
        Local = 0,
        System = 1,
        D3D_INCLUDE_LOCAL = 0,
        D3D_INCLUDE_SYSTEM = (D3D_INCLUDE_LOCAL + 1),
        D3D10_INCLUDE_LOCAL = D3D_INCLUDE_LOCAL,
        D3D10_INCLUDE_SYSTEM = D3D_INCLUDE_SYSTEM,
        //D3D_INCLUDE_FORCE_DWORD = 0x7fffffff,
    }

    public unsafe partial struct ID3DInclude
    {
        public Vtbl* lpVtbl;

        public unsafe partial struct Vtbl
        {
            public delegate* unmanaged[Stdcall]<ID3DInclude*, D3D_INCLUDE_TYPE, LPCSTR, void*, void**, uint*, int> Open;

            public delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int> Close;
        }
    }
    public unsafe struct ID3DIncludePtr
    {
        public ID3DInclude* include;
        public ID3DIncludePtr(ID3DInclude* include)
        {
            this.include = include;
        }
    }

    public unsafe partial struct ID3D10Blob
    {
        public Vtbl* lpVtbl;

        public unsafe partial struct Vtbl
        {
            public delegate* unmanaged[Stdcall]<ID3D10Blob*, Guid*, void**, int> QueryInterface;

            public delegate* unmanaged[Stdcall]<ID3D10Blob*, uint> AddRef;

            public delegate* unmanaged[Stdcall]<ID3D10Blob*, uint> Release;

            public delegate* unmanaged[Stdcall]<ID3D10Blob*, void*> GetBufferPointer;

            public delegate* unmanaged[Stdcall]<ID3D10Blob*, nuint> GetBufferSize;
        }

        public int QueryInterface(in Guid guid, out void* p)
        {
            fixed (ID3D10Blob* ptr = &this)
            fixed (Guid* pGuid = &guid)
            fixed (void** pResult = &p)
                return lpVtbl->QueryInterface(ptr, pGuid, pResult);
        }
        public uint AddRef()
        {
            fixed (ID3D10Blob* ptr = &this)
                return lpVtbl->AddRef(ptr);
        }
        public uint Release()
        {
            fixed (ID3D10Blob* ptr = &this)
                return lpVtbl->Release(ptr);
        }
        public void* GetBufferPointer()
        {
            fixed (ID3D10Blob* ptr = &this)
                return lpVtbl->GetBufferPointer(ptr);
        }
        public nuint GetBufferSize()
        {
            fixed (ID3D10Blob* ptr = &this)
                return lpVtbl->GetBufferSize(ptr);
        }
    }

    public unsafe struct ID3D10BlobPtr : IDisposable
    {
        public ID3D10Blob* blob;

        public readonly bool IsNull => blob == null;

        public readonly int BufferSize => checked((int)blob->GetBufferSize());
        public readonly nuint LongBufferSize => blob->GetBufferSize();
        public readonly nint BufferPointer => (nint)blob->GetBufferPointer();

        public readonly byte[] ToArray()
        {
            if (IsNull)
                return null!;
            byte[] arr = new byte[BufferSize];
            Marshal.Copy(BufferPointer, arr, 0, arr.Length);
            return arr;
        }
        //public Span<byte> AsSpan()
        //{
        //    return new(blob->GetBufferPointer(), checked((int)blob->GetBufferSize()));
        //}

        public void Dispose()
        {
            if (blob != null)
                blob->Release();
            blob = null;
        }
    }

    public partial struct D3D_SHADER_MACRO
    {
        public LPCSTR Name;

        public LPCSTR Definition;
    }
    public sealed unsafe class D3DShaderMacros : IDisposable
    {
        public D3D_SHADER_MACRO* PMacros { get; private set; }
        public D3DShaderMacros(IEnumerable<KeyValuePair<string, string?>> macros)
        {
            List<D3D_SHADER_MACRO> macrosList = new List<D3D_SHADER_MACRO>();

            foreach (var macro in macros)
            {
                macrosList.Add(new D3D_SHADER_MACRO()
                {
                    Name = new LPCSTR(macro.Key),
                    Definition = new LPCSTR(macro.Value ?? "")
                });
            }

            D3D_SHADER_MACRO* pMacros = (D3D_SHADER_MACRO*)Marshal.AllocHGlobal(sizeof(D3D_SHADER_MACRO) * (macrosList.Count + 1));
            for (int i = 0; i < macrosList.Count; i++)
                pMacros[i] = macrosList[i];
            pMacros[macrosList.Count] = default;
            PMacros = pMacros;
        }

        private void Dispose(bool disposing)
        {
            if (PMacros == null)
                return;
            D3D_SHADER_MACRO* p = PMacros;

            while (!p->Name.IsNull)
            {
                p->Name.Dispose();
                p->Definition.Dispose();
                p++;
            }
            Marshal.FreeHGlobal((nint)PMacros);
            PMacros = null;
        }
        ~D3DShaderMacros()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    [Flags]
    public enum CompilerFlags : uint
    {
        None,
        Debug = (1 << 0),
        SkipValidation = (1 << 1),
        SkipOptimization = (1 << 2),
        PackMatrixRowMajor = (1 << 3),
        PackMatrixColumnMajor = (1 << 4),
        PartialPrecision = (1 << 5),
        ForceVSSoftwareNoOpt = (1 << 6),
        ForcePSSoftwareNoOpt = (1 << 7),
        NoPreshader = (1 << 8),
        AvoidFlowControl = (1 << 9),
        PreferFlowControl = (1 << 10),
        EnableStrictness = (1 << 11),
        EnableBackwardsCompatibility = (1 << 12),
        IEEEStrictness = (1 << 13),
        OptimizationLevel0 = (1 << 14),
        OptimizationLevel1 = 0,
        OptimizationLevel2 = ((1 << 14) | (1 << 15)),
        OptimizationLevel3 = (1 << 15),
        Reserved16 = (1 << 16),
        Reserved17 = (1 << 17),
        WarningsAreErrors = (1 << 18),
        ResourcesMayAlias = (1 << 19),
        EnableUnboundedDescriptorTables = (1 << 20),
        AllResourcesBound = (1 << 21),
        DebugNameForSource = (1 << 22),
        DebugNameForBinary = (1 << 23),
    }

    public interface ID3DIncludeManaged
    {
        int Open(D3D_INCLUDE_TYPE includeType, string fileName, nint parentData, out nint data, out int dataLength);
        int Close(nint data);
    }

    public sealed class ID3DIncludeNewHandler : ID3DIncludeManaged
    {
        public string[] SystemIncludePaths { get; }
        public ID3DIncludeNewHandler(string initialFile, string[]? systemIncludePaths = null)
        {
            SystemIncludePaths = systemIncludePaths ?? Array.Empty<string>();
            visitedPathStack.Push(Path.GetDirectoryName(initialFile)!);
        }

        // assumes a stack-ish behaviour for Open and Close
        // <source file>
        // +- #include "header.h"
        //    +- Open("header.h")
        //    \- Close("header.h")
        // +- #include "otherheader.h"     
        //      +- Open("otherheader.h")
        //      +- #include "anotherheader.h"
        //      |  +- Open("anotherheader.h")
        //      |  \- Close("anotherheader.h")
        //      \- Close("otherheader.h")
        // 
        Stack<string> visitedPathStack = new Stack<string>();
        public int Close(nint data)
        {
            Marshal.FreeHGlobal(data);
            visitedPathStack.Pop();
            return HResult.S_OK;
        }

        public int Open(D3D_INCLUDE_TYPE includeType, string fileName, nint parentData, out nint data, out int dataLength)
        {
            if (includeType == D3D_INCLUDE_TYPE.Local)
            {
                // file 
                // \ #include A
                //   \ #include B
                // B's directory is checked first
                // A's directory is checked
                // file's directory is checked
                foreach (string dir in visitedPathStack)
                {
                    if (TryOpen(dir, fileName, out data, out dataLength))
                    {
                        return HResult.S_OK;
                    }
                }
            }
            foreach (string path in SystemIncludePaths)
            {
                if (TryOpen(path, fileName, out data, out dataLength))
                    return HResult.S_OK;
            }
            unsafe bool TryOpen(string folder, string fileName, out nint data, out int dataLength)
            {
                data = default;
                dataLength = 0;
                string filePath = Path.Combine(folder, fileName);
                try
                {
                    string text = File.ReadAllText(filePath);
                    byte* pData = (byte*)Marshal.AllocHGlobal(text.Length + 1);

                    try
                    {
                        int written;
                        fixed (char* pText = text)
                            written = Encoding.ASCII.GetBytes(pText, text.Length, pData, text.Length);

                        if (written >= text.Length + 1)
                            throw new Exception("written >= text.Length + 1");
                        pData[text.Length] = 0;
                        data = (nint)pData;
                        dataLength = text.Length;
                        visitedPathStack.Push(Path.GetDirectoryName(filePath)!);
                    }
                    catch
                    {
                        Marshal.FreeHGlobal((nint)pData);
                        data = default;
                        dataLength = 0;
                        throw;
                    }
                    return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
            data = default;
            dataLength = 0;
            return HResult.ERROR_FILE_NOT_FOUND;
        }
    }

    //public class D3DIncludeHandler
    //{
    //    public List<string> AdditionalSearchPaths { get; } = new List<string>();
    //    public D3DIncludeHandler()
    //    {
    //    }


    //    public virtual int Open(D3D_INCLUDE_TYPE type, string fileName, nint parentData, out ArraySegment<byte> asciiContents)
    //    {
    //        try
    //        {
    //            string filePath = fileName.ToString();
    //            string newIncludeDirectory;
    //            if (!Path.IsPathRooted(filePath))
    //            {
    //                string previousDirectory = visitedIncludeDirectories.Peek();
    //                filePath = Path.Combine(previousDirectory, filePath);
    //                newIncludeDirectory = Path.GetDirectoryName(filePath)!;
    //            }
    //            else
    //            {
    //                newIncludeDirectory = Path.GetDirectoryName(filePath)!;
    //            }


    //            string text = File.ReadAllText(filePath);
    //            asciiContents = Encoding.ASCII.GetBytes(text);
    //            //byte* pContents = (byte*)Marshal.AllocHGlobal(contents.Length + 1);
    //            //Marshal.Copy(contents, 0, (IntPtr)pContents, contents.Length);
    //            //pContents[contents.Length] = 0;
    //            //*asciiContents = pContents;
    //            //*bytes = (uint)contents.Length;
    //            visitedIncludeDirectories.Push(newIncludeDirectory);
    //            return D3DCompiler.S_OK;
    //        }
    //        catch (FileNotFoundException e)
    //        {
    //            return e.HResult;
    //        }
    //    }
    //    public virtual int Close()
    //    {
    //        context.visitedIncludeDirectories.Pop();

    //        Marshal.FreeHGlobal((nint)pData);
    //        return D3DCompiler.S_OK;
    //    }



    //    public ID3DIncludePtr IncludePtr =>
    //}

    public readonly struct HResult
    {
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int S_OK = 0;
        public const int ERROR_FILE_NOT_FOUND = unchecked((int)0x00000002);

        public static readonly HResult Ok = new(S_OK);
        public static readonly HResult Fail = new(E_FAIL);

        public HResult(int code)
        {
            this.code = code;
        }
        public readonly int code;

        public readonly void Throw()
        {
            Marshal.ThrowExceptionForHR(code);
        }
    }

    public static unsafe class D3DCompiler
    {
        public static readonly ID3DIncludePtr D3D_COMPILE_STANDARD_FILE_INCLUDE = new((ID3DInclude*)1);


        private unsafe ref struct ID3DIncludeEx
        {
            public ID3DInclude.Vtbl* lpVtbl;
            public void* pD3DIncludeHandler;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ID3DIncludeOpen(ID3DInclude* self, D3D_INCLUDE_TYPE includeType, LPCSTR fileName, void* parentData, void** data, uint* bytes);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ID3DIncludeClose(ID3DInclude* self, void* pData);

        private static unsafe readonly ID3DIncludeOpen OpenDelegate = CallbackOpen;
        private static unsafe readonly ID3DIncludeClose CloseDelegate = CallbackClose;
        private static unsafe readonly delegate* unmanaged[Stdcall]<ID3DInclude*, D3D_INCLUDE_TYPE, LPCSTR, void*, void**, uint*, int> POpen = (delegate* unmanaged[Stdcall]<ID3DInclude*, D3D_INCLUDE_TYPE, LPCSTR, void*, void**, uint*, int>)Marshal.GetFunctionPointerForDelegate(OpenDelegate);
        private static unsafe readonly delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int> PClose = (delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int>)Marshal.GetFunctionPointerForDelegate(CloseDelegate);
        private static unsafe int CallbackOpen(ID3DInclude* _self, D3D_INCLUDE_TYPE includeType, LPCSTR fileName, void* parentData, void** data, uint* bytes)
        {
            ID3DIncludeManaged handler = Unsafe.AsRef<ID3DIncludeManaged>(((ID3DIncludeEx*)_self)->pD3DIncludeHandler);
            int result = handler.Open(includeType, fileName.ToString(), (nint)parentData, out *(nint*)data, out *(int*)bytes);
            return result;
        }
        private static unsafe int CallbackClose(ID3DInclude* _self, void* pData)
        {
            ID3DIncludeManaged handler = Unsafe.AsRef<ID3DIncludeManaged>(((ID3DIncludeEx*)_self)->pD3DIncludeHandler);
            return handler.Close((nint)pData);
        }

        public static HResult D3DCompile(ArraySegment<byte> asciiShaderCode, string fileName, D3DShaderMacros? macros, ID3DIncludeManaged? include, string? entryPoint, string? target, CompilerFlags compilerFlags, uint flags2, out byte[] code, out string? errorMessages)
        {
            using LPCSTR pFileName = new(fileName.Replace('/', '\\'));
            using LPCSTR pEntryPoint = new(entryPoint);
            using LPCSTR pTarget = new(target);

            ID3DInclude.Vtbl vtbl;
            vtbl.Open = POpen;
            vtbl.Close = PClose;
            ID3DIncludeEx id3dinclude;
            id3dinclude.lpVtbl = &vtbl;
            id3dinclude.pD3DIncludeHandler = Unsafe.AsPointer(ref include);
            HResult result;
            ID3D10BlobPtr pCode, pErrorMsgs;
            fixed (byte* pData = asciiShaderCode.Array!)
            {
                ID3DIncludePtr pInclude = include == null ? D3D_COMPILE_STANDARD_FILE_INCLUDE : new ID3DIncludePtr((ID3DInclude*)&id3dinclude);
                D3D_SHADER_MACRO* pMacros = macros == null ? null : macros.PMacros;
                result = D3DCompile(pData + asciiShaderCode.Offset, (uint)asciiShaderCode.Count, pFileName, pMacros, pInclude, pEntryPoint, pTarget, compilerFlags, flags2, out pCode, out pErrorMsgs);
            }
            code = pCode.ToArray();
            errorMessages = pErrorMsgs.IsNull ? null : Marshal.PtrToStringAnsi(pErrorMsgs.BufferPointer, pErrorMsgs.BufferSize - 1); // - 1 to skip null char
            return result;
        }

        //public static HResult D3DCompileFromFile(string fileName, D3DShaderMacros? macros, ID3DIncludeManaged? include, string? entryPoint, string? target, CompilerFlags compilerFlags, uint flags2, out byte[] code, out string? errorMessages) 
        //{
        //    using LPCWSTR pFileName = new(fileName);
        //    using LPCSTR pEntryPoint = new(entryPoint);
        //    using LPCSTR pTarget = new(target);

        //    ID3DInclude.Vtbl vtbl;
        //    vtbl.Open = POpen;
        //    vtbl.Close = PClose;
        //    ID3DIncludeEx id3dinclude;
        //    id3dinclude.lpVtbl = &vtbl;
        //    id3dinclude.pD3DIncludeHandler = Unsafe.AsPointer(ref include);

        //    HResult result = D3DCompileFromFile(pFileName, macros.PMacros, new ID3DIncludePtr((ID3DInclude*)&id3dinclude), pEntryPoint, pTarget, compilerFlags, flags2, out var pCode, out var pErrorMsgs);
        //    code = pCode.ToArray();
        //    errorMessages = Marshal.PtrToStringAnsi(pErrorMsgs.BufferPointer, pErrorMsgs.BufferSize - 1);
        //    return result;
        //}

        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dcompilefromfile
        [DllImport("d3dcompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern HResult D3DCompileFromFile(LPCWSTR pFileName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, LPCSTR pEntrypoint, LPCSTR pTarget, CompilerFlags Flags1, uint Flags2, out ID3D10BlobPtr ppCode, out ID3D10BlobPtr ppErrorMsgs);

        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dcompile
        [DllImport("d3dcompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern HResult D3DCompile(void* pSrcData, /*[NativeTypeName("SIZE_T")]*/ nuint SrcDataSize, LPCSTR pSourceName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, LPCSTR pEntrypoint, LPCSTR pTarget, CompilerFlags Flags1, uint Flags2, out ID3D10BlobPtr ppCode, out ID3D10BlobPtr ppErrorMsgs);

        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dpreprocess
        [DllImport("d3dcompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern HResult D3DPreprocess(void* pSrcData, /*[NativeTypeName("SIZE_T")]*/ nuint SrcDataSize, LPCSTR pSourceName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, out ID3D10BlobPtr ppCodeText, out ID3D10BlobPtr ppErrorMsgs);

    }
}
