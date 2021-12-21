using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBCLib
{
    internal static class DBCUtility
    {
        internal static int FieldCount(FieldInfo[] fields, Type type)
        {
            var instance = Activator.CreateInstance(type);
            var fieldCount = 0;
            foreach (var field in fields)
            {
                if (Type.GetTypeCode(field.FieldType) == TypeCode.Object)
                {
                    if (field.FieldType == typeof(LocalizedString))
                    {
                        fieldCount += LocalizedString.Size;
                    }
                    else if (field.FieldType.IsArray)
                    {
                        fieldCount += Type.GetTypeCode(field.FieldType.GetElementType()) switch
                        {
                            TypeCode.Int32 => ((int[])field.GetValue(instance)).Length,
                            TypeCode.UInt32 => ((uint[])field.GetValue(instance)).Length,
                            TypeCode.Single => ((float[])field.GetValue(instance)).Length,
                            _ => throw new NotImplementedException(Type.GetTypeCode(field.FieldType.GetElementType()).ToString()),
                        };
                    }
                }
                else
                    ++fieldCount;
            }

            return fieldCount;
        }

        internal static DBCInfo GetDBCInfo(BinaryReader reader)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader), "Reader cannot be null.");

            var info = new DBCInfo(
                dbcRecords: reader.ReadUInt32(),
                dbcFields: reader.ReadUInt32(),
                recordSize: reader.ReadUInt32(),
                stringSize: reader.ReadUInt32()
            );

            return info;
        }

        internal static Dictionary<int, string> GetStringTable(BinaryReader reader, DBCInfo info, long headerSize)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader), "Reader cannot be null.");

            // DBC records can contain strings. These strings are not stored in the record but in an additional string block, at the end of the file.
            // A record contains an offset into that block.
            // These stings are zero terminated (read: c strings) and might be zero length.
            reader.BaseStream.Position = info.DBCRecords * info.RecordSize + headerSize;
            var stringData = reader.ReadBytes((int)info.StringSize);
            string fullString = Encoding.UTF8.GetString(stringData);
            var strings = fullString.Split(new[] { '\0' }, StringSplitOptions.None);

            var stringTable = new Dictionary<int, string>();
            var currentPosition = 0;
            foreach (string str in strings)
            {
                stringTable.Add(currentPosition, str);
                currentPosition += Encoding.UTF8.GetByteCount(str) + 1;
            }

            return stringTable;
        }
    }
}
