using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBCLib
{
    internal class DBCWriter<T> where T : class, new()
    {
        private static readonly Type DbcType = typeof(T);
        private static readonly FieldInfo[] Fields = DbcType.GetFields();
        private static readonly WriterFieldMetadata[] FieldMetadata = CreateFieldMetadata(Fields);

        private readonly Dictionary<string, int> stringPositions = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<int, string> stringTable = new Dictionary<int, string>();
        private KeyValuePair<int, string> lastItem;

        internal void WriteDBC(DBCFile<T> dbcFile, string path, string signature)
        {
            using var fileStream = File.Create(path);
            using var writer = new BinaryWriter(fileStream);
            // Sign the file with the signature
            var signatureBytes = Encoding.UTF8.GetBytes(signature);
            writer.Write(signatureBytes);
            writer.Write(dbcFile.Records.Count);

            // Get fields of the DBC type and write to the DBC file
            int fieldCount = DBCUtility.FieldCount(Fields, DbcType);
            writer.Write(fieldCount);
            writer.Write(fieldCount * 4);
            writer.Write(0);

            // Adding an empty string to obtain the correct size
            if (signature == "WDBC")
                AddStringToDictionary(string.Empty);

            foreach (var record in dbcFile.Records)
            {
                foreach (var metadata in FieldMetadata)
                {
                    switch (metadata.TypeCode)
                    {
                        case TypeCode.Object:
                            {
                                if (metadata.IsLocalizedString)
                                {
                                    int position = AddStringToDictionary((LocalizedString)metadata.Field.GetValue(record));

                                    // Local strings before the local position
                                    for (uint i = 0; i < dbcFile.LocalPosition; ++i)
                                    {
                                        writer.Write(0);
                                    }

                                    // Write to the Local Position
                                    writer.Write(position);

                                    // Local strings after the local position
                                    for (uint j = dbcFile.LocalPosition + 1; j < LocalizedString.Size - 1; ++j)
                                        writer.Write(0);

                                    // 17th location field
                                    writer.Write(dbcFile.LocalFlag);
                                }
                                else
                                {
                                    if (metadata.Field.GetValue(record) is Array array)
                                    {
                                        int arrayLength = array.Length;

                                        switch (metadata.ElementTypeCode)
                                        {
                                            case TypeCode.Int32:
                                                for (var i = 0; i < arrayLength; ++i)
                                                    writer.Write((int)array.GetValue(i));
                                                break;
                                            case TypeCode.UInt32:
                                                for (var i = 0; i < arrayLength; ++i)
                                                    writer.Write((uint)array.GetValue(i));
                                                break;
                                            case TypeCode.Single:
                                                for (var i = 0; i < arrayLength; ++i)
                                                    writer.Write((float)array.GetValue(i));
                                                break;
                                            default:
                                                throw new NotImplementedException(metadata.ElementTypeCode.ToString());
                                        }
                                    }
                                }
                                break;
                            }
                        case TypeCode.Byte:
                            {
                                var value = (byte)metadata.Field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                        case TypeCode.Int32:
                            {
                                var value = (int)metadata.Field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                        case TypeCode.UInt32:
                            {
                                var value = (uint)metadata.Field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                        case TypeCode.String:
                            {
                                var str = metadata.Field.GetValue(record) as string;
                                writer.Write(AddStringToDictionary(str));
                                break;
                            }
                        case TypeCode.Single:
                            {
                                var value = (float)metadata.Field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                        default:
                            throw new NotImplementedException(metadata.TypeCode.ToString());
                    }
                }
            }

            // Write all of the strings to the DBC file
            foreach (var str in stringTable.Values)
            {
                var stringTableBytes = Encoding.UTF8.GetBytes(str);
                writer.Write(stringTableBytes);
                writer.Write((byte)0);
            }

            // TODO: Allow for dynamic header size
            writer.BaseStream.Position = 16;
            if (stringTable.Count > 0)
                writer.Write(lastItem.Key + Encoding.UTF8.GetByteCount(lastItem.Value) + 1);
        }

        private int AddStringToDictionary(string str)
        {
            str ??= string.Empty;

            // Reuse the same offset for duplicate strings.
            if (stringPositions.TryGetValue(str, out int position))
                return position;

            if (stringTable.Count > 0)
                position = lastItem.Key + Encoding.UTF8.GetByteCount(lastItem.Value) + 1;

            // Add the values to the dictionaries
            stringTable.Add(position, str);
            stringPositions.Add(str, position);
            lastItem = new KeyValuePair<int, string>(position, str);

            return position;
        }

        private static WriterFieldMetadata[] CreateFieldMetadata(FieldInfo[] fields)
        {
            var metadata = new WriterFieldMetadata[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                var typeCode = Type.GetTypeCode(field.FieldType);
                var isLocalizedString = field.FieldType == typeof(LocalizedString);
                var elementTypeCode = field.FieldType.IsArray ? Type.GetTypeCode(field.FieldType.GetElementType()) : default;
                metadata[i] = new WriterFieldMetadata(field, typeCode, isLocalizedString, elementTypeCode);
            }

            return metadata;
        }

        private readonly struct WriterFieldMetadata
        {
            internal WriterFieldMetadata(FieldInfo field, TypeCode typeCode, bool isLocalizedString, TypeCode elementTypeCode)
            {
                Field = field;
                TypeCode = typeCode;
                IsLocalizedString = isLocalizedString;
                ElementTypeCode = elementTypeCode;
            }

            internal FieldInfo Field { get; }
            internal TypeCode TypeCode { get; }
            internal bool IsLocalizedString { get; }
            internal TypeCode ElementTypeCode { get; }
        }
    }
}
