namespace DBCLib
{
    internal readonly struct DBCInfo
    {
        public readonly uint DBCRecords { get; }
        public readonly uint DBCFields { get; }
        public readonly uint RecordSize { get; }
        public readonly uint StringSize { get; }

        public DBCInfo(uint dbcRecords, uint dbcFields, uint recordSize, uint stringSize)
        {
            DBCRecords = dbcRecords;
            DBCFields = dbcFields;
            RecordSize = recordSize;
            StringSize = stringSize;
        }
    }
}
