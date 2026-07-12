using Xunit;

namespace DBCLib.Test
{
    public class DBCInfoTest
    {
        [Theory]
        [InlineData((uint)0, (uint)1, (uint)2, (uint)3)]
        [InlineData((uint)10, (uint)20, (uint)30, (uint)40)]
        [InlineData(uint.MaxValue, (uint)0, (uint)148, (uint)1)]
        public void Constructor_AssignsAllProperties(uint dbcRecords, uint dbcFields, uint recordSize, uint stringSize)
        {
            DBCInfo dbcInfo = new(dbcRecords, dbcFields, recordSize, stringSize);

            Assert.Equal(dbcRecords, dbcInfo.DBCRecords);
            Assert.Equal(dbcFields, dbcInfo.DBCFields);
            Assert.Equal(recordSize, dbcInfo.RecordSize);
            Assert.Equal(stringSize, dbcInfo.StringSize);
        }
    }
}
