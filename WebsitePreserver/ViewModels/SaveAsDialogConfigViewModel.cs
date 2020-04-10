using ConfigurationEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsitePreserver.ViewModels.Base;

namespace WebsitePreserver.ViewModels
{
    public class SaveAsDialogConfigViewModel : BaseViewModel
    {
        #region Public Properties

        private string _saveAsDialogTitle = ReadConfigFile($"{nameof(SaveAsDialogTitle)}", string.Empty);

        public string SaveAsDialogTitle
        {
            set
            {
                if (value == _saveAsDialogTitle)
                    return;

                _saveAsDialogTitle = value;

                ConfigFileReader.UpdateKeyValue(nameof(SaveAsDialogTitle), value);

            }
            get
            {
                return _saveAsDialogTitle;
            }
        }

        //private string _confirmSaveAsDialogTitle = ReadConfigFile($"{nameof(ConfirmSaveAsDialogTitle)}", "Speichern unter bestätigen");

        //public string ConfirmSaveAsDialogTitle
        //{
        //    set
        //    {
        //        if (value == _confirmSaveAsDialogTitle)
        //            return;

        //        _confirmSaveAsDialogTitle = value;

        //        ConfigFileReader.UpdateKeyValue(nameof(ConfirmSaveAsDialogTitle), value);

        //    }
        //    get
        //    {
        //        return _confirmSaveAsDialogTitle;
        //    }
        //}


        #endregion

        #region Methods


        private static string ReadConfigFile(string key, string defaultValue)
        {
            string value = ConfigFileReader.ReadKeyValue(key);

            return string.IsNullOrWhiteSpace(value) ?
                defaultValue :
                value;

        }

        #endregion
    }
}
