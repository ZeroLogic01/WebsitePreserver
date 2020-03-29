using System.Threading.Tasks;
using WebsitePreserver.ViewModels.Base;

namespace WebsitePreserver.ViewModels
{
    public sealed class StatusViewModel : BaseViewModel
    {
        #region Private Fields

        private static StatusViewModel instance = null;

        /// <summary>
        /// An object to make the singleton pattern implementation thread safe
        /// </summary>
        private static readonly object padlock = new object();

        #endregion

        #region Constructor
        private StatusViewModel()
        {

        }

        #endregion

        #region Properties

        public static StatusViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    /* lock this object so that one thread can access this code block at a time,
                     * this ensures that only one thread will create an instance */
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new StatusViewModel();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Status text.
        /// </summary>
        public string Status { get; private set; }

        #endregion

        #region Methods

        public async Task ChangeStatus(string status, int delayInMilliseconds = 100)
        {
            await Task.Delay(delayInMilliseconds);
            Status = status;
        }

        #endregion
    }
}
