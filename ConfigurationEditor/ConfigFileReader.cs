using System;
using System.Configuration;

namespace ConfigurationEditor
{
    public static class ConfigFileReader
    {
        #region Methods

        /// <summary>
        /// Reads key value from the app.config file
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadKeyValue(string key)
        {
            string value = string.Empty;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                value = appSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading application settings");
            }
            return value;
        }

        /// <summary>
        /// Updates the key value in app.config file.
        /// </summary>
        /// <param name="value">The updated value.</param>
        public static void UpdateKeyValue(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing application settings");
            }
        }

        #endregion
    }
}
