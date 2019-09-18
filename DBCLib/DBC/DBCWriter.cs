using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBCLib
{
    internal class DBCWriter<T> where T : class, new()
    {
        private readonly Dictionary<int, int> stringHashes = new Dictionary<int, int>();
        private readonly Dictionary<int, string> stringTable = new Dictionary<int, string>();
        private KeyValuePair<int, string> lastItem;

        internal void WriteDBC(DBCFile<T> dbcFile, string path, string signature)
        {
            using (FileStream fileStream = File.OpenWrite(path))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // Sign the file with the signature
                byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
                writer.Write(signatureBytes);
                writer.Write(dbcFile.Records.Count);

                // Get fields of the DBC type and write to the DBC file
                Type dbcType = dbcFile.GetDBCType();
                FieldInfo[] fields = dbcType.GetFields();
                int fieldCount = dbcFile.FieldCount(fields, dbcType);
                writer.Write(fieldCount);
                writer.Write(fieldCount * 4);
                writer.Write(0);

                // Adding an empty string to obtain the correct size
                if (signature == "WDBC")
                    AddStringToDictionary(string.Empty);

                foreach (T record in dbcFile.Records)
                {
                    foreach (FieldInfo field in fields)
                    {
                        switch (Type.GetTypeCode(field.FieldType))
                        {
                            case TypeCode.Object:
                            {
                                if (field.FieldType == typeof(LocalizedString))
                                {
                                    int position = AddStringToDictionary((LocalizedString)field.GetValue(record));

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
                                    if (field.GetValue(record) is Array array)
                                    {
                                        int arrayLength = array.Length;

                                        switch (Type.GetTypeCode(field.FieldType.GetElementType()))
                                        {
                                            case TypeCode.Int32:
                                                for (int i = 0; i < arrayLength; ++i)
                                                    writer.Write((int)array.GetValue(i));
                                                break;
                                            case TypeCode.UInt32:
                                                for (int i = 0; i < arrayLength; ++i)
                                                    writer.Write((uint)array.GetValue(i));
                                                break;
                                            case TypeCode.Single:
                                                for (int i = 0; i < arrayLength; ++i)
                                                    writer.Write((float)array.GetValue(i));
                                                break;
                                            default:
                                                throw new NotImplementedException(Type.GetTypeCode(field.FieldType.GetElementType()).ToString());
                                        }
                                    }
                                }
                                break;
                            }
                            case TypeCode.Byte:
                            {
                                byte value = (byte)field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                            case TypeCode.Int32:
                            {
                                int value = (int)field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                            case TypeCode.UInt32:
                            {
                                uint value = (uint)field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                            case TypeCode.String:
                            {
                                string str = field.GetValue(record) as string;
                                writer.Write(AddStringToDictionary(str));
                                break;
                            }
                            case TypeCode.Single:
                            {
                                float value = (float)field.GetValue(record);
                                writer.Write(value);
                                break;
                            }
                            default:
                                throw new NotImplementedException(Type.GetTypeCode(field.FieldType).ToString());
                        }
                    }
                }

                // Write all of the strings to the dbc file
                foreach (string str in stringTable.Values)
                {
                    var stringTableBytes = Encoding.UTF8.GetBytes(str);
                    writer.Write(stringTableBytes);
                    writer.Write((byte)0);
                }

                // Todo: Allow for dynamic header size
                writer.BaseStream.Position = 16;
                if (stringTable.Count > 0)
                    writer.Write(stringTable.Last().Key + Encoding.UTF8.GetByteCount(stringTable.Last().Value) + 1);
            }
        }

        private int AddStringToDictionary(string str)
        {
            if (str == null)
                str = "";

            // Get the hash code of the string
            int strHash = str.GetHashCode();

            if (stringHashes.TryGetValue(strHash, out int position))
                return position;

            if (stringTable.Count > 0)
                position = lastItem.Key + Encoding.UTF8.GetByteCount(lastItem.Value) + 1;

            // Add the values to the dictionaries
            stringTable.Add(position, str);
            stringHashes.Add(strHash, position);
            lastItem = new KeyValuePair<int, string>(position, str);

            return position;
        }
    }
}
