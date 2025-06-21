using D3DBindings;
using System;
using System.Runtime.InteropServices;

namespace KBO.FXC
{
    public sealed unsafe class DefineMacros : IDisposable
    {
        public D3D_SHADER_MACRO* Macros { get; private set; }
        public DefineMacros(params (string Name, string? Definition)[] macros)
        {
            D3D_SHADER_MACRO* pMacros = (D3D_SHADER_MACRO*)Marshal.AllocHGlobal(sizeof(D3D_SHADER_MACRO) * (macros.Length + 1));
            for (int i = 0; i < macros.Length; i++)
            {
                pMacros[i].Name = new LPCSTR(macros[i].Name);
                pMacros[i].Definition = new LPCSTR(macros[i].Definition ?? "");
            }
            pMacros[macros.Length] = default;
            Macros = pMacros;
        }

        private void Dispose(bool disposing)
        {
            if (Macros == null)
                return;
            D3D_SHADER_MACRO* p = Macros;

            while (!p->Name.IsNull)
            {
                p->Name.Dispose();
                p->Definition.Dispose();
                p++;
            }
            Marshal.FreeHGlobal((nint)Macros);
            Macros = null;
        }
        ~DefineMacros()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
