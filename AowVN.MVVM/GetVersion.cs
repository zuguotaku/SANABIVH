using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AowVN.MVVM
{
    internal class GetVersion
    {
        public static List<string> GetVersionsInFile(string filePath)
        {
            List<string> versions = new List<string>();
            try
            {
                // Đọc nội dung của file
                string content = File.ReadAllText(filePath);

                // Sử dụng biểu thức chính quy để tìm và phân tích các phiên bản
                MatchCollection matches = Regex.Matches(content, @"\b1+\.\d+\.\d+\b");

                // Lấy các phiên bản từ các kết quả của biểu thức chính quy
                foreach (Match match in matches)
                {
                    versions.Add("v" + match.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Có lỗi xảy ra!");
            }
            return versions;
        }
    }
}
