using AutoIt;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCrawler
{
    internal static class FirefoxAutomater
    {
        internal static async Task<bool> SaveHtmlFile(IntPtr browserWinHandle, string saveAsDialogTitle,
                string folderPath, ProcessHelper processHelper, double waitForConsoleDelay, CancellationToken cancellationToken)
        {
            if (InvokeSaveAsDialog(browserWinHandle))
            {
                var (isSaved, fileName) = await HandleSaveAsDialog(saveAsDialogTitle
                    , folderPath, 5, processHelper, cancellationToken);
                if (isSaved)
                {
                    string fileNameWithoutExtension =
                        $"{Path.GetFileNameWithoutExtension(fileName).Replace("#", "")}";

                    string imageFile = $"\"{Path.Combine(folderPath, fileNameWithoutExtension)}\"";

                    string screenshot = $":screenshot {imageFile} --fullpage";
                    AutoItX.ControlSend(browserWinHandle, IntPtr.Zero, "^+{k}", 0);
                    await Task.Delay(TimeSpan.FromSeconds(waitForConsoleDelay));
                    AutoItX.ControlSend(browserWinHandle, IntPtr.Zero, "^+{k}", 0);
                    AutoItX.ControlSend(browserWinHandle, IntPtr.Zero, screenshot, 0);
                    AutoItX.ControlSend(browserWinHandle, IntPtr.Zero, "{Enter}", 0);

                    return true;
                }
            }
            return false;
        }

        private static bool InvokeSaveAsDialog(IntPtr browserWinHandle)
        {

            // if the window doesn't exist, return
            if (AutoItX.WinExists(browserWinHandle) == 0)
            {
                return false;
            }
            AutoItX.WinActivate(browserWinHandle);
            if (AutoItX.WinWaitActive(browserWinHandle, 10) == 0)
            {
                Console.WriteLine("Browser window not found");
                return false;
            }


            if (AutoItX.ControlSend(browserWinHandle, IntPtr.Zero, "^{s}", 0) == 0)
            {
                return false;
            }
            return true;
        }

        private static async Task<(bool isSaved, string fileName)> HandleSaveAsDialog(string saveAsDialogTitle,
            string folderPath, int retryCount,
            ProcessHelper processHelper, CancellationToken cancellationToken)
        {
            if (AutoItX.WinExists(saveAsDialogTitle) != 0)
            {
                // IntPtr saveAsDialogHandle = AutoItX.WinGetHandle(saveAsDialogTitle);
                AutoItX.WinActivate(saveAsDialogTitle);
                if (AutoItX.WinWaitActive(saveAsDialogTitle, "", 5) == 0)
                {
                    return (false, string.Empty);
                }

                // get the full file name
                string fileName = AutoItX.ControlGetText(saveAsDialogTitle, "", @"[CLASS:Edit; INSTANCE:1]");
                string fileExtension = Path.GetExtension(fileName);

                if (fileExtension.Equals(".html", StringComparison.InvariantCultureIgnoreCase) || fileExtension.Equals(".htm", StringComparison.InvariantCultureIgnoreCase))
                {
                    // get the autoit script path
                    string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "ManipulateComboBox.exe");

                    // select correct comboBox
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = scriptPath,
                        ErrorDialog = true,
                        UseShellExecute = false,
                        Arguments = $"\"{saveAsDialogTitle}\" \"{1}\""
                    };
                    await processHelper.StartProcess(startInfo, cancellationToken);
                }


                while (true)
                {
                    string currentSaveLocation = GetCurrentSaveLocation(saveAsDialogTitle);
                    /* 
                    * Sometime folder path won't get saved inside address-bar folder path running first time. 
                    * So to make sure it get saved correctly, outer while loop & the condition is important.
                   */
                    if (currentSaveLocation.Equals(folderPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }

                    //  var toolbarHandle = AutoItX.ControlGetHandle(saveAsDialogTitle, @"ToolbarWindow324");
                    if (AutoItX.ControlFocus(saveAsDialogTitle, "", @"ToolbarWindow324") == 0 || AutoItX.ControlSend(saveAsDialogTitle, "", @"ToolbarWindow324", "{space}") == 0)
                    {
                        return (false, string.Empty);
                    }
                    AutoItX.ControlSetText(saveAsDialogTitle, "", "Edit2", folderPath);
                    AutoItX.ControlSend(saveAsDialogTitle, "", "Edit2", "{Enter}");
                    await Task.Delay(500);
                }

                if (File.Exists(Path.Combine(folderPath, fileName)))
                {
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}" +
                        $"{DateTime.Now:-yyyy-MM-dd-HH-mm-ss}{fileExtension}";
                }

                AutoItX.ControlFocus(saveAsDialogTitle, "", "Edit1");
                AutoItX.ControlCommand(saveAsDialogTitle, "", "[CLASS:Edit;INSTANCE:1]", "EditPaste", fileName);
                AutoItX.ControlFocus(saveAsDialogTitle, "", "Button2");
                AutoItX.ControlClick(saveAsDialogTitle, "", "Button2");
                return (true, fileName);
            }
            else if (retryCount >= 0)
            {
                Console.WriteLine(retryCount);
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await HandleSaveAsDialog(saveAsDialogTitle,
                    folderPath, retryCount - 1, processHelper, cancellationToken);
            }
            return (false, string.Empty);
        }

        private static string GetCurrentSaveLocation(string saveAsDialogTitle)
        {
            string addressBarPrefixText = AutoItX.ControlGetText(saveAsDialogTitle, "", @"[CLASS:ToolbarWindow32; INSTANCE:4]");
            return addressBarPrefixText
                .Substring(addressBarPrefixText.IndexOf(':') + 1)
                .Trim();
        }
    }
}
