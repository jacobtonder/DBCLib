using System;
using System.IO;
using System.Text;
using DBCLib.Test.Structures;

namespace DBCLib.Test.Helpers
{
    internal static class DbcTestData
    {
        internal static string CreateTempDbcFilePath() => Path.Combine(Path.GetTempPath(), $"dbcfiletest-{Guid.NewGuid():N}.dbc");

        internal static CharTitlesEntry CreateEntry(uint id)
        {
            return new CharTitlesEntry
            {
                Id = id,
                ConditionId = 0,
                NameMale = "Title %s",
                NameFemale = "Title %s",
                TitleMaskId = 1
            };
        }

        internal static void WriteCharTitlesDbc(string filePath, uint id)
        {
            using var stream = File.Create(filePath);
            using var writer = new BinaryWriter(stream);

            writer.Write(Encoding.UTF8.GetBytes("WDBC"));
            writer.Write((uint)1);
            writer.Write((uint)37);
            writer.Write((uint)(37 * 4));

            byte[] stringBlock = { 0, (byte)'M', (byte)'a', (byte)'l', (byte)'e', 0, (byte)'F', (byte)'e', (byte)'m', (byte)'a', (byte)'l', (byte)'e', 0 };
            writer.Write((uint)stringBlock.Length);

            writer.Write(id);
            writer.Write((uint)0);

            writer.Write(1);
            for (int i = 1; i < LocalizedString.Size - 1; ++i)
                writer.Write(0);
            writer.Write((uint)0);

            writer.Write(6);
            for (int i = 1; i < LocalizedString.Size - 1; ++i)
                writer.Write(0);
            writer.Write((uint)0);

            writer.Write((uint)1);
            writer.Write(stringBlock);
        }

        internal static void WriteEmptyCharTitlesDbc(string filePath)
        {
            using var stream = File.Create(filePath);
            using var writer = new BinaryWriter(stream);

            writer.Write(Encoding.UTF8.GetBytes("WDBC"));
            writer.Write((uint)0);
            writer.Write((uint)37);
            writer.Write((uint)(37 * 4));
            writer.Write((uint)1);
            writer.Write((byte)0);
        }
    }
}