using D3DBindings;
using KBOFXC;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace KBO.FXC.Tests.IncludeTests
{
    // TODO: fix tests
    public class IncludeTests
    {
        struct IncludeFile
        {
            public string Name;
            public (bool systemInclude, IncludeFile include)[] includes;

            public IncludeFile(string name, params (bool systemInclude, IncludeFile include)[] includes)
            {
                Name = name;
                this.includes = includes;
            }

            public static implicit operator IncludeFile(string str) => new() { Name = str };
            //public static IncludeFile From(string file, string[] included, string[] systemIncluded) => new IncludeFile() { };
        }

        string rootPath ;
        [SetUp]
        public void Setup()
        {
            rootPath = Path.GetFullPath(Path.Combine("teststmp", "include"));
            if(Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
            Directory.CreateDirectory(rootPath);
        }

        [Test]
        public async Task Test1Parent()
        {
            await TestEqualResultWithFxc(
            new IncludeFile("folder/testfile.fx", includes:
            [
                (systemInclude: false, new IncludeFile("testheader.fx")),
            ]), includePaths: []);
        }
        [Test]
        public async Task Test2DoubleParent()
        {
            await TestEqualResultWithFxc(
            new IncludeFile("folder1/folder2/testfile.fx", includes:
            [
                (systemInclude: false, new IncludeFile("folder1/utils.h", includes:
                [
                    (systemInclude: false, new IncludeFile("globalutils.h")),
                ])),
            ]), includePaths: []);
        }
        void CreateFile(string path)
        {

        }
        async Task TestEqualResultWithFxc(IncludeFile file, string[] includePaths, [CallerMemberName] string testName = "")
        {
            if (string.IsNullOrWhiteSpace(testName))
                testName = "unnamed";
            string start = Path.Combine(rootPath, testName);
            string targetFilePath = Path.Combine(start, file.Name);
            byte[] contents = Visit(file);
            byte[] Visit(IncludeFile file)
            {
                string outPath = Path.Combine(start, file.Name);
                MemoryStream ms = new(128);
                using (StreamWriter writer = new(ms, Encoding.ASCII, leaveOpen: true))
                {
                    foreach ((bool systemInclude, IncludeFile include) in file.includes)
                    {
                        writer.Write("#include ");
                        writer.Write(systemInclude ? '<' : '"');
                        string path = Path.GetRelativePath(Path.GetDirectoryName(outPath)!, Path.Combine(start, include.Name));
                        writer.Write(path);
                        writer.Write(systemInclude ? '>' : '"');
                        writer.WriteLine();
                        Visit(include);
                    }
                    writer.WriteLine($"#error from include at {Path.GetRelativePath(start, outPath)}");
                }
                byte[] arr = ms.ToArray();
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                File.WriteAllBytes(outPath, arr);
                return arr;
            }

            MemoryStream ms = new(256);
            ms.Write(contents);
            using (StreamWriter writer = new(ms, Encoding.ASCII, leaveOpen: true))
            {
                writer.WriteLine(@"
sampler2D uImage0 : register(s0);
float4 PSMain(float4 color : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, texCoords) * color;
}

technique t0
{
    pass p0
    {
        PixelShader = compile ps_2_0 PSMain();
    }
}");
            }
            contents = ms.ToArray();

            File.WriteAllBytes(targetFilePath, contents);

            (byte[] code2, string errorMessages2) = await InvokeFxc(targetFilePath, CompilerFlags.None);
            HResult result = D3DCompiler.D3DCompile(contents, targetFilePath, null, new ID3DIncludeNewHandler(targetFilePath, includePaths), testName, "fx_2_0", CompilerFlags.None, 0, out byte[] code1, out string? errorMessages1);

            string d3dcompilerDiagnostics = errorMessages1.Trim();
            string fxcDiagnostics = errorMessages2.Replace("compilation failed; no code produced", null).Trim();
            // dont throw the result because this is always supposed to fail
            Assert.Multiple(() =>
            {
                //Assert.That(code1, Is.EquivalentTo(code2));
                Assert.That(d3dcompilerDiagnostics, Is.EquivalentTo(fxcDiagnostics));
            });

        }
        private async Task<(byte[], string)> InvokeFxc(string file, CompilerFlags flags)
        {
            string outputFilePath = Path.ChangeExtension(file, ".fxc");
            ProcessStartInfo startInfo = new ProcessStartInfo(TestHelpers.GetFXC())
            {

            };
            startInfo.ArgumentList.Add(file);
            startInfo.ArgumentList.Add("/nologo");
            startInfo.ArgumentList.Add("/T");
            startInfo.ArgumentList.Add("fx_2_0");
            startInfo.ArgumentList.Add("/Fo");
            startInfo.ArgumentList.Add(outputFilePath);
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            using var process = Process.Start(startInfo);
            _ = process!.Id;
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process!.WaitForExitAsync().ConfigureAwait(false);


            string diagnostics = error;
            if (File.Exists(outputFilePath))
                return (File.ReadAllBytes(file), diagnostics);
            return ([], diagnostics);
        }

        //private void TestEqualResultsWithAndWithoutHandler(string shaderFilePath, CompilerFlags flags, [CallerMemberName] string testName = "")
        //{
        //    byte[] contents = Encoding.ASCII.GetBytes(File.ReadAllText(shaderFilePath));
        //    HResult r1 = D3DCompiler.D3DCompile(contents, shaderFilePath, null, new ID3DIncludeNewHandler(), null, null, flags, 0, out byte[] code1, out string? errorMessages1);
        //    HResult r2 = D3DCompiler.D3DCompile(contents, shaderFilePath, null, null, null, null, flags, 0, out byte[] code2, out string? errorMessages2);

        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(r1.code, Is.EqualTo(r2.code));
        //        Assert.That(code1, Is.EquivalentTo(code2));
        //        Assert.That(errorMessages1, Is.EquivalentTo(errorMessages2));
        //    });
        //}
    }
}