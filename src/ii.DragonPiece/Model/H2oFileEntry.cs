namespace ii.DragonPiece.Model
{
    public class H2oFileEntry
    {
        public int Compressed { get; set; } // 0 = Uncompressed, other = Compressed
        public int Unknown1 { get; set; }
        public int FilenameIndex { get; set; }
        public int Unknown2 { get; set; }
        public int FileSizeRaw { get; set; }
        public int FileSizeCompressed { get; set; }
        public uint FileOffset { get; set; }
        public uint Unknown3 { get; set; }
        public ushort Unknown4 { get; set; }
        public ushort Unknown5 { get; set; }
        public string? Filename { get; set; } = null;

        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }
}