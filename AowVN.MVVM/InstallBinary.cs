using System.IO;
using System.Threading.Tasks;
using System;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.IO;

namespace AowVN.MVVM
{
    internal static class InstallBinary
    {
        public static double Process = 0;
        public static async Task ReadBinaryAndWriteFolder(string outputFolder)
        {
            //using (MemoryStream stream = new MemoryStream(AowVN.MVVM.Resource.AowVN))
            using (FileStream stream = new FileStream(Path.Combine(outputFolder, ConstantStrings.FolderData, "AowVN.aow"), FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Read and write file bytes (consider chunking for large files)
                        Process = ((double)reader.BaseStream.Position / reader.BaseStream.Length) * 100;
                        {

                            string fileName = reader.ReadString();
                            Console.WriteLine($"{fileName}");
                            //byte[] properties = reader.ReadBytes(5);

                            long decompressedSize = reader.ReadInt64();
                            int compressSize = reader.ReadInt32();

                            byte[] temp = new byte[4];
                            reader.Read(temp, 0, temp.Length);

                            byte[] compressedData = reader.ReadBytes(compressSize);


                            //Console.WriteLine(fileName);
                            string filePath = Path.Combine(outputFolder, fileName);
                            string fileDirectory = Path.GetDirectoryName(filePath);
                            if (!Directory.Exists(fileDirectory))
                            {
                                Directory.CreateDirectory(fileDirectory);
                            }

                            using (MemoryStream compressedStream = new MemoryStream(compressedData))
                            using (FileStream outputStream = File.Create(filePath))
                            using (ZlibStream zlibStream = new ZlibStream(NonDisposingStream.Create(compressedStream), CompressionMode.Decompress))
                            {

                                int bufferSize = 4096; // Adjust as needed
                                byte[] buffer = new byte[bufferSize];
                                int bytesRead;
                                while ((bytesRead = zlibStream.Read(buffer, 0, bufferSize)) > 0)
                                {
                                    outputStream.Write(buffer, 0, bytesRead);
                                    outputStream.Flush();
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}
