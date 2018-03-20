using System.IO;
using System.Text;
using DBCLib.Exceptions;

namespace DBCLib
{
    public class DBCFile
    {
        public string FilePath { get; }
        public string Signature { get; }
        public bool IsLoaded { get; private set; }

        public DBCFile(string path, string signature)
        {
            FilePath = path;
            Signature = signature;
            IsLoaded = false;
        }

        public void LoadDBC()
        {
            // We don't need to load the file multiple times.
            if (IsLoaded)
                return;

            string path = FilePath;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                byte[] byteSignature = reader.ReadBytes(Signature.Length);
                string stringSignature = Encoding.UTF8.GetString(byteSignature);
                if (stringSignature != Signature)
                    throw new InvalidSignatureException(stringSignature);
            }

            // Todo: Load the DBC file

            IsLoaded = true;
        }

        public void SaveDBC()
        {
            string path = FilePath;

            // Todo: Save the DBC file
        }
    }
}
