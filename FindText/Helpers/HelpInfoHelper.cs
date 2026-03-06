using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace FindText.Helpers
{
    class HelpInfoHelper
    {

        public static string GetHelpInfo(string code)
        {
            Type type = App.Current.GetType();
            string resName = $"{type.FullName.Remove(type.FullName.Length - 4, 4)}.HelpInfo.{code}HelpInfo.txt";
            string str = string.Empty;
            try
            {
                using (Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
                {
                    if (sm != null)
                    {
                        byte[] bs = new byte[sm.Length];
                        sm.Read(bs, 0, (int)sm.Length);
                        sm.Close();
                        str = Encoding.UTF8.GetString(bs);
                    }
                }
            }
            catch
            {

            }
            return str;
        }
    }
}
