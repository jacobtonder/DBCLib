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
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(signature))
                throw new ArgumentNullException(nameof(signature));

            FilePath = path;
            Signature = signature;
            DBCType = typeof(T);
            IsLoaded = false;
            IsEdited = false;
        }

        public string FilePath { get; }
        public string Signature { get; }
        public Type DBCType { get; }
        public bool IsLoaded { get; private set; }
        public bool IsEdited { get; private set; }
        public uint LocaleFlag { get; internal set; }
        public Dictionary<uint, T>.ValueCollection Records { get => records.Values; }

        internal int FieldCount(FieldInfo[] fields, Type type)
        {
            Object instance = Activator.CreateInstance(type);
            int fieldCount = 0;
            foreach (FieldInfo field in fields)
            {
                if (Type.GetTypeCode(field.FieldType) == TypeCode.Object)
                {
                    if (field.FieldType == typeof(LocalizedString))
                    {
                        fieldCount += LocalizedString.Size;
                    }
                    else if (field.FieldType.IsArray)
                    {
                        switch (Type.GetTypeCode(field.FieldType.GetElementType()))
                        {
                            case TypeCode.Int32:
                                fieldCount += ((int[])field.GetValue(instance)).Length;
                                break;
                            case TypeCode.UInt32:
                                fieldCount += ((uint[])field.GetValue(instance)).Length;
                                break;
                            case TypeCode.Single:
                                fieldCount += ((float[])field.GetValue(instance)).Length;
                                break;
                            default:
                                throw new NotImplementedException(Type.GetTypeCode(field.FieldType.GetElementType()).ToString());
                        }
                    }
                }
                else
                    ++fieldCount;
            }

            return fieldCount;
        }

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

                DBCInfo info = new DBCInfo(
                    reader.ReadUInt32(), // DBC Records
                    reader.ReadUInt32(), // DBC Fields
                    reader.ReadUInt32(), // Record Size
                    reader.ReadUInt32()  // String Size
                );

                // Read the DBC File
                DBCReader<T> dbcReader = new DBCReader<T>();
                dbcReader.ReadDBC(this, reader, info);
            }

            // Set IsLoaded to true to avoid loading the same dbc file multiple times
            IsLoaded = true;
        }

        public void SaveDBC()
        {
            // Dont want to save if no changes done
            if (!IsEdited)
                return;

            string path = FilePath;
            string signature = Signature;

            // Write to DBC File
            DBCWriter<T> dbcWriter = new DBCWriter<T>();
            dbcWriter.WriteDBC(this, path, signature);
        }

        public void AddEntry(uint key, T value)
        {
            // Check if key exists
            if (records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Set the key of the record to the value
            records[key] = value;

            IsEdited = true;
        }

        public void RemoveEntry(uint key)
        {
            // Check if key does not exist
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Remove the value from the records
            records.Remove(key);

            IsEdited = true;
        }

        public void ReplaceEntry(uint key, T value)
        {
            // Check if key does not exist
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Set the key of the record to the value
            records[key] = value;

            IsEdited = true;
        }
    }
}
