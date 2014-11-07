using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AuthenticationServer.Log
{
    public class Log
    {
        public void WriteLog(string output, string filepath)
        {
            try
            {
                var fs =
                    new FileStream(
                        Environment.CurrentDirectory + filepath,
                        FileMode.Append, FileAccess.Write);
                var fw = new StreamWriter(fs);
                DateTime time = DateTime.Now;
                fw.WriteLine(time + " | " + output);
                fw.Close();
                fs.Close();
            }
            catch
            {
                throw new System.ArgumentException("Could not write Log", "LogFile");
            }
        }
    }
}
