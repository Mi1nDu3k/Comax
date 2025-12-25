using System.Text;
using System.Text.RegularExpressions;

namespace Comax.Common.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return string.Empty;

            string str = phrase.ToLowerInvariant();

            // Xóa dấu tiếng Việt 
            str = RemoveDiacritics(str);

            // Xóa ký tự đặc biệt, chỉ giữ lại chữ cái, số và dấu cách
            // Thay thế tất cả ký tự không hợp lệ bằng dấu cách
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Chuyển nhiều dấu cách thành 1 dấu cách
            str = Regex.Replace(str, @"\s+", " ").Trim();

            // Cắt chuỗi nếu quá dài (tùy chọn, ví dụ tối đa 45 ký tự)
            // if (str.Length > 45) str = str.Substring(0, 45).Trim();

            // Thay khoảng trắng bằng dấu gạch ngang
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        private static string RemoveDiacritics(string text)
        {
            // Chuẩn hóa chuỗi sang dạng FormD (tách ký tự và dấu)
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                // Lấy các ký tự không phải là dấu
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuẩn hóa lại về FormC
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}