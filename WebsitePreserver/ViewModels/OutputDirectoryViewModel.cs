
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows.Input;
using WebsitePreserver.ViewModels.Base;

namespace WebsitePreserver.ViewModels
{
    public class OutputDirectoryViewModel : BaseViewModel
    {
        #region Class Constructor

        public OutputDirectoryViewModel()
        {
            SelectCommand = new RelayCommand(Select, CanSelect);
        }

        #endregion

        #region Public Properties

        public string TbLabel { get; private set; } = "Output Directory";

        public string Path { get; set; } /*= @"E:\Websites Preserver\";*/

        public string BtnSelectLbl { get; set; } = "---";

        public ICommand SelectCommand { set; get; }

        public bool CanSelectOutputDirectory { set; get; } = true;

        #endregion

        #region Select Output Root Directory Command

        private void Select(object input)
        {
            try
            {

                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select an output directory."
                };

                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    Path = dialog.FileName;
                }
            }
            catch (Exception) { }
        }

        private bool CanSelect(object input)
        {
            return (CanSelectOutputDirectory);
        }

        #endregion
    }
}
