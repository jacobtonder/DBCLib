using System.IO;

namespace DBCLib
{
    public class DBCFile
    {
        public string FilePath { get; }
        public bool IsLoaded { get; private set; }

        public DBCFile(string path)
        {
            FilePath = path;
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
