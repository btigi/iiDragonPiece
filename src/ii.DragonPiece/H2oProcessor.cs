using ii.DragonPiece.Model;
using System.Text;

namespace ii.DragonPiece
{
    public class H2oProcessor
    {
        public H2oFile Process(string filename, LqrFile lqrFile)
        {
            try
            {
                using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(fileStream);
                var result = new H2oFile();

                var signature = Encoding.ASCII.GetString(reader.ReadBytes(8));
                if (signature != "LIQDLH2O")
                {
                    throw new InvalidDataException($"Invalid H2O signature: {signature}.");
                }

                reader.BaseStream.Position = 0;

                // -------------------------------------------------------
                // Header
                // -------------------------------------------------------
                result.Header = new H2oHeader();
                result.Header.Signature = Encoding.ASCII.GetString(reader.ReadBytes(8));
                result.Header.Version = reader.ReadSingle();
                var commentBytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0x1a)
                {
                    commentBytes.Add(b);
                }
                result.Header.Comment = Encoding.UTF8.GetString(commentBytes.ToArray());
                result.Header.VersionInt = reader.ReadInt32();
                result.Header.FileCount = reader.ReadInt32();
                result.Header.FileCountActual = reader.ReadInt32();
                result.Header.Unknown1 = reader.ReadInt32();
                result.Header.FileSizeCompressed = reader.ReadInt32();
                result.Header.Unknown2 = reader.ReadInt32();
                result.Header.FileSizeRaw = reader.ReadInt32();
                result.Header.Unknown3 = reader.ReadBytes(36);
                result.Header.FileEntryOffset = reader.ReadInt32();

                var filenames = lqrFile?.FileEntries.Select(s => s.Filename).ToArray();

                // -------------------------------------------------------
                // Unknown
                // -------------------------------------------------------
                // 32 bytes unknown

                // -------------------------------------------------------
                // File entries
                // -------------------------------------------------------
                var entries = new List<H2oFileEntry>();
                reader.BaseStream.Seek(32, SeekOrigin.Current);

                var fileCount = 0;
                for (var i = 0; i < result.Header.FileCount; i++)
                {
                    var entry = new H2oFileEntry();

                    entry.Compressed = reader.ReadInt32(); // 0 = Uncompressed, other = Compressed
                    entry.Unknown1 = reader.ReadInt32();
                    entry.FilenameIndex = reader.ReadInt32();
                    entry.Unknown2 = reader.ReadInt32();
                    entry.FileSizeRaw = reader.ReadInt32();
                    entry.FileSizeCompressed = reader.ReadInt32();
                    entry.FileOffset = reader.ReadUInt32();
                    entry.Unknown3 = reader.ReadUInt32();
                    entry.Unknown4 = reader.ReadUInt16();
                    entry.Unknown5 = reader.ReadUInt16();

                    entry.Filename = $"file_{fileCount:D4}";
                    if (filenames != null && entry.FilenameIndex >= 0 && entry.FilenameIndex < filenames.Length)
                    {
                        entry.Filename = filenames[entry.FilenameIndex];
                    }
                    
                    // There are a lot of empty entries that we need to filter out
                    if (entry.FileSizeCompressed > 0 || entry.FileSizeRaw > 0)
                    {
                        entries.Add(entry);
                        fileCount++;
                    }
                }

                result.FileEntries = entries;


                // -------------------------------------------------------
                // File data
                // -------------------------------------------------------
                var cnt = 0;
                uint maxOffset = 0;
                foreach (var entry in result.FileEntries)
                {
                    if (entry.FileOffset > maxOffset)
                    {
                        maxOffset = entry.FileOffset;
                    }

                    if ((entry.FileSizeCompressed > 0 || entry.FileSizeRaw > 0) && entry.FileOffset > 100 && entry.FileSizeRaw != 0)
                    {
                        var size = entry.FileSizeCompressed == 0 ? entry.FileSizeRaw : entry.FileSizeCompressed;
                        reader.BaseStream.Seek(entry.FileOffset, SeekOrigin.Begin);
                        if (size > 0)
                        {
                            entry.Bytes = reader.ReadBytes(size);
                        }
                    }
                    cnt++;
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}