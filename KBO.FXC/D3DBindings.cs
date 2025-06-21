using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public LPCSTR(string str)
        {
            data = Marshal.StringToHGlobalAnsi(str);
        }
        public LPCSTR(nint existingPtr)
        {
            data = existingPtr;
        }
        public void Dispose()
        {
            if (data == default)
                return;
            Marshal.FreeHGlobal(data);
            data = default;
        }
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

    [Flags]
    public enum CompilerFlags : uint
    {
        None,
        DEBUG = (1 << 0),
        SKIP_VALIDATION = (1 << 1),
        SKIP_OPTIMIZATION = (1 << 2),
        PACK_MATRIX_ROW_MAJOR = (1 << 3),
        PACK_MATRIX_COLUMN_MAJOR = (1 << 4),
        PARTIAL_PRECISION = (1 << 5),
        FORCE_VS_SOFTWARE_NO_OPT = (1 << 6),
        FORCE_PS_SOFTWARE_NO_OPT = (1 << 7),
        NO_PRESHADER = (1 << 8),
        AVOID_FLOW_CONTROL = (1 << 9),
        PREFER_FLOW_CONTROL = (1 << 10),
        ENABLE_STRICTNESS = (1 << 11),
        ENABLE_BACKWARDS_COMPATIBILITY = (1 << 12),
        IEEE_STRICTNESS = (1 << 13),
        OPTIMIZATION_LEVEL0 = (1 << 14),
        OPTIMIZATION_LEVEL1 = 0,
        OPTIMIZATION_LEVEL2 = ((1 << 14) | (1 << 15)),
        OPTIMIZATION_LEVEL3 = (1 << 15),
        RESERVED16 = (1 << 16),
        RESERVED17 = (1 << 17),
        WARNINGS_ARE_ERRORS = (1 << 18),
        RESOURCES_MAY_ALIAS = (1 << 19),
        ENABLE_UNBOUNDED_DESCRIPTOR_TABLES = (1 << 20),
        ALL_RESOURCES_BOUND = (1 << 21),
        DEBUG_NAME_FOR_SOURCE = (1 << 22),
        DEBUG_NAME_FOR_BINARY = (1 << 23),
    }

    internal unsafe class D3DCompiler
    {
        public const int E_FAIL = unchecked((int)0x80004005);
        public static readonly ID3DIncludePtr D3D_COMPILE_STANDARD_FILE_INCLUDE = new((ID3DInclude*)1);

        // 
        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dcompilefromfile
        [DllImport("D3DCompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int D3DCompileFromFile(LPCWSTR pFileName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, LPCSTR pEntrypoint, LPCSTR pTarget, CompilerFlags Flags1, uint Flags2, out ID3D10BlobPtr ppCode, out ID3D10BlobPtr ppErrorMsgs);

        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dcompile
        [DllImport("D3DCompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int D3DCompile(void* pSrcData, /*[NativeTypeName("SIZE_T")]*/ nuint SrcDataSize, LPCSTR pSourceName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, LPCSTR pEntrypoint, LPCSTR pTarget, CompilerFlags Flags1, uint Flags2, out ID3D10BlobPtr ppCode, out ID3D10BlobPtr ppErrorMsgs);

        // https://learn.microsoft.com/en-us/windows/win32/api/d3dcompiler/nf-d3dcompiler-d3dpreprocess
        [DllImport("D3DCompiler_47", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int D3DPreprocess(void* pSrcData, /*[NativeTypeName("SIZE_T")]*/ nuint SrcDataSize, LPCSTR pSourceName, D3D_SHADER_MACRO* pDefines, ID3DIncludePtr pInclude, out ID3D10BlobPtr ppCodeText, out ID3D10BlobPtr ppErrorMsgs);

    }
}
