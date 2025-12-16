using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using BIMKraft.Models;

namespace BIMKraft.Services
{
    /// <summary>
    /// Service for managing line length calculator presets
    /// </summary>
    public static class LineLengthPresetService
    {
        private static readonly string PresetsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BIMKraft",
            "LineLengthCalculator",
            "Presets"
        );

        private static readonly string LastSelectionFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BIMKraft",
            "LineLengthCalculator",
            "LastSelection.json"
        );

        static LineLengthPresetService()
        {
            // Ensure directory exists
            if (!Directory.Exists(PresetsDirectory))
            {
                Directory.CreateDirectory(PresetsDirectory);
            }
        }

        /// <summary>
        /// Saves a preset to a file
        /// </summary>
        public static void SavePreset(LineLengthPreset preset)
        {
            if (preset == null || string.IsNullOrWhiteSpace(preset.Name))
                throw new ArgumentException("Preset name cannot be empty");

            string fileName = GetSafeFileName(preset.Name) + ".json";
            string filePath = Path.Combine(PresetsDirectory, fileName);

            string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a preset from a file
        /// </summary>
        public static LineLengthPreset LoadPreset(string presetName)
        {
            string fileName = GetSafeFileName(presetName) + ".json";
            string filePath = Path.Combine(PresetsDirectory, fileName);

            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<LineLengthPreset>(json);
        }

        /// <summary>
        /// Gets a list of all available preset names
        /// </summary>
        public static List<string> GetPresetNames()
        {
            if (!Directory.Exists(PresetsDirectory))
                return new List<string>();

            return Directory.GetFiles(PresetsDirectory, "*.json")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Deletes a preset
        /// </summary>
        public static void DeletePreset(string presetName)
        {
            string fileName = GetSafeFileName(presetName) + ".json";
            string filePath = Path.Combine(PresetsDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Checks if a preset exists
        /// </summary>
        public static bool PresetExists(string presetName)
        {
            string fileName = GetSafeFileName(presetName) + ".json";
            string filePath = Path.Combine(PresetsDirectory, fileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Saves the last line selection
        /// </summary>
        public static void SaveLastSelection(List<int> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0)
                return;

            string directory = Path.GetDirectoryName(LastSelectionFile);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(elementIds, Formatting.Indented);
            File.WriteAllText(LastSelectionFile, json);
        }

        /// <summary>
        /// Loads the last line selection
        /// </summary>
        public static List<int> LoadLastSelection()
        {
            if (!File.Exists(LastSelectionFile))
                return new List<int>();

            try
            {
                string json = File.ReadAllText(LastSelectionFile);
                return JsonConvert.DeserializeObject<List<int>>(json) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Converts a preset name to a safe file name
        /// </summary>
        private static string GetSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
