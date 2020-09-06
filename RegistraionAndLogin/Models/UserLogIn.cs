using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistraionAndLogin.Models
{
    public class UserLogIn
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="Email required")]
        [MaxLength(255)]
        [DataType(DataType.Password)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings =false, ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name ="Remember Me")]
        public bool RememberMe { get; set; }
    }
}