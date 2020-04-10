using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCrawler
{
    public class ProcessHelper
    {
        public Process Process { get; private set; }
        public bool CanKill { get; set; } = false;


        public async Task StartProcess(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            Process = new Process()
            {
                StartInfo = startInfo
            };

            Process.Exited += Process_Exited;

            var cancellationTokenRegisteration = cancellationToken.Register(() =>
            {
                try
                {
                    if (CanKill)
                    {
                        Process.Kill();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });

            try
            {
                await Task.Run(() =>
                {
                    Process.Start();
                    CanKill = true;
                    cancellationToken.ThrowIfCancellationRequested();
                    Process.WaitForExit();
                    CanKill = false;
                }, cancellationToken);
            }

            catch (OperationCanceledException)
            {
                cancellationTokenRegisteration.Dispose();
            }
            catch (Exception ex)
            {
                cancellationTokenRegisteration.Dispose();
                if (ex.Message.Equals("The system cannot find the file specified"))
                {
                    throw new FileNotFoundException("ManipulateComboBox.exe not found, Maybe blocked by the anti-virus or deleted.");
                }
                throw ex;
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            CanKill = false;
            Process.Dispose();
        }
    }
}
