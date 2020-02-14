using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BeatapChartMaker
{
    class ReadWriteIni
    {
        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);
        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
        private readonly StringBuilder _builder = new StringBuilder(255);
        public string FilePath { get; set; }

        public ReadWriteIni(string path)
        {
            FilePath = path;
        }

        public string GetStringData(string section, string key, string defaultValue = null)
        {
            _builder.Clear();
            GetPrivateProfileString(section, key, defaultValue, _builder, 255, FilePath);
            return _builder.ToString();
        }

        public int GetIntData(string section, string key, int defaultValue = 0)
        {
            return (int)GetPrivateProfileInt(section, key, defaultValue, FilePath);
        }

        public bool WriteString(string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, FilePath);
        }
    }
}
