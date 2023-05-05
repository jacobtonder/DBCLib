using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DBCLib.Exceptions;

namespace DBCLib
{
    /// <summary>
    /// The purpose if this class is to encapsulate the DBC file for ease of use.
    /// </summary>
    /// <typeparam name="T">The type of DBC file.</typeparam>
    public class DBCFile<T> where T : class, new()
    {
        private readonly Dictionary<uint, T> records = new();
        private readonly Type dbcType;
        private readonly string filePath;
        private readonly string dbcSignature;
        private bool isEdited;
        private bool isLoaded;

        /// <summary>
        /// Initialize the DBCFile class with file path and DBC signature.
        /// </summary>
        /// <param name="filePath">The file path to the DBC file.</param>
        /// <param name="dbcSignature">The signature of the DBC file.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DBCFile(string filePath, string dbcSignature = "WDBC")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (string.IsNullOrWhiteSpace(dbcSignature))
                throw new ArgumentNullException(nameof(dbcSignature));

            this.filePath = filePath;
            this.dbcSignature = dbcSignature;
            dbcType = typeof(T);
            isEdited = false;
            isLoaded = false;
        }

        /// <summary>
        /// Dictionary of the records in the loaded DBC file.
        /// </summary>
        public Dictionary<uint, T>.ValueCollection Records => records.Values;
        /// <summary>
        /// The Max Key of all the records in the loaded DBC file.
        /// </summary>
        public uint MaxKey => records.Keys.Max();

        internal Type GetDBCType() => dbcType;
        internal uint LocalFlag { get; set; }
        internal uint LocalPosition { get; set; }

        /// <summary>
        /// Loads the specified DBC file provided in the constructor.
        /// </summary>
        /// <exception cref="FileNotFoundException">Exception thrown if the file is not found.</exception>
        public void Load()
        {
            if (isLoaded)
                return;
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find DBC File.", filePath);

            ReadDBC();

            // Set IsLoaded to true to avoid loading the same DBC file multiple times
            isLoaded = true;
        }

        /// <summary>
        /// Saves the specified DBC file provided in the constructor.
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public void Save()
        {
            if (!isEdited)
                return;

            var dbcWriter = new DBCWriter<T>();
            dbcWriter.WriteDBC(this, filePath, dbcSignature);

            isEdited = false;
        }

        /// <summary>
        /// Adds the specified value at the specified key in the DBC records.
        /// </summary>
        /// <param name="key">The key used for location in the DBC records.</param>
        /// <param name="value">The value that is added to the DBC record.</param>
        /// <exception cref="ArgumentException">Exception thrown if the provided key already exists in the DBC records.</exception>
        /// <exception cref="ArgumentNullException">Exception thrown if the provided value is null.</exception>
        public void AddEntry(uint key, T value)
        {
            if (records.ContainsKey(key))
                throw new ArgumentException("The DBC File already contains the entry.", nameof(key));

            records[key] = value ?? throw new ArgumentNullException(nameof(value));

            isEdited = true;
        }

        /// <summary>
        /// Removes the entry for the specified key in the DBC records.
        /// </summary>
        /// <param name="key">The key used for location in the DBC records.</param>
        /// <exception cref="ArgumentException">Exception thrown if the provided key does not exist in the DBC records.</exception>
        public void RemoveEntry(uint key)
        {
            if (!records.ContainsKey(key))
                throw new ArgumentException("The DBC File does not contain the entry.", nameof(key));

            records.Remove(key);

            isEdited = true;
        }

        /// <summary>
        /// Replaces the specified value at the specified key in the DBC records.
        /// </summary>
        /// <param name="key">The key used for location in the DBC records.</param>
        /// <param name="value">The value that is added to the DBC record.</param>
        /// <exception cref="ArgumentException">Exception thrown if the provided key does not exist in the DBC records.</exception>
        /// <exception cref="ArgumentNullException">Exception thrown if the provided value is null.</exception>
        public void ReplaceEntry(uint key, T value)
        {
            if (!records.ContainsKey(key))
                throw new ArgumentException("The DBC File does not contain the entry.", nameof(key));

            records[key] = value ?? throw new ArgumentNullException(nameof(value));

            isEdited = true;
        }

        private void ReadDBC()
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            var byteSignature = reader.ReadBytes(dbcSignature.Length);
            string stringSignature = Encoding.UTF8.GetString(byteSignature);
            if (stringSignature != dbcSignature)
                throw new InvalidSignatureException(stringSignature);

            // Read the DBC File
            DBCReader<T>.ReadDBC(this, reader);
        }
    }
}
