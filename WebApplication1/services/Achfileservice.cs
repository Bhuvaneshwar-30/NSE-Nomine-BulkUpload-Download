using OfficeOpenXml;
using System.Globalization;

namespace WebApplication1.Services
{
    public class Achfileservice
    {
        public Dictionary<string, string> GetAchDataByCreatedDate(DateTime fromDate, DateTime toDate, string filePath)
        {
            var result = new Dictionary<string, string>();
            FileInfo fileInfo = new FileInfo(filePath);

            using (var package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int colCount = worksheet.Dimension.Columns;
                int headerRow = 1;
                int dataRow = 2;

                string createdDateStr = string.Empty;

                for (int col = 1; col <= colCount; col++)
                {
                    string key = worksheet.Cells[headerRow, col].Text.Trim();
                    string value = worksheet.Cells[dataRow, col].Text.Trim();

                    if (!string.IsNullOrEmpty(key))
                        result[key] = value;

                    if (key == "CREATED_DATE")
                        createdDateStr = value;
                }
                if (DateTime.TryParseExact(createdDateStr, new[] { "dd/MM/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createdDate))
                {
                    if (createdDate >= fromDate && createdDate <= toDate)
                        return result;
                }
            }

            return null;
        }
    }
}
