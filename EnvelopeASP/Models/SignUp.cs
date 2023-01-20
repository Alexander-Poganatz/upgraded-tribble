using System.ComponentModel.DataAnnotations;

namespace EnvelopeASP.Models
{
    public class SignUp
    {
        [Required(AllowEmptyStrings =false)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string ConfirmPassword { get; set; }

        public SignUp()
        {
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
