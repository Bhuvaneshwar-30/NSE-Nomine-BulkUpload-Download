using System.ComponentModel;

namespace WebApplication1.Model
{
    public class nomineuploadDto
    {
        #region Properties

        [DisplayName("Customer ID")]
        public int CustomerID { get; set; }
        public IFormFile File { get; set; } = null!;

        #endregion
    }
}
