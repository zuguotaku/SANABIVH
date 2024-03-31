using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AowVN.MVVM
{
    internal class ReadVersion
    {
        public static Dictionary<string, string> ReadVersionLinkPairsFromFile(string filePath)
        {
            Dictionary<string, string> versionLinkPairs = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string currentVersion = "";
                string currentLink = "";
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("v"))
                    {
                        // Nếu dòng bắt đầu bằng "v", đó là một phiên bản mới
                        currentVersion = line.Trim();
                        currentLink = ""; // Đặt lại link cho phiên bản mới
                    }
                    else
                    {
                        // Nếu không, đó là link tải tương ứng với phiên bản trước đó
                        currentLink = line.Trim();
                        if (!string.IsNullOrEmpty(currentVersion) && !string.IsNullOrEmpty(currentLink))
                        {
                            versionLinkPairs[currentVersion] = currentLink;
                        }
                    }
                }
            }

            return versionLinkPairs;
        }
    }
}
