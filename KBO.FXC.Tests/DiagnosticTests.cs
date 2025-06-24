using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DBindings;
namespace KBO.FXC.Tests
{
    internal class DiagnosticTests
    {
        [SetUp]
        public void Setup()
        {
            //rootPath = Path.GetFullPath(Path.Combine("teststmp", "include"));
            //if (Directory.Exists(rootPath))
            //    Directory.Delete(rootPath, true);
            //Directory.CreateDirectory(rootPath);
        }

        [Test]
        public void TestDiagnostic()
        {
            string file = "test.fx";
            WriteFile(file, @"sampler2D uImage0 : register(s0);
float4 PSMain(float4 color : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, texCoords) * color;
}

technique t0
{
    pass p0
    {t t
        PixelShader = compile ps_2_0 PSMain();
    }
}");
            EffectCompiler.CompileEffect(file, null, null, CompilerFlags.None, out byte[] code, out string? diagnosticsStr);

            var diagnostics = CompileDiagnostic.GetDiagnostics(diagnosticsStr);

        }
        void WriteFile(string name, string contents)
        {
            string? d = Path.GetDirectoryName(name);
            if (!string.IsNullOrEmpty(d))
                Directory.CreateDirectory(d);
            File.WriteAllText(name, contents);
        }
    }
}
