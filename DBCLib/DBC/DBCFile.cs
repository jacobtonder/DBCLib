using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DBCLib.Exceptions;

namespace DBCLib
{
    public class DBCFile<T> where T : class, new()
    {
        private readonly string filePath;
        private readonly string signature;
        private readonly Dictionary<uint, T> records = new Dictionary<uint, T>();
        private bool isEdited;
        private bool isLoaded;

        public DBCFile(string path, string dbcSignature)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(dbcSignature))
                throw new ArgumentNullException(nameof(signature));

            filePath = path;
            signature = dbcSignature;
            dbcType = typeof(T);
            isEdited = false;
            isLoaded = false;
        }

        public Dictionary<uint, T>.ValueCollection Records { get => records.Values; }

        private readonly Type dbcType;

        internal Type GetDBCType()
        {
            return dbcType;
        }

        public uint MaxKey { get => records.Keys.Max(); }
        internal uint LocalFlag { get; set; }
        internal uint LocalPosition { get; set; }

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
            if (isLoaded)
                return;
            
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                byte[] byteSignature = reader.ReadBytes(signature.Length);
                string stringSignature = Encoding.UTF8.GetString(byteSignature);
                if (stringSignature != signature)
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
            isLoaded = true;
        }

        public void SaveDBC()
        {
            // Dont want to save if no changes done
            if (!isEdited)
                return;

            // Write to DBC File
            DBCWriter<T> dbcWriter = new DBCWriter<T>();
            dbcWriter.WriteDBC(this, filePath, signature);
        }

        public void AddEntry(uint key, T value)
        {
            // Check if key exists
            if (records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Set the key of the record to the value
            records[key] = value;

            isEdited = true;
        }

        public void RemoveEntry(uint key)
        {
            // Check if key does not exist
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Remove the value from the records
            records.Remove(key);

            isEdited = true;
        }

        public void ReplaceEntry(uint key, T value)
        {
            // Check if key does not exist
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            // Set the key of the record to the value
            records[key] = value;

            isEdited = true;
        }
    }
}
