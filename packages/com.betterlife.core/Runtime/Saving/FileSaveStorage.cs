using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Save storage as UTF-8 files "&lt;slot&gt;.json" in a folder. Writes go to a temporary file first and then replace the
    /// slot, so a crash never leaves a half-written save.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private const string Extension = ".json";
        private const string TempExtension = ".tmp";
        private const string BackupExtension = ".bak";

        private readonly string _directory;

        /// <summary>
        /// Creates the storage; the folder is created when missing.
        /// </summary>
        /// <param name="directory">Folder for save files.</param>
        public FileSaveStorage(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("A save folder is required.", nameof(directory));
            }
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        /// <inheritdoc />
        public bool Exists(string slot) => File.Exists(PathOf(slot));

        /// <inheritdoc />
        public string Read(string slot)
        {
            string path = PathOf(slot);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        /// <inheritdoc />
        public void Write(string slot, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            string path = PathOf(slot);
            string temp = path + TempExtension;
            File.WriteAllText(temp, text, new UTF8Encoding(false));
            if (File.Exists(path))
            {
                File.Replace(temp, path, path + BackupExtension);
                File.Delete(path + BackupExtension);
            }
            else
            {
                File.Move(temp, path);
            }
        }

        /// <inheritdoc />
        public bool Delete(string slot)
        {
            string path = PathOf(slot);
            if (!File.Exists(path))
            {
                return false;
            }
            File.Delete(path);
            return true;
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Slots()
        {
            return Directory.GetFiles(_directory, "*" + Extension)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(SlotNames.IsValid)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();
        }

        private string PathOf(string slot) => Path.Combine(_directory, SlotNames.Require(slot) + Extension);
    }
}
