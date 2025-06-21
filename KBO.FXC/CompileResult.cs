namespace KBO.FXC
{
    public struct CompileResult
    {
        public int hresult;
        public byte[]? effectData;
        public bool anyErrors;

        public string? diagnosticsStr;
        public CompileDiagnostic[] diagnostics;
    }
}
