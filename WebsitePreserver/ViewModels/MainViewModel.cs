using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WebsiteCrawler;
using WebsitePreserver.ViewModels.Base;

namespace WebsitePreserver.ViewModels
{
    public class MainViewModel : MetroWindow
    {
        #region private Fields

        // Variable
        private readonly IDialogCoordinator _dialogCoordinator;
        private bool _isStartBtnEnabled = true;
        private bool _CanCancel = false;

        #endregion


        #region Class constructor

        public MainViewModel(IDialogCoordinator instance)
        {
            StartCommand = new RelayCommand(Start, CanStart);
            CancelCommand = new RelayCommand(Cancel, CanCancel);

            _dialogCoordinator = instance;

        }


        #endregion


        #region Public Properties

        public string ApplicationTitle { get; set; } = "Websites Preserver";


        public InputFileViewModel InputFileVM { get; set; } = new InputFileViewModel();
        public OutputDirectoryViewModel OutputDirectoryVM { get; set; } = new OutputDirectoryViewModel();

        public ICommand StartCommand { set; get; }
        public ICommand CancelCommand { set; get; }

        public string BtnStartLbl { get; set; } = "Start";
        public string BtnCancelLbl { get; set; } = "Stop";

        public CancellationTokenSource CTS { private set; get; }



        #endregion


        #region Methods

        private void Enable(bool enable)
        {
            InputFileVM.CanSelectExcelFile = enable;
            OutputDirectoryVM.CanSelectOutputDirectory = enable;
            _isStartBtnEnabled = enable;
        }

        private async void Preserver_UpdateStatusText(string status, int delayInMiliseconds = 150)
        {
            await StatusViewModel.Instance.ChangeStatus(status, delayInMiliseconds);
        }

        #endregion

        #region Start Command

        private async void Start(object input)
        {
            CTS = new CancellationTokenSource();
            _CanCancel = true;


            if (string.IsNullOrWhiteSpace(InputFileVM.Path) || !File.Exists(InputFileVM.Path))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Selected Excel file don't exist!");
                InputFileVM.Path = string.Empty;
                return;
            }
            if (string.IsNullOrWhiteSpace(OutputDirectoryVM.Path))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Output directory is missing!");
                OutputDirectoryVM.Path = string.Empty;
                return;
            }

            // Disable buttons
            Enable(false);

            await StatusViewModel.Instance.ChangeStatus("Processing...", 100);
            try
            {
                Preserver preserver = new Preserver()
                {
                    ExcelFilePath = InputFileVM.Path,
                    OutputRootDirectory = OutputDirectoryVM.Path
                };
                preserver.UpdateStatusText += Preserver_UpdateStatusText;

                await preserver.Start(CTS.Token, InputFileVM.FirefoxProfileName, 
                    Properties.Resources.SaveAsDialogTitle, Properties.Resources.ConfirmSaveAsDialogTitle,
                    Properties.Resources.SaveAsDialogFolderAddressPrefixText, Properties.Resources.TemporaryFolder);
            }
            catch (Exception e)
            {
                Application.Current.MainWindow.Activate();
                if (e != null && !string.IsNullOrWhiteSpace(e.Message) && !CTS.Token.IsCancellationRequested)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Error", $"{e.Message}");
                }
                if (!CTS.IsCancellationRequested)
                {
                    try
                    {
                        CTS.Cancel(true);
                        _CanCancel = false;
                    }
                    catch { }
                }
            }
            await StatusViewModel.Instance.ChangeStatus(string.Empty, 150);

            Application.Current.MainWindow.Activate();

            #region Process Killer Test Code

            //ProcessKiller.ClassLibrary.ProcessHelper processHelper = new ProcessKiller.ClassLibrary.ProcessHelper();
            //await processHelper.StartProcess(cancellationToken: CTS.Token);


            #endregion

            // re-enable buttons
            Enable(true);

            /*
             * Problem: Start button is not getting enabled on 
             * _isStartBtnEnabled getting true, unless we click on the UI.
             * 
             * WPF doesn't update command bound controls unless it has a reason to. 
             * Clicking on the GUI causes WPF to refresh so the update then works.
             * You can manually cause a refresh of any command bound controls by calling 
             * CommandManager.InvalidateRequerySuggested.
             * 
             */
            CommandManager.InvalidateRequerySuggested();
        }


        private bool CanStart(object obj)
        {
            if (!_isStartBtnEnabled)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(InputFileVM.FirefoxProfileName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(InputFileVM.Path))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(OutputDirectoryVM.Path))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Cancel Command

        private async void Cancel(object input)
        {
            try
            {
                if (!CTS.IsCancellationRequested)
                {
                    await StatusViewModel.Instance.ChangeStatus("Stopping...", 0);
                    CTS.Cancel(true);
                    _CanCancel = false;
                }
            }
            catch (Exception) { }
        }

        private bool CanCancel(object input)
        {
            return (!_isStartBtnEnabled && _CanCancel);
        }

        #endregion
    }
}
