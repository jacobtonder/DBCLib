using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBCLib
{
    internal class DBCWriter<T> where T : class, new()
    {
        internal void WriteDBC(DBCFile<T> dbcFile, string path, string signature)
        {
            using (FileStream fileStream = File.OpenWrite(path))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // Sign the file with the signture
                byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
                writer.Write(signatureBytes);
                writer.Write(dbcFile.Records.Count);

                // Get fields of the dbc type and write to the dbc file
                Type dbcType = dbcFile.DBCType;
                FieldInfo[] fields = dbcType.GetFields();
                int fieldCount = dbcFile.FieldCount(fields, dbcType);
                writer.Write(fieldCount);
                writer.Write(fieldCount * 4);
                writer.Write(0);

                foreach(T record in dbcFile.Records)
                {
                    foreach (FieldInfo field in fields)
                    {
                        switch (Type.GetTypeCode(field.FieldType))
                        {
                            case TypeCode.Object:
                            {
                                if (field.FieldType == typeof(LocalizedString))
                                {
                                    // TODO
                                }
                                else
                                {
                                    Array array = field.GetValue(record) as Array;
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
                                // TODO
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
            }
        }
    }
}
