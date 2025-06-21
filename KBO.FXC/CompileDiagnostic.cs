namespace KBO.FXC
{
    internal struct CompileDiagnostic
    {
        public bool isWarning;
        public int column, columnEnd;
        public int row, rowEnd;
        public string message;
        public string code;
        public string? file;
        public CompileDiagnostic(bool isWarning, int column, int columnEnd, int row, int rowEnd, string message, string code, string? file)
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
    }
}
