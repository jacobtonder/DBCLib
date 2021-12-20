using System;
using System.IO;
using System.Reflection;

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

        static public DBCInfo GetDBCInfo(BinaryReader reader)
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
    }
}
