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
        private readonly Dictionary<uint, T> records = new Dictionary<uint, T>();
        private readonly Type dbcType;
        private readonly string filePath;
        private readonly string signature;
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

        public Dictionary<uint, T>.ValueCollection Records => records.Values;
        public uint MaxKey => records.Keys.Max();

        internal Type GetDBCType() => dbcType;
        internal uint LocalFlag { get; set; }
        internal uint LocalPosition { get; set; }

        public void LoadDBC()
        {
            if (isLoaded)
                return;
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                var byteSignature = reader.ReadBytes(signature.Length);
                string stringSignature = Encoding.UTF8.GetString(byteSignature);
                if (stringSignature != signature)
                    throw new InvalidSignatureException(stringSignature);

                var info = new DBCInfo(
                    reader.ReadUInt32(),    // DBC Records
                    reader.ReadUInt32(),    // DBC Fields
                    reader.ReadUInt32(),    // Record Size
                    reader.ReadUInt32()     // String Size
                );

                // Read the DBC File
                var dbcReader = new DBCReader<T>();
                dbcReader.ReadDBC(this, reader, info);
            }

            // Set IsLoaded to true to avoid loading the same DBC file multiple times
            isLoaded = true;
        }

        public void SaveDBC()
        {
            if (!isEdited)
                return;

            isEdited = false;

            var dbcWriter = new DBCWriter<T>();
            dbcWriter.WriteDBC(this, filePath, signature);
        }

        public void AddEntry(uint key, T value)
        {
            if (records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            records[key] = value ?? throw new ArgumentNullException(nameof(value));

            isEdited = true;
        }

        public void RemoveEntry(uint key)
        {
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            records.Remove(key);

            isEdited = true;
        }

        public void ReplaceEntry(uint key, T value)
        {
            if (!records.ContainsKey(key))
                throw new ArgumentException(nameof(key));

            records[key] = value ?? throw new ArgumentNullException(nameof(value));

            isEdited = true;
        }

        internal int FieldCount(FieldInfo[] fields, Type type)
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
    }
}
