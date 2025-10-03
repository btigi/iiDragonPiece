namespace ii.DragonPiece.Model
{
    public class H2oHeader
    {
        public string Signature { get; set; } = "";
        public float Version { get; set; }
        public string Comment { get; set; } = "";
        public int VersionInt { get; set; }
        public int FileCount { get; set; }
        public int FileCountActual { get; set; }
        public int Unknown1 { get; set; }
        public int FileSizeCompressed { get; set; }
        public int Unknown2 { get; set; }
        public int FileSizeRaw { get; set; }
        public byte[] Unknown3 { get; set; } = new byte[36];
        public int FileEntryOffset { get; set; }
    }
}