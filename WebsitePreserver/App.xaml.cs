using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebsitePreserver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is System.Runtime.InteropServices.COMException comException && comException.ErrorCode == -2147221040)
            {
                MessageBox.Show(e.Exception.Message
                    + Environment.NewLine
                    + "Shutting down the Application!");
                Current.Shutdown();
            }

            if (e.Exception != null)
            {
                MessageBox.Show($"An unexpected error occurred: {e.Exception.InnerException.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;
                var currentAssembly = Assembly.GetExecutingAssembly();

                // Setup path to application config file in ./Config dir:
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = Environment.CurrentDirectory
                };
#if (DEBUG)
                setup.ConfigurationFile = setup.ApplicationBase +
                                 string.Format("\\{0}.config", appName);
#else
                setup.ConfigurationFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Website Preserver" +
                                                    string.Format("\\Config\\WebsitePreserver.exe.config");
#endif

                //Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}

                // Create a new app domain using setup with new config file path:
                AppDomain newDomain = AppDomain.CreateDomain("NewAppDomain", null, setup);
                int ret = newDomain.ExecuteAssemblyByName(currentAssembly.FullName, e.Args);
                // Above causes recursive call to this method.

                //--------------------------------------------------------------------------//

                try
                {
                    AppDomain.Unload(newDomain);
                    Environment.ExitCode = ret;

                }
                catch (CannotUnloadAppDomainException) { }
                catch (Exception) { }
                // We get here when the new app domain we created is shutdown.  Shutdown the 
                // original default app domain (to avoid running app again there):
                // We could use Shutdown(0) but we have to remove the main window uri from xaml
                // and then set it for new app domain (above execute command) using:
                // StartupUri = new Uri("Window1.xaml", UriKind.Relative);
                Environment.Exit(0);
                return;
            }
        }
    }
}
