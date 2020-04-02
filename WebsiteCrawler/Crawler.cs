using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebsiteCrawler
{
    public class Crawler
    {
        /// <summary>
        /// Delegate to update the status text.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="delayInMiliseconds"></param>
        public delegate void ProgressHandler(string status, int delayInMiliseconds = 150);
        public event ProgressHandler UpdateStatus;

        public async Task DownloadWebpageAndScreenshot(List<Project> projects, string localPath,
            string firefoxProfile, string temporaryDownloadsDirectory, string saveAsDialogTitle, string confirmSaveAsDialogTitle,
            string saveAsDialogFolderAddressPrefixText, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(saveAsDialogTitle);
                Console.WriteLine(confirmSaveAsDialogTitle);
                Console.WriteLine(saveAsDialogFolderAddressPrefixText);
                UpdateStatus?.Invoke($"Initializing Firefox...");

                await Task.Run(async () =>
                    {
                        FirefoxOptions options = new FirefoxOptions()
                        {
                            Profile = GetFirefoxProfile(firefoxProfile, temporaryDownloadsDirectory)
                        };

                        FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();

                        service.HideCommandPromptWindow = true;

                        //Pass FProfile parameter In web-driver to use preferences to download file.
                        FirefoxDriver driver = new FirefoxDriver(service, options, TimeSpan.FromMinutes(10));
                        if (cancellationToken.IsCancellationRequested)
                        {
                            driver.Quit();
                            return;
                        }

                        await Task.Run(async () =>
                           {
                               // maximize window
                               driver.Manage().Window.Maximize();

                               foreach (var project in projects)
                               {
                                   if (cancellationToken.IsCancellationRequested) { break; }

                                   var projectDir = Directory.CreateDirectory(Path.Combine(localPath, project.Name));

                                   UpdateStatus?.Invoke($"Preserving {project.Name}...", 0);

                                   //Path of the text-file that contains all URLSs of this project
                                   string filePath = Path.Combine(localPath, project.Name, $"{project.Name} URLs.txt");

                                   // initialize the Process helper
                                   ProcessHelper processHelper = new ProcessHelper();

                                   //var url = @"http://seifen-trend.wixsite.com/mtg-seifen-trend";
                                   foreach (var url in project.URLs)
                                   {
                                       try
                                       {
                                           cancellationToken.ThrowIfCancellationRequested();

                                           UpdateStatus?.Invoke($"Preserving {url}", 0);
                                           driver.Navigate().GoToUrl(url);

                                           cancellationToken.ThrowIfCancellationRequested();

                                           string pageTitle = driver.Title;

                                           pageTitle = pageTitle.Replace("\"", "\"\"");

                                           // get the autoit script path
                                           string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "SaveHtmlFile.exe");

                                           /* 
                                            * some webpages load pages using ajax/javascript which the webdriver can't 
                                            * detect so it's better wait for x seconds to let the page fully load then
                                            * run the process
                                           */
                                           await Task.Delay(TimeSpan.FromSeconds(2));
                                           await Task.Run(async () =>
                                           {
                                               ProcessStartInfo startInfo = new ProcessStartInfo
                                               {
                                                   FileName = scriptPath,
                                                   ErrorDialog = true,
                                                   UseShellExecute = false,
                                                   Arguments = $"\"{projectDir.FullName}\" \"{pageTitle}\" \"{saveAsDialogTitle}\" \"{confirmSaveAsDialogTitle}\" " +
                                                   $"\"{saveAsDialogFolderAddressPrefixText}\""
                                               };

                                               await processHelper.StartProcess(startInfo, cancellationToken);

                                           });

                                           /* 
                                            * if URL is the first element, this means we need 
                                            * to create a new file
                                            */
                                           if (project.URLs.FirstOrDefault().Equals(url))
                                           {
                                               await TextFileCreator.Create(filePath, $"{url}{Environment.NewLine}");
                                           }
                                           else /* else append it */
                                           {
                                               await TextFileCreator.Append(filePath, $"{url}{Environment.NewLine}");
                                           }
                                       }
                                       catch (OperationCanceledException)
                                       {
                                           if (processHelper.CanKill)
                                           {
                                               processHelper.Process.Kill();
                                           }
                                           driver.Quit();
                                           return;
                                       }
                                       catch (Exception)
                                       {
                                           UpdateStatus?.Invoke($"An error occurred preserving {project.Name}", 0);
                                           if (processHelper.CanKill)
                                           {
                                               processHelper.Process.Kill();
                                           }
                                           driver.Quit();
                                           throw;
                                       }
                                   }
                                   // break;
                               }

                               if (!cancellationToken.IsCancellationRequested)
                               {
                                   UpdateStatus?.Invoke($"Preservation complete", 0);
                                   var msg = "Ensure that whether all files are successfully downloaded  or not, if yes press the stop button";
                                   UpdateStatus?.Invoke(msg);
                                   // await Task.Delay(TimeSpan.FromSeconds(6));
                                   MessageBox.Show(msg,
                                       "Information"
                                       , MessageBoxButton.OK, MessageBoxImage.Information);
                               }

                               while (true)
                               {
                                   if (cancellationToken.IsCancellationRequested)
                                   {
                                       driver.Quit();
                                       return;
                                   }
                                   await Task.Delay(TimeSpan.FromSeconds(1));
                               }
                           });

                    }, cancellationToken);
            }
            catch (TaskCanceledException) { }
        }

        private static FirefoxProfile GetFirefoxProfile(string firefoxProfile, string temporaryDownloadsDirectory)
        {
            FirefoxProfileManager manager = new FirefoxProfileManager();

            FirefoxProfile profile = manager.GetProfile(firefoxProfile);
            if (profile == null)
            {
                profile = new FirefoxProfile();
            }


            //Set Location to store files after downloading.
            //profile.SetPreference("intl.accept_languages", "pa-IN");
            profile.SetPreference("browser.download.dir", temporaryDownloadsDirectory);
            profile.SetPreference("browser.download.folderList", 2);
            //Set Preference to not show file download confirmation dialog using MIME types Of different file extension types.
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk",
                "text/html;image/png;image/jpeg;image/pjpeg;text/html;");
            profile.SetPreference("pdfjs.disabled", false);

            return profile;
        }

        public static bool ProfileExist(string firefoxProfile)
        {
            FirefoxProfileManager manager = new FirefoxProfileManager();

            FirefoxProfile profile = manager.GetProfile(firefoxProfile);
            if (profile == null)
            {
                return false;
            }
            return true;
        }
    }
}