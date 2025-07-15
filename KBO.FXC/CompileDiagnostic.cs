using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace KBO.FXC
{
    public partial struct CompileDiagnostic
    {
        public bool? isWarning;
        public int column, columnEnd;
        public int row, rowEnd;
        public string message;
        public string code;
        public string? file;

        public CompileDiagnostic(bool? isWarning, int column, int columnEnd, int row, int rowEnd, string message, string code, string? file)
        {
            this.isWarning = isWarning;
            this.column = column;
            this.columnEnd = columnEnd;
            this.row = row;
            this.rowEnd = rowEnd;
            this.message = message;
            this.code = code;
            this.file = file;

        }


        private const string DiagnosticFormatRegexStr = @"\s*((?<filepath>\w:[\w_\\\/. ]+)\((?<row>\d+)(\-(?<rowend>\d+))?\,(?<column>\d+)(\-(?<columnend>\d+))?\)\:\s*)?(?<kind>warning|error)?\s*(?<codeorsource>(X?\d+)|(\w+))\s*\:\s*(?<diagmsg>[\w\d\ :',.]*)"; //@"\s*((?<filepath>\w:[\w_\\\/. ]+)\((?<row>\d+)(\-(?<rowend>\d+))?\,(?<column>\d+)(\-(?<columnend>\d+))?\)\:\s*)?(?<kind>warning|error)\s*(?<code>X?\d+)\s*\:\s*(?<diagmsg>[\w\d\ :',.]*)";
        private const RegexOptions DiagnosticFormatRegexOptions = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex DiagnosticFormatRegex =
#if !NET8_0_OR_GREATER
            new Regex(DiagnosticFormatRegexStr, DiagnosticFormatRegexOptions);
#else
            DiagnosticFormatRegexF();
        [GeneratedRegex(DiagnosticFormatRegexStr, DiagnosticFormatRegexOptions)]
        private static partial Regex DiagnosticFormatRegexF();
#endif

        private static readonly char[] newlinechar = new char[] { '\n' };
        public static CompileDiagnostic[] GetDiagnostics(string? d3dOutputDiagnostics) // string file parameter
        {
            if (string.IsNullOrWhiteSpace(d3dOutputDiagnostics))
                return Array.Empty<CompileDiagnostic>();

            List<CompileDiagnostic> diagnostics = new();
            string[] diagnosticsLines = d3dOutputDiagnostics?.Split(newlinechar, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (string diagnostic in diagnosticsLines)
            {
                if (diagnostic == "\0")
                    continue;

                // 1. warning X4717: Effects deprecated for D3DCompiler_47
                // 2. <cut full path>\EtherealHorizons\Assets\Effects\ScreenShaders\Assets\Effects\ScreenShaders\Test.fx(2,10-14): error X1507: failed to open source file: 'n.h'
                // if the message does not start with "warning" or "error", its assumed to be the 2nd format
                var match = DiagnosticFormatRegex.Match(diagnostic);
                if (!match.Success)
                {
                    diagnostics.Add(new CompileDiagnostic(true, 0, 0, 0, 0, diagnostic, "", null));
                }
                var groups = match.Groups;
                string? diagnosticFilePath = groups["filepath"].Value.Trim();
                if (string.IsNullOrWhiteSpace(diagnosticFilePath))
                    diagnosticFilePath = null; //relativeName;
                int row = int.TryParse(groups["row"].Value, out int res) ? res : 0;
                int rowEnd = int.TryParse(groups["rowend"].Value, out res) ? res : row;
                int column = int.TryParse(groups["column"].Value, out res) ? res : 0;
                int columnEnd = int.TryParse(groups["columnend"].Value, out res) ? res : column;
                string kind = groups["kind"].Value;
                bool? isWarning;
                if (string.IsNullOrWhiteSpace(kind))
                    isWarning = null;
                else 
                    isWarning = kind.Equals("warning", StringComparison.OrdinalIgnoreCase);
                string codeOrSource = groups["codeorsource"].Value;
                string message = groups["diagmsg"].Value;

                if (message == "Compilation failed")
                    isWarning = false;

                diagnostics.Add(new CompileDiagnostic(isWarning, column, columnEnd, row, rowEnd, message, codeOrSource, diagnosticFilePath));
            }
            return diagnostics.ToArray();
        }
    }
}
