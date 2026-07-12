using System;
using System.IO;
using Xunit;
using DBCLib.Test.Structures;
using DBCLib.Exceptions;
using DBCLib.Test.Helpers;

namespace DBCLib.Test
{
    public class DBCFileTest
    {
        [Fact]
        public void DBCType_Equal()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Equal(typeof(CharTitlesEntry), dbcFile.GetDBCType());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [InlineData("            ")]
        [InlineData(null)]
        public void Constructor_Path_ThrowsArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>(path, "signature"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_Signature_ThrowsArgumentNullException(string signature)
        {
            Assert.Throws<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>("//path//", signature));
        }

        [Fact]
        public void FieldCount_Equal()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");
            var fields = dbcFile.GetDBCType().GetFields();

            // Calculate field counts of DBC file
            int fieldCounts = DBCUtility.FieldCount(fields, dbcFile.GetDBCType());

            Assert.Equal(37, fieldCounts);
        }

        [Fact]
        public void Records_ThrowsDBCFileNotLoadedException_WhenNotLoaded()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Throws<DBCFileNotLoadedException>(() => _ = dbcFile.Records);
        }

        [Fact]
        public void MaxKey_ThrowsDBCFileNotLoadedException_WhenNotLoaded()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Throws<DBCFileNotLoadedException>(() => _ = dbcFile.MaxKey);
        }

        [Fact]
        public void Load_WithSingleRecord_PopulatesRecordsAndMaxKey()
        {
            string filePath = DbcTestData.CreateTempDbcFilePath();
            try
            {
                DbcTestData.WriteCharTitlesDbc(filePath, 42);
                DBCFile<CharTitlesEntry> dbcFile = new(filePath);

                dbcFile.Load();

                Assert.Single(dbcFile.Records);
                Assert.Equal((uint)42, dbcFile.MaxKey);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public void MaxKey_Updates_WhenAddingAndRemovingEntries()
        {
            string filePath = DbcTestData.CreateTempDbcFilePath();
            try
            {
                DbcTestData.WriteCharTitlesDbc(filePath, 1);
                DBCFile<CharTitlesEntry> dbcFile = new(filePath);
                dbcFile.Load();

                dbcFile.AddEntry(10, DbcTestData.CreateEntry(10));
                dbcFile.AddEntry(5, DbcTestData.CreateEntry(5));
                Assert.Equal((uint)10, dbcFile.MaxKey);

                dbcFile.RemoveEntry(10);
                Assert.Equal((uint)5, dbcFile.MaxKey);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public void Save_PersistsAddedRecord()
        {
            string filePath = DbcTestData.CreateTempDbcFilePath();
            try
            {
                DbcTestData.WriteCharTitlesDbc(filePath, 1);

                DBCFile<CharTitlesEntry> dbcFile = new(filePath);
                dbcFile.Load();
                dbcFile.AddEntry(2, DbcTestData.CreateEntry(2));
                dbcFile.Save();

                DBCFile<CharTitlesEntry> reloaded = new(filePath);
                reloaded.Load();

                Assert.Equal(2, reloaded.Records.Count);
                Assert.Equal((uint)2, reloaded.MaxKey);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public void MaxKey_ThrowsInvalidOperationException_WhenLoadedFileHasNoRecords()
        {
            string filePath = DbcTestData.CreateTempDbcFilePath();
            try
            {
                DbcTestData.WriteEmptyCharTitlesDbc(filePath);
                DBCFile<CharTitlesEntry> dbcFile = new(filePath);
                dbcFile.Load();

                Assert.Throws<InvalidOperationException>(() => _ = dbcFile.MaxKey);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public void RemoveEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Throws<ArgumentException>(() => dbcFile.RemoveEntry(1));
        }

        [Fact]
        public void ReplaceEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Throws<ArgumentException>(() => dbcFile.ReplaceEntry(1, new CharTitlesEntry()));
        }

        [Fact]
        public void ReplaceEntry_ThrowsArgumentNullException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");
            dbcFile.AddEntry(1, new CharTitlesEntry());

            Assert.Throws<ArgumentNullException>(() => dbcFile.ReplaceEntry(1, null));
        }

        [Fact]
        public void AddEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");
            CharTitlesEntry charTitlesEntry = DbcTestData.CreateEntry(1);

            dbcFile.AddEntry(1, charTitlesEntry);

            Assert.Throws<ArgumentException>(() => dbcFile.AddEntry(1, new CharTitlesEntry()));
        }

        [Fact]
        public void AddEntry_ThrowsArgumentNullException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new("//path//", "signature");

            Assert.Throws<ArgumentNullException>(() => dbcFile.AddEntry(1, null));
        }

    }
}
