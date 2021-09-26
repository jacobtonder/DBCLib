using Xunit;

namespace DBCLib.Test
{
    public class DBCInfoTest
    {
        [Fact]
        public void DBCRecords_Equal()
        {
            DBCInfo dbcInfo = new(0, 1, 2, 3);

            Assert.Equal((uint)0, dbcInfo.DBCRecords);
        }

        [Fact]
        public void DBCFields_Equal()
        {
            DBCInfo dbcInfo = new(0, 1, 2, 3);

            Assert.Equal((uint)1, dbcInfo.DBCFields);
        }

        [Fact]
        public void RecordSize_Equal()
        {
            DBCInfo dbcInfo = new(0, 1, 2, 3);

            Assert.Equal((uint)2, dbcInfo.RecordSize);
        }

        [Fact]
        public void StringSize_Equal()
        {
            DBCInfo dbcInfo = new(0, 1, 2, 3);

            Assert.Equal((uint)3, dbcInfo.StringSize);
        }
    }
}
