using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBCLib.Test
{
    [TestClass]
    public class DBCFileTest
    {
        private DBCFile<DBCFileTest> dbcFile;

        [TestInitialize]
        public void Initialize()
        {
            dbcFile = new DBCFile<DBCFileTest>("//path//", "signature");
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbcFile = null;
        }

        [TestMethod]
        public void DBCType_AreEqual()
        {
            Assert.AreEqual(typeof(DBCFileTest), dbcFile.DBCType);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("     ")]
        [DataRow("            ")]
        [DataRow(null)]
        public void Constructor_Path_ThrowsArgumentNullException(string path)
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DBCFile<DBCFileTest>(path, "signature"));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void Constructor_Signature_ThrowsArgumentNullException(string signature)
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DBCFile<DBCFileTest>("//path//", signature));
        }

        [TestMethod]
        public void RemoveEntry_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => dbcFile.RemoveEntry(1));
        }

        [TestMethod]
        public void ReplaceEntry_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => dbcFile.ReplaceEntry(1, new DBCFileTest()));
        }
    }
}
