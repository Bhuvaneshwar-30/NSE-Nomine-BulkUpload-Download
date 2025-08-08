using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using WebApplication1.Model;
using WebApplication1.Services;



namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NomineController : ControllerBase
    {
#pragma warning disable CS8601
        private readonly string _connectionString;
        private readonly filePath _filePath;
        private readonly Achfileservice _achFileService;

        public NomineController(IOptions<filePath> filePath, IConfiguration configuration, Achfileservice achFileService)
        {
            _filePath = filePath.Value;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _achFileService = achFileService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadNomineData([FromForm] nomineuploadDto dto)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required");

            var customerIdFromForm = dto.CustomerID;
            var uploadsFolder = _filePath.NomineeFileUploadPath;

            if (string.IsNullOrWhiteSpace(uploadsFolder))
                throw new InvalidOperationException("Upload folder path is not configured in settings.");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{DateTime.Now.Ticks}_{Path.GetFileName(dto.File.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            int extractedCustomerId = 0;
            Dictionary<string, string> excelData = new(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var filestream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(filestream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    var table = result.Tables[0];
                    Console.WriteLine($"Table info: Rows = {table.Rows.Count}, Columns = {table.Columns.Count}");

                    if (table.Rows.Count == 0)
                    {
                        return BadRequest("Excel has no Data");
                    }

                    var dataRow = table.Rows[0];
                    foreach (DataColumn column in table.Columns)
                    {
                        var key = column.ColumnName?.Trim();


                        if (string.IsNullOrEmpty(key))
                            continue;
                        var value = dataRow[column]?.ToString()?.Trim();
                        excelData[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error in processing file:{ex.Message}");
            }
            foreach (var kvp in excelData)
            {
                Console.WriteLine($"Extracted Key: {kvp.Key}, Value: {kvp.Value}");
            }

            if (!excelData.TryGetValue("Customer ID", out var customerIdStr) ||
                !int.TryParse(customerIdStr, out extractedCustomerId) || extractedCustomerId <=0)
            {
                return BadRequest("Customer ID is missing or invalid");
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var command = new SqlCommand("sp_insertUploadedFiles", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@CustomerID", extractedCustomerId);
            command.Parameters.AddWithValue("@FileName", dto.File.FileName);
            command.Parameters.AddWithValue("Filepath", filePath);
            command.Parameters.AddWithValue("@UploadTime", DateTime.Now);



            var nominedata = new SqlCommand("sp_insertnominedata", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            var nominedto = new NomineeDto
            {
                CustomerID = extractedCustomerId,
                InvestorName = excelData.GetValueOrDefault("Investor Name"),
                ExecuteThroughPoa = excelData.GetValueOrDefault("POA Execution"),
                BankName = excelData.GetValueOrDefault("Bank Name"),
                AccountNumber = long.TryParse(excelData.GetValueOrDefault("Account Number"), out var account) ? account : null,
                BranchName = excelData.GetValueOrDefault("Branch Name"),
                AccountType = excelData.GetValueOrDefault("Account Type"),
                MicrNumber = long.TryParse(excelData.GetValueOrDefault("MICR Number"), out var micr) ? micr : null,
                IfscCode = excelData.GetValueOrDefault("IFSC Code"),
                BankHolderName = excelData.GetValueOrDefault("Bank Holder Name"),
                BankHolderName1 = excelData.GetValueOrDefault("Bank Holder Name 1"),
                BankHolderName2 = excelData.GetValueOrDefault("Bank Holder Name 2"),
                AchAmount = decimal.TryParse(excelData.GetValueOrDefault("ACH Amount"), out var amt) ? amt : null,
                AchFromDate = DateTime.TryParse(excelData.GetValueOrDefault("ACH From Date"), out var from) ? from : null,
                AchToDate = DateTime.TryParse(excelData.GetValueOrDefault("ACH To Date"), out var to) ? to : null,
                ModeOfHolder = excelData.GetValueOrDefault("mode_of_holder")
            };


            nominedata.Parameters.AddWithValue("@CustomerID", extractedCustomerId);
            nominedata.Parameters.AddWithValue("@InvestorName", excelData.GetValueOrDefault("Investor Name"));
            nominedata.Parameters.AddWithValue("@ExecuteThroughPoa", excelData.GetValueOrDefault("POA Execution"));
            nominedata.Parameters.AddWithValue("@BankName", excelData.GetValueOrDefault("Bank Name"));
            nominedata.Parameters.AddWithValue("@AccountNumber", excelData.GetValueOrDefault("Account Number"));
            nominedata.Parameters.AddWithValue("@BranchName", excelData.GetValueOrDefault("Branch Name"));
            nominedata.Parameters.AddWithValue("@AccountType", excelData.GetValueOrDefault("Account Type"));
            nominedata.Parameters.AddWithValue("@MicrNumber", excelData.GetValueOrDefault("MICR Number"));
            nominedata.Parameters.AddWithValue("@IfscCode", excelData.GetValueOrDefault("IFSC Code"));
            nominedata.Parameters.AddWithValue("@BankHolderName", excelData.GetValueOrDefault("Bank Holder Name"));
            nominedata.Parameters.AddWithValue("@BankHolderName1", excelData.ContainsKey("Bank Holder Name 1") ? (object)excelData["Bank Holder Name 1"] : DBNull.Value);
            nominedata.Parameters.AddWithValue("@BankHolderName2", excelData.ContainsKey("Bank Holder Name 2") ? (object)excelData["Bank Holder Name 2"] : DBNull.Value);
            nominedata.Parameters.AddWithValue("@AchAmount",
            decimal.TryParse(excelData.GetValueOrDefault("ACH Amount"), out var achAmount) ? achAmount : DBNull.Value);
            nominedata.Parameters.AddWithValue("@AchFromDate",
            DateTime.TryParse(excelData.GetValueOrDefault("ACH From Date"), out var achfromdate) ? achfromdate : DBNull.Value);
            nominedata.Parameters.AddWithValue("@AchToDate",
            DateTime.TryParse(excelData.GetValueOrDefault("ACH To Date"), out var achtodate) ? achtodate : DBNull.Value);
            nominedata.Parameters.AddWithValue("@ModeOfHolder", excelData.ContainsKey("mode_of_holder") ? (object)excelData["mode_of_holder"] : DBNull.Value);


            command.ExecuteNonQuery();
            nominedata.ExecuteNonQuery();
            return Ok("Upload and DB insert successful.");
        }

        [HttpGet("GetNomineeData")]
        public async Task<IActionResult> GetNomineeData(int customerId, [FromQuery(Name = "searchStart")] string fromDate, [FromQuery(Name = "searchEnd")] string toDate)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                if (!DateTime.TryParseExact(fromDate, "yyyy-MM-dd", null, DateTimeStyles.None, out var fromDateParsed) ||
                    !DateTime.TryParseExact(toDate, "yyyy-MM-dd", null, DateTimeStyles.None, out var toDateParsed))
                {
                    return BadRequest("Invalid date format. Expected yyyy-MM-dd.");
                }

                var command = new SqlCommand("sp_GetNomineeData", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@CustomerID", customerId);
                command.Parameters.AddWithValue("@searchStart", fromDateParsed);
                command.Parameters.AddWithValue("@searchEnd", toDateParsed);

                var reader = await command.ExecuteReaderAsync();

                var result = new List<NomineeDto>();
                while (await reader.ReadAsync())
                {
                    var nominee = new NomineeDto
                    {
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        InvestorName = reader["InvestorName"]?.ToString(),
                        ExecuteThroughPoa = reader["ExecuteThroughPoa"]?.ToString(),
                        BankName = reader["BankName"]?.ToString(),
                        AccountNumber = reader["AccountNumber"] != DBNull.Value ? Convert.ToInt64(reader["AccountNumber"]) : null,
                        BranchName = reader["BranchName"]?.ToString(),
                        AccountType = reader["AccountType"]?.ToString(),
                        MicrNumber = reader["MicrNumber"] != DBNull.Value ? Convert.ToInt64(reader["MicrNumber"]) : null,
                        IfscCode = reader["IfscCode"]?.ToString(),
                        BankHolderName = reader["BankHolderName"]?.ToString(),
                        BankHolderName1 = reader["BankHolderName1"]?.ToString(),
                        BankHolderName2 = reader["BankHolderName2"]?.ToString(),
                        AchAmount = reader["AchAmount"] != DBNull.Value ? Convert.ToDecimal(reader["AchAmount"]) : null,
                        AchFromDate = reader["AchFromDate"] != DBNull.Value ? Convert.ToDateTime(reader["AchFromDate"]) : null,
                        AchToDate = reader["AchToDate"] != DBNull.Value ? Convert.ToDateTime(reader["AchToDate"]) : null,
                        ModeOfHolder = reader["ModeOfHolder"]?.ToString(),
                        CREATED_DATE = Convert.ToDateTime(reader["CREATED_DATE"]),
                        //FilePath = reader["FilePath"]?.ToString(),
                        //FileName = reader["FileName"]?.ToString(),
                        //UploadTime = reader["UploadTime"] != DBNull.Value ? Convert.ToDateTime(reader["UploadTime"]) : null,

                    };
                    result.Add(nominee);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server Error:{ex.Message}");
            }
        }



        //    [HttpGet("GetAchData")]
        //    public IActionResult GetAchData([FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] string filePath)
        //{
        //        if (!DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDt) ||
        //            !DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDt))
        //        {
        //            return BadRequest("Invalid date format. Expected dd/MM/yyyy.");
        //        }

        //        if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
        //        {
        //            return NotFound("Excel file not found.");
        //        }

        //        var data = _achFileService.GetAchDateRange(fromDt, toDt, filePath);

        //        if (data == null)
        //            return NotFound("No ACH data found in the given date range.");

        //        return Ok(data);
        //    }
        //}

    }

}
