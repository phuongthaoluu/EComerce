
using System.ComponentModel.DataAnnotations;

namespace Wholesale.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter username")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
