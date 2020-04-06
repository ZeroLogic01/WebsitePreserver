using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCrawler
{
    public class Preserver
    {

        #region Public Properties

        public string ExcelFilePath { get; set; }
        public string OutputRootDirectory { get; set; }

        /// <summary>
        /// Delegate to update the status text.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="delayInMiliseconds"></param>
        public delegate void ProgressHandler(string status, int delayInMiliseconds = 150);
        public event ProgressHandler UpdateStatusText;



        #endregion

        #region Methods


        public async Task Start(string firefoxProfileName, string saveAsDialogTitle, string confirmSaveAsDialogTitle, string saveAsDialogFolderAddressPrefixText,
            string tempDownloadDirectory, CancellationToken cancellationToken)
        {
            if (!Crawler.ProfileExist(firefoxProfileName)) 
            {
                throw new Exception($"Firefox profile ({firefoxProfileName}) doesn't exist.");
            }

            UpdateStatusText?.Invoke("Reading excel file...");
            var projects = await ExcelReader.ReadExcelFile(ExcelFilePath);

            if (cancellationToken.IsCancellationRequested) { return; }

            UpdateStatus("Projects details has been extracted successfully", 0);
            Crawler crawler = new Crawler();
            crawler.UpdateStatus += UpdateStatus;


            string _temporaryDownloadsDirectory = Path.Combine(OutputRootDirectory, tempDownloadDirectory);
            await crawler.DownloadWebpageAndScreenshot(projects, OutputRootDirectory,
               firefoxProfileName, _temporaryDownloadsDirectory, saveAsDialogTitle, confirmSaveAsDialogTitle,
                    saveAsDialogFolderAddressPrefixText, cancellationToken);
            try
            {
                // delete the "temporary files" directory
                if (Directory.Exists(_temporaryDownloadsDirectory))
                {
                    Directory.Delete(_temporaryDownloadsDirectory);
                }
            }
            catch (Exception) { }
        }


        public void UpdateStatus(string status, int delayInMiliseconds = 150)
        {
            UpdateStatusText?.Invoke(status, delayInMiliseconds);
        }

        #endregion
    }
}
