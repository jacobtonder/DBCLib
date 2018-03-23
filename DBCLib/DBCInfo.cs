namespace DBCLib
{
    public struct DBCInfo
    {
        public uint DBCRecords { get; }
        public uint DBCFields { get; }
        public uint RecordSize { get; }
        public uint StringSize { get; }

        public DBCInfo(uint dbcRecords, uint dbcFields, uint recordSize, uint stringSize)
        {
            DBCRecords = dbcRecords;
            DBCFields = dbcFields;
            RecordSize = recordSize;
            StringSize = stringSize;
        }
    }
}
