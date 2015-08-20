using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSpeedViewer
{
    public static class Utility
    {
        #region 格式转换
        public static double dlsTemp = 0;
        public static int dlCount = 0;
        public static string NumberToDlSpeed(double dlSpeed)
        {
            if (dlSpeed == dlsTemp)
            {
                dlCount++;
                if (dlCount >= 3)
                    dlSpeed = 0;
            }
            if (dlSpeed != 0)
                dlsTemp = dlSpeed;
            string s = "";
            if (dlSpeed < 1024.0)
                s = RemoveZore(String.Format("{0:N}", dlSpeed)) + "B/秒";
            if (dlSpeed >= 1024 && dlSpeed < 1024 * 1024)
                s = RemoveZore(String.Format("{0:N}", (dlSpeed / 1024))) + "KB/秒";
            if (dlSpeed >= 1024 * 1024)
                s = RemoveZore(String.Format("{0:N}", dlSpeed / (1024 * 1024))) + "MB/秒";
            return s;
        }
        public static double ulsTemp = 0;
        public static int ulCount = 0;
        public static string NumberToUlSpeed(double ulSpeed)
        {
            if (ulSpeed == ulsTemp)
            {
                ulCount++;
                if (ulCount >= 3)
                    ulSpeed = 0;
            }
            if (ulSpeed != 0)
                ulsTemp = ulSpeed;
            string s = "";
            if (ulSpeed < 1024.0)
                s = RemoveZore(String.Format("{0:N}", ulSpeed)) + "B/秒";
            if (ulSpeed >= 1024 && ulSpeed < 1024 * 1024)
                s = RemoveZore(String.Format("{0:N}", (ulSpeed / 1024))) + "KB/秒";
            if (ulSpeed >= 1024 * 1024)
                s = RemoveZore(String.Format("{0:N}", ulSpeed / (1024 * 1024))) + "MB/秒";
            return s;
        }
        public static string RemoveZore(string s)
        {
            if (s.Substring(s.Length - 3) == ".00")
            {
                return s.Substring(0, s.Length - 3);
            }
            else if (s.Substring(s.Length - 1) == "0")
                return s.Substring(0, s.Length - 1);
            else
                return s;
        }
        #endregion
    }
}
