using D3DBindings;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KBO.FXC
{
    public static class EffectCompiler
    {
        public static HResult CompileEffect(string filePath, IEnumerable<KeyValuePair<string, string>>? macros, string[]? additionalSearchPaths, CompilerFlags flags, out byte[] effectCode, out string? diagnostics)
        {
            byte[] contents = Encoding.ASCII.GetBytes(File.ReadAllText(filePath));

            var allMacros = new Dictionary<string, string?>(4);
            if (macros != null)
                foreach (var pair in macros)
                    allMacros.Add(pair.Key, pair.Value);
            if (!allMacros.ContainsKey("KBOFXC"))
                allMacros["KBOFXC"] = "1";
            HResult result = D3DCompiler.D3DCompile(new ArraySegment<byte>(contents), filePath, new D3DShaderMacros(allMacros),
                new ID3DIncludeNewHandler(filePath, additionalSearchPaths), entryPoint: null, target: "fx_2_0",
                flags, 0, out effectCode, out diagnostics);

            return result;
        }
    }
}
