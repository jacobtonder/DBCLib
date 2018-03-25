using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBCLib.Test
{
    [TestClass]
    public class DBCInfoTest
    {
        private DBCInfo dbcInfo;

        [TestInitialize]
        public void Initialize()
        {
            dbcInfo = new DBCInfo(0, 1, 2, 3);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbcInfo = default(DBCInfo);
        }

        [TestMethod]
        public void DBCRecords_AreEqual()
        {
            Assert.AreEqual((uint)0, dbcInfo.DBCRecords);
        }

        [TestMethod]
        public void DBCFields_AreEqual()
        {
            Assert.AreEqual((uint)1, dbcInfo.DBCFields);
        }

        [TestMethod]
        public void RecordSize_AreEqual()
        {
            Assert.AreEqual((uint)2, dbcInfo.RecordSize);
        }

        [TestMethod]
        public void StringSize_AreEqual()
        {
            Assert.AreEqual((uint)3, dbcInfo.StringSize);
        }
    }
}
