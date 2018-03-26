using System;
using System.IO;
using System.Text;

namespace DBCLib
{
    internal class DBCWriter<T> where T : class, new()
    {
        internal void WriteDBC(DBCFile<T> dbcFile, string path, string signature)
        {
            using (FileStream fileStream = File.OpenWrite(path))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // Sign the file with the signture
                byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
                writer.Write(signatureBytes);
            }
        }
    }
}
