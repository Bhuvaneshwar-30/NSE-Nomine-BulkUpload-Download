namespace WebApplication1.Model
{
    public class NomineeDto
    {
        public int CustomerID { get; set; }
        public string? InvestorName { get; set; }
        public string? ExecuteThroughPoa { get; set; }
        public string? BankName { get; set; }
        public long? AccountNumber { get; set; }
        public string? BranchName { get; set; }
        public string? AccountType { get; set; }
        public long? MicrNumber { get; set; }
        public string? IfscCode { get; set; }
        public string? BankHolderName { get; set; }
        public string? BankHolderName1 { get; set; }
        public string? BankHolderName2 { get; set; }
        public decimal? AchAmount { get; set; }
        public DateTime? AchFromDate { get; set; }
        public DateTime? AchToDate { get; set; }
        public string? ModeOfHolder { get; set; }

        public DateTime CREATED_DATE { get; set; }
        //public string? FilePath { get; set; }
        //public string? FileName { get; set; }
        //public DateTime? UploadTime { get; set; }
    }
}
