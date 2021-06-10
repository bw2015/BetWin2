using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SP.Studio.ErrorLog;
using SP.Studio.IO;

namespace GameLogService
{
    public class Utils
    {
        public static void SaveLog(Exception ex)
        {
            string errorId;
            int httpCode;
            string result = ErrorAgent.CreateDetail(ex, out errorId, out httpCode);
            SaveLog(result);
        }

        public static void SaveLog(string message)
        {
            string errorFile = System.Windows.Forms.Application.StartupPath + @"\ErrorLog\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            FileAgent.CreateDirectory(errorFile, true);
            File.AppendAllText(errorFile, message + "\n\r\n\r");
        }
    }
}
