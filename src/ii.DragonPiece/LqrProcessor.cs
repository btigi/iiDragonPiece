using System.Text;

namespace ii.DragonPiece
{
    public class LqrFile
    {
        public List<string> SymbolicNames { get; set; } = new List<string>();
        public List<LqrFileEntry> FileEntries { get; set; } = new List<LqrFileEntry>();
    }

    public class LqrFileEntry
    {
        public int FileId { get; set; }
        public string Filename { get; set; } = string.Empty;
    }

    public class LqrProcessor
    {
        public LqrFile Process(string filename)
        {
            var result = new LqrFile();

            using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            var signature = reader.ReadInt32();
            if (signature != 108)
            {
                throw new InvalidDataException($"Invalid LQR signature: {signature}.");
            }

            var checksum = reader.ReadInt32(); // ?
            var valueOffset = reader.ReadInt64();
            var miscDataOffset = reader.ReadInt64();
            var unknownDataOffset = reader.ReadInt64();
            var symbolicNameOffset = reader.ReadInt64();
            var filenameOffset = reader.ReadInt64();

            // -------------------------------------------------------
            // Values
            // -------------------------------------------------------
            reader.BaseStream.Seek(valueOffset + 8, SeekOrigin.Begin);
            var valueCount = reader.ReadInt32();
            for (var j = 0; j < valueCount; j++)
            {
                var value = reader.ReadInt32();
            }

            // -------------------------------------------------------
            // Misc data
            // -------------------------------------------------------
            reader.BaseStream.Seek(miscDataOffset, SeekOrigin.Begin);
            for (var j = 0; j < 8; j++)
            {
                var value = reader.ReadInt16();
            }
            var textLength = reader.ReadInt32();
            var textBlock = reader.ReadBytes(textLength * 2);
            var text1 = Encoding.Unicode.GetString(textBlock);
            _ = reader.ReadByte();
            textLength = reader.ReadInt32();
            textBlock = reader.ReadBytes(textLength * 2);
            var text2 = Encoding.Unicode.GetString(textBlock);

            // and some unknown data here

            // -------------------------------------------------------
            // Unknown data
            // -------------------------------------------------------
            reader.BaseStream.Seek(unknownDataOffset, SeekOrigin.Begin);

            // -------------------------------------------------------
            // Symbolic names
            // -------------------------------------------------------
            reader.BaseStream.Seek(symbolicNameOffset, SeekOrigin.Begin);
            var symbolicNameCount = reader.ReadInt32();
            var symbolicNameBlockLength = reader.ReadInt32();
            var symbolicNameBlock = reader.ReadBytes(symbolicNameBlockLength);
            var symbolicNames = Encoding.Unicode.GetString(symbolicNameBlock);
            result.SymbolicNames = symbolicNames.Split('\0').ToList();

            // -------------------------------------------------------
            // Filenames
            // -------------------------------------------------------
            if (miscDataOffset - filenameOffset > 28)
            {
                reader.BaseStream.Seek(filenameOffset, SeekOrigin.Begin);
                var entryCount = reader.ReadInt32();
                var entryCount2 = reader.ReadInt32();

                var i = 0;
                byte resourceType = 0;
                try
                {
                    var amt = 20;

                    if (filename.ToLower().Contains("speechfx") || filename.ToLower().Contains("text") || filename.ToLower().Contains("levels") || filename.ToLower().Contains("dialog"))
                    {
                        amt = 20;
                    }
                    if (filename.ToLower().Contains("projectiles") || filename.ToLower().Contains("skyboxes"))
                    {
                        amt = 24;
                    }
                    if (filename.ToLower().Contains("fonts"))
                    {
                        amt = 16;
                    }
                    if (filename.ToLower().Contains("actors"))
                    {
                        amt = 28;
                    }

                    for (i = 0; i < entryCount2; i++)
                    {
                        var fileId = reader.ReadInt32();
                        var cnt = reader.ReadByte();
                        if (cnt == 0)
                        {
                            continue;
                        }

                        if (cnt == 1)
                        {
                            var rtHandled = false;

                            resourceType = reader.ReadByte();
                            if (resourceType == 0)
                            {
                                rtHandled = true;
                                var length = reader.ReadInt32();
                                if (length == 0)
                                {
                                    _ = reader.ReadInt32();
                                }
                                else
                                {
                                    var thisfilename = Encoding.Unicode.GetString(reader.ReadBytes(length * 2)).TrimEnd('\0');
                                    var fileEntry = new LqrFileEntry
                                    {
                                        FileId = fileId,
                                        //Filename = thisfilename + "_" + Convert.ToString(reader.BaseStream.Position)
                                        Filename = thisfilename
                                    };
                                    result.FileEntries.Add(fileEntry);
                                    reader.ReadBytes(amt);
                                }
                            }
                            else
                            //if (resourceType == 10 || resourceType == 11 || resourceType == 12 || resourceType == 13 || resourceType == 14 || resourceType == 19 || resourceType == 20 ||
                            //resourceType == 23 || resourceType == 25 || resourceType == 35 || resourceType == 39 || resourceType == 51 || resourceType == 57 || resourceType == 60 ||
                            //resourceType == 64 || resourceType == 66 || resourceType == 1 || resourceType == 44 || resourceType == 34 || resourceType == 24 || resourceType == 126 || true)
                            {
                                rtHandled = true;
                                _ = reader.ReadInt32();
                                _ = reader.ReadInt32();
                                var length = reader.ReadInt32();
                                if (length == 0)
                                {
                                    _ = reader.ReadInt32();
                                }
                                else
                                {
                                    var thisfilename = Encoding.Unicode.GetString(reader.ReadBytes(length * 2)).TrimEnd('\0');
                                    var fileEntry = new LqrFileEntry
                                    {
                                        FileId = fileId,
                                        //Filename = thisfilename + "_" + Convert.ToString(reader.BaseStream.Position)
                                        Filename = thisfilename
                                    };
                                    result.FileEntries.Add(fileEntry);
                                    reader.ReadBytes(amt);
                                }
                            }

                            if (!rtHandled)
                                Console.WriteLine(resourceType);

                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return result;
        }
    }
}