using System;
using Xunit;
using DBCLib.Test.Structures;

namespace DBCLib.Test
{
    public class DBCFileTest
    {
        [Fact]
        public void DBCType_Equal()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

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
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

            Assert.Throws<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>(path, "signature"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_Signature_ThrowsArgumentNullException(string signature)
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

            Assert.Throws<ArgumentNullException>(() => new DBCFile<CharTitlesEntry>("//path//", signature));
        }

        [Fact]
        public void FieldCount_Equal()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");
            var fields = dbcFile.GetDBCType().GetFields();

            // Calculate field counts of DBC file
            int fieldCounts = dbcFile.FieldCount(fields, dbcFile.GetDBCType());

            Assert.Equal(37, fieldCounts);
        }

        [Fact]
        public void RemoveEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

            Assert.Throws<ArgumentException>(() => dbcFile.RemoveEntry(1));
        }

        [Fact]
        public void ReplaceEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

            Assert.Throws<ArgumentException>(() => dbcFile.ReplaceEntry(1, new CharTitlesEntry()));
        }

        [Fact]
        public void AddEntry_Contains()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");
            CharTitlesEntry charTitlesEntry = new CharTitlesEntry
            {
                Id = 1,
                NameMale = "Title %s",
                NameFemale = "Title %s",
                TitleMaskId = 1
            };

            dbcFile.AddEntry(1, charTitlesEntry);

            Assert.Contains(charTitlesEntry, dbcFile.Records);
        }

        [Fact]
        public void AddEntry_ThrowsArgumentException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");
            CharTitlesEntry charTitlesEntry = new CharTitlesEntry
            {
                Id = 1,
                NameMale = "Title %s",
                NameFemale = "Title %s",
                TitleMaskId = 1
            };

            dbcFile.AddEntry(1, charTitlesEntry);

            Assert.Throws<ArgumentException>(() => dbcFile.AddEntry(1, new CharTitlesEntry()));
        }

        [Fact]
        public void AddEntry_ThrowsNullException()
        {
            DBCFile<CharTitlesEntry> dbcFile = new DBCFile<CharTitlesEntry>("//path//", "signature");

            Assert.Throws<ArgumentNullException>(() => dbcFile.AddEntry(1, null));
        }
    }
}
