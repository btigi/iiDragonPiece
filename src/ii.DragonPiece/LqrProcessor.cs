using System.Text;

namespace ii.DragonPiece
{
    public class LqrProcessor
    {
        public string[] Process(string filename)
        {
            try
            {
                using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(fileStream);

                var signature = reader.ReadBytes(4);
                var a = reader.ReadInt32();  // ?

                var b = reader.ReadInt32();  // 40
                var c = reader.ReadInt32();  // 0

                var d = reader.ReadInt32();  // offset to something2 -> 20 bytes then the name of the associated h2o file (text length, upper case, text length, lower case)?
                var e = reader.ReadInt32();  // 0

                var f = reader.ReadInt32();  // offset to something3
                var g = reader.ReadInt32();  // 0

                var h = reader.ReadInt32();  // offset to symbolic names (end of file)
                var ii = reader.ReadInt32(); // 0 

                var j = reader.ReadInt32();  // 80 / offset to filenames (18 bytes before first text)
                var k = reader.ReadInt32();  // 0


                // Symbolic names ?
                reader.BaseStream.Seek(h, SeekOrigin.Begin);
                var count = reader.ReadInt32();
                var lengthofblock = reader.ReadInt32();
                var blockbytes = reader.ReadBytes(lengthofblock);
                var parts = Encoding.Unicode.GetString(blockbytes);
                var splits = parts.Split('\0');

                // Filenames
                reader.BaseStream.Seek(j, SeekOrigin.Begin);

                var entryCount = reader.ReadInt32();
                var entryCount2 = reader.ReadInt32();

                var filenames = new string[entryCount2];

                var i = 0;
                try
                {
                    var amt = 20;

                    if (filename.ToLower().Contains("speechfx") || filename.ToLower().Contains("text") || filename.ToLower().Contains("levels"))
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

                    for (i = 0; i < entryCount2; i++)
                    {
                        var fileId = reader.ReadInt32();
                        //_ = reader.ReadInt16();
                        var cnt = reader.ReadByte();
                        if (cnt == 0)
                        {
                            continue;
                        }

                        if (cnt == 1)
                        {
                            _ = reader.ReadByte();
                            var length = reader.ReadInt32();
                            var thisfilename = Encoding.Unicode.GetString(reader.ReadBytes(length * 2)).TrimEnd('\0');
                            filenames[i] = thisfilename + "_" + Convert.ToString(reader.BaseStream.Position);
                            _ = reader.ReadBytes(amt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading .lqr file: {ex.Message}");
                    return null;
                }
                return filenames;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}