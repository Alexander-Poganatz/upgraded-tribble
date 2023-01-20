using System.ComponentModel.DataAnnotations;

namespace EnvelopeASP.Models
{
    public class Login
    {
        [Required(AllowEmptyStrings =false)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }

        public Login()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}
