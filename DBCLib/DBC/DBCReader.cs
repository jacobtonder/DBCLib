using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DBCLib.Exceptions;

namespace DBCLib
{
    internal static class DBCReader<T> where T : class, new()
    {
        private static readonly Type DbcType = typeof(T);
        private static readonly FieldInfo[] Fields = DbcType.GetFields();
        private static readonly ReaderFieldMetadata[] FieldMetadata = CreateFieldMetadata(Fields);
        private static readonly int ExpectedFieldCount = DBCUtility.FieldCount(Fields, DbcType);

        internal static void ReadDBC(DBCFile<T> dbcFile, BinaryReader reader)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));

            var info = DBCUtility.GetDBCInfo(reader);

            // Validate the DBC fields
            if (info.DBCFields != ExpectedFieldCount)
                throw new InvalidDBCFieldsException(DbcType.ToString());

            // We don't need to read the first bytes again (signature)
            long headerSize = reader.BaseStream.Position;

            // Extract all strings and construct string table
            var stringTable = DBCUtility.GetStringTable(reader, info, headerSize);

            // Reset position to base position
            reader.BaseStream.Position = headerSize;

            // Loop through all of the records in the DBC file
            for (uint i = 0; i < info.DBCRecords; ++i)
            {
                var instance = new T();

                foreach (var metadata in FieldMetadata)
                {
                    switch (metadata.TypeCode)
                    {
                        case TypeCode.Object:
                        {
                            if (metadata.IsLocalizedString)
                            {
                                var value = "";
                                for (uint j = 0; j < LocalizedString.Size - 1; ++j)
                                {
                                    int offsetKey = reader.ReadInt32();
                                    if (string.IsNullOrEmpty(value) && offsetKey != 0 && stringTable.TryGetValue(offsetKey, out string stringFromTable))
                                    {
                                        value = stringFromTable;
                                        dbcFile.LocalPosition = j;
                                    }
                                }

                                dbcFile.LocalFlag = reader.ReadUInt32();

                                metadata.Field.SetValue(instance, (LocalizedString)value);
                            }
                            else if (metadata.IsArray)
                            {
                                Array array = metadata.ElementTypeCode switch
                                {
                                    TypeCode.Int32 => ReadInt32Array(reader, metadata.ArrayLength),
                                    TypeCode.UInt32 => ReadUInt32Array(reader, metadata.ArrayLength),
                                    TypeCode.Single => ReadSingleArray(reader, metadata.ArrayLength),
                                    _ => throw new NotImplementedException(metadata.ElementTypeCode.ToString())
                                };

                                metadata.Field.SetValue(instance, array);
                            }
                            break;
                        }
                        case TypeCode.Byte:
                        {
                            byte value = reader.ReadByte();
                            metadata.Field.SetValue(instance, value);
                            break;
                        }
                        case TypeCode.Int32:
                        {
                            int value = reader.ReadInt32();
                            metadata.Field.SetValue(instance, value);
                            break;
                        }
                        case TypeCode.UInt32:
                        {
                            uint value = reader.ReadUInt32();
                            metadata.Field.SetValue(instance, value);
                            break;
                        }
                        case TypeCode.Single:
                        {
                            float value = reader.ReadSingle();
                            metadata.Field.SetValue(instance, value);
                            break;
                        }
                        case TypeCode.String:
                        {
                            // Get offset for string table
                            int offsetKey = reader.ReadInt32();

                            // Check if offset exists in the string table
                            if (!stringTable.TryGetValue(offsetKey, out string stringFromTable))
                                throw new KeyNotFoundException(offsetKey.ToString());

                            string value = stringFromTable;
                            metadata.Field.SetValue(instance, value);
                            break;
                        }
                        default:
                            throw new NotImplementedException(metadata.TypeCode.ToString());
                    }
                }

                // Get the first value of the record and use that as the key for the DBC record
                var firstValue = Fields[0].GetValue(instance);
                var key = (uint)Convert.ChangeType(firstValue, typeof(uint));
                dbcFile.AddLoadedEntry(key, instance);
            }
        }

        private static ReaderFieldMetadata[] CreateFieldMetadata(FieldInfo[] fields)
        {
            var template = new T();
            var metadata = new ReaderFieldMetadata[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                var typeCode = Type.GetTypeCode(field.FieldType);
                var isArray = field.FieldType.IsArray;
                var elementTypeCode = isArray ? Type.GetTypeCode(field.FieldType.GetElementType()) : default;
                var arrayLength = isArray ? ((Array)field.GetValue(template)).Length : 0;

                metadata[i] = new ReaderFieldMetadata(
                    field,
                    typeCode,
                    field.FieldType == typeof(LocalizedString),
                    isArray,
                    elementTypeCode,
                    arrayLength);
            }

            return metadata;
        }

        private static int[] ReadInt32Array(BinaryReader reader, int length)
        {
            var array = new int[length];
            for (var i = 0; i < length; ++i)
                array[i] = reader.ReadInt32();
            return array;
        }

        private static uint[] ReadUInt32Array(BinaryReader reader, int length)
        {
            var array = new uint[length];
            for (var i = 0; i < length; ++i)
                array[i] = reader.ReadUInt32();
            return array;
        }

        private static float[] ReadSingleArray(BinaryReader reader, int length)
        {
            var array = new float[length];
            for (var i = 0; i < length; ++i)
                array[i] = reader.ReadSingle();
            return array;
        }

        private readonly struct ReaderFieldMetadata
        {
            internal ReaderFieldMetadata(FieldInfo field, TypeCode typeCode, bool isLocalizedString, bool isArray, TypeCode elementTypeCode, int arrayLength)
            {
                Field = field;
                TypeCode = typeCode;
                IsLocalizedString = isLocalizedString;
                IsArray = isArray;
                ElementTypeCode = elementTypeCode;
                ArrayLength = arrayLength;
            }

            internal FieldInfo Field { get; }
            internal TypeCode TypeCode { get; }
            internal bool IsLocalizedString { get; }
            internal bool IsArray { get; }
            internal TypeCode ElementTypeCode { get; }
            internal int ArrayLength { get; }
        }
    }
}
