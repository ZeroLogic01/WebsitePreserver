using System.Collections.Generic;

namespace WebsiteCrawler
{
    /// <summary>
    /// Data contained in source excel file.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Name of the project.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Uri of the web-page
        /// </summary>
        public List<string> URLs { get; set; } = new List<string>();
    }
}
