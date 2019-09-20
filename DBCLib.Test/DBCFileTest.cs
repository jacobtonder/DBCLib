using System;
using System.Reflection;
using DBCLib.Test.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBCLib.Test
{
    [TestClass]
    public class DBCFileTest
    {
        private DBCFile<CharTitlesEntry> dbcFile;

        [TestInitialize]
        public void Initialize()
        {
            dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbcFile = null;
        }

        [TestMethod]
        public void DBCType_AreEqual()
        {
            Assert.AreEqual(typeof(CharTitlesEntry), dbcFile.GetDBCType());
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("     ")]
        [DataRow("            ")]
        [DataRow(null)]
        public void Constructor_Path_ThrowsArgumentNullException(string path)
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>(path, "signature"));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void Constructor_Signature_ThrowsArgumentNullException(string signature)
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>("//path//", signature));
        }

        [TestMethod]
        public void FieldCount_AreEqual()
        {
            var fields = dbcFile.GetDBCType().GetFields();

            // Calculate field counts of DBC file
            int fieldCounts = dbcFile.FieldCount(fields, dbcFile.GetDBCType());

            Assert.AreEqual(37, fieldCounts);
        }

        [TestMethod]
        public void RemoveEntry_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => dbcFile.RemoveEntry(1));
        }

        [TestMethod]
        public void ReplaceEntry_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => dbcFile.ReplaceEntry(1, new CharTitlesEntry()));
        }
    }
}
