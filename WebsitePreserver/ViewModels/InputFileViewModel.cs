using ConfigurationEditor;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using WebsitePreserver.ViewModels.Base;

namespace WebsitePreserver.ViewModels
{
    public class InputFileViewModel : BaseViewModel
    {
        #region Public Fields


        private string _firefoxProfileName = ReadSelectedLanguage(nameof(FirefoxProfileName), "");

        #endregion

        #region Class Constructor

        public InputFileViewModel()
        {
            SelectCommand = new RelayCommand(Select, CanSelect);
        }

        #endregion

        #region Public Properties

        public string TbLabel { get; private set; } = "Select an input excel file";

        public string Path { get; set; } /*= @"E:\Websites Preserver\testfile-0.xlsx";*/

        public string BtnSelectLbl { get; set; } = "---";

        public ICommand SelectCommand { set; get; }

        public bool CanSelectExcelFile { set; get; } = true;


        /// <summary>
        /// Firefox profile name.
        /// </summary>
        public string FirefoxProfileName
        {
            set
            {
                if (value == _firefoxProfileName)
                    return;

                _firefoxProfileName = value;

                ConfigFileReader.UpdateKeyValue(nameof(FirefoxProfileName), value);

            }
            get
            {
                return _firefoxProfileName;
            }
        }

        #endregion

        #region Select Excel File Command

        private void Select(object input)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                    Title = "Select an input file."

                };

                fileDialog.ShowDialog(Application.Current.MainWindow);

                Path = string.IsNullOrWhiteSpace(fileDialog.FileName) ? string.Empty : fileDialog.FileName;
            }
            catch (Exception) { }
        }

        private bool CanSelect(object input)
        {
            return (CanSelectExcelFile);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the selected language code from config file and finds that language code inside the
        /// list of languages supported by google api.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaulProfileName"></param>
        /// <returns></returns>
        private static string ReadSelectedLanguage(string key, string defaulProfileName)
        {
            string value = ConfigFileReader.ReadKeyValue(key);

            return string.IsNullOrWhiteSpace(value) ?
                defaulProfileName :
                value;

        }

        #endregion

    }
}
