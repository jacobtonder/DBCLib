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

        public void LoadData()
        {
            if (IsLoaded)
                return;

            IsLoaded = true;

            return;
        }
    }
}
