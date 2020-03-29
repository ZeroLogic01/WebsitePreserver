using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WebsiteCrawler
{
    public static class ExcelReader
    {
        /// <summary>
        /// Reads the excel file and extracts the project data from it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<List<Project>> ReadExcelFile(string filePath)
        {
            try
            {
                return await Task.Run(() =>
                  {
                      List<Project> projects = new List<Project>();

                      Application xlApp = new Application();
                      Workbook xlWorkBook = xlApp.Workbooks.Open(filePath, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                      Worksheet xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

                      Range range = xlWorkSheet.UsedRange;
                      int rw = range.Rows.Count;
                      // ignoring header row, start rCount from 2
                      for (int rCnt = 2; rCnt <= rw; rCnt++)
                      {
                          // first column (project name)
                          string name = (string)(range.Cells[rCnt, 1] as Range).Value2;
                          // 2nd column (project Uri)
                          string uri = (string)(range.Cells[rCnt, 2] as Range).Value2;

                          // search if the project name already exist
                          Project project = projects.FirstOrDefault(proj => proj.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                          if (project == null)
                          {
                              var newProj = new Project()
                              {
                                  Name = name,
                              };
                              newProj.URLs.Add(GetUri(uri).ToString());

                              projects.Add(newProj);
                          }
                          else
                          {
                              project.URLs.Add(uri);
                          }
                      }

                      xlWorkBook.Close(true, null, null);
                      xlApp.Quit();
                      Marshal.ReleaseComObject(xlWorkSheet);
                      Marshal.ReleaseComObject(xlWorkBook);
                      Marshal.ReleaseComObject(xlApp);

                      return projects;
                  });
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred reading the excel file. {ex.Message}");
            }
        }


        /// <summary>
        /// If Uri does not specify a scheme, the scheme defaults to "http:"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Uri GetUri(string s)
        {
            return new UriBuilder(s).Uri;
        }

    }
}
