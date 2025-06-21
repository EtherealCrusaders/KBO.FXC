namespace KBO.FXC
{
    internal struct CompileResult
    {
        public int hresult;
        public byte[]? effectData;
        public bool anyErrors;

        public string? diagnosticsStr;
        //public string? preprocessedText;
        public CompileDiagnostic[] diagnostics;
    }
}
