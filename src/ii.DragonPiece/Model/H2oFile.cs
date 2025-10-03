namespace ii.DragonPiece.Model
{
    public class H2oFile
    {
        public H2oHeader Header { get; set; } = new H2oHeader();
        public List<H2oFileEntry> FileEntries { get; set; } = [];
    }
}