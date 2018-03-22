using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DBCLib.Exceptions;

namespace DBCLib
{
    public class DBCFile<T> where T : class, new()
    {
        private Dictionary<uint, T> records = new Dictionary<uint, T>();

        public DBCFile(string path, string signature)
        {
            FilePath = path;
            Signature = signature;
            DBCType = typeof(T);
            IsLoaded = false;
        }

        public string FilePath { get; }
        public string Signature { get; }
        public Type DBCType { get; }
        public bool IsLoaded { get; private set; }
        public uint LocaleFlag { get; private set; }
        public Dictionary<uint, T>.ValueCollection Records { get => records.Values; }

        public void LoadDBC()
        {
            // We don't need to load the file multiple times.
            if (IsLoaded)
                return;
            
            using (BinaryReader reader = new BinaryReader(File.OpenRead(FilePath)))
            {
                byte[] byteSignature = reader.ReadBytes(Signature.Length);
                string stringSignature = Encoding.UTF8.GetString(byteSignature);
                if (stringSignature != Signature)
                    throw new InvalidSignatureException(stringSignature);

                FieldInfo[] fields = DBCType.GetFields();

                uint dbcRecords = reader.ReadUInt32();
                uint dbcFields = reader.ReadUInt32();
                uint recordSize = reader.ReadUInt32();
                uint stringSize = reader.ReadUInt32();

                // Set position of reader
                reader.BaseStream.Position = dbcRecords * recordSize + 20;

                byte[] stringData = reader.ReadBytes((int)stringSize);
                string fullString = Encoding.UTF8.GetString(stringData);
                string[] strings = fullString.Split(new string[] { "\0" }, StringSplitOptions.None);

                Dictionary<int, string> stringTable = new Dictionary<int, string>();
                int currentPosition = 0;
                foreach (string s in strings)
                {
                    stringTable.Add(currentPosition, s);
                    currentPosition += Encoding.UTF8.GetByteCount(s) + 1;
                }

                // Reset position
                reader.BaseStream.Position = 20;

                // Loop through all of the records in the DBC file
                for (uint i = 0; i < dbcRecords; ++i)
                {
                    Object dbcObject = Activator.CreateInstance(DBCType);

                    foreach (FieldInfo field in fields)
                    {
                        switch (Type.GetTypeCode(field.FieldType))
                        {
                            case TypeCode.Object:
                            {
                                if (field.FieldType == typeof(LocalizedString))
                                {
                                    string value = "";
                                    for (uint j = 0; j < LocalizedString.Size; ++j)
                                    {
                                        int offsetKey = reader.ReadInt32();
                                        if (value == "" && offsetKey != 0 && stringTable.TryGetValue(offsetKey, out string stringFromTable))
                                        {
                                            value = stringFromTable;
                                        }
                                    }

                                    LocaleFlag = reader.ReadUInt32();

                                    field.SetValue(dbcObject, (LocalizedString)value);
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    Array array;
                                    int arrayLength;

                                    switch (Type.GetTypeCode(field.FieldType.GetElementType()))
                                    {
                                        case TypeCode.Int32:
                                            // Get length of array
                                            arrayLength = ((int[])field.GetValue(dbcObject)).Length;

                                            // Set Array
                                            array = new int[arrayLength];

                                            // Set Value of DBC object by looping through the array
                                            for (int j = 0; j < arrayLength; ++j)
                                                array.SetValue(reader.ReadInt32(), j);
                                            field.SetValue(dbcObject, array);
                                            break;
                                        case TypeCode.UInt32:
                                            // Get length of array
                                            arrayLength = ((uint[])field.GetValue(dbcObject)).Length;

                                            // Set Array
                                            array = new uint[arrayLength];

                                            // Set Value of DBC object by looping through the array
                                            for (int j = 0; j < arrayLength; ++j)
                                                array.SetValue(reader.ReadUInt32(), j);
                                            field.SetValue(dbcObject, array);
                                            break;
                                        case TypeCode.Single:
                                            // Get length of array
                                            arrayLength = ((float[])field.GetValue(dbcObject)).Length;

                                            // Set Array
                                            array = new float[arrayLength];

                                            // Set Value of DBC object by looping through the array
                                            for (int j = 0; j < arrayLength; ++j)
                                                array.SetValue(reader.ReadSingle(), j);
                                            field.SetValue(dbcObject, array);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            }
                            case TypeCode.Int32:
                            {
                                int value = reader.ReadInt32();
                                field.SetValue(dbcObject, value);
                                break;
                            }
                            case TypeCode.UInt32:
                            {
                                uint value = reader.ReadUInt32();
                                field.SetValue(dbcObject, value);
                                break;
                            }
                            case TypeCode.Single:
                            {
                                float value = reader.ReadSingle();
                                field.SetValue(dbcObject, value);
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
                                field.SetValue(dbcObject, value);
                                break;
                            }
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    // Get the first value of the dbc file and use that as key for the dbc record
                    Object firstValue = fields[0].GetValue(dbcObject);
                    uint key = (uint)Convert.ChangeType(firstValue, typeof(uint));
                    records.Add(key, (T)dbcObject);
                }
            }

            IsLoaded = true;
        }

        public void SaveDBC()
        {
            string path = FilePath;

            // Todo: Save the DBC file
        }
    }
}
