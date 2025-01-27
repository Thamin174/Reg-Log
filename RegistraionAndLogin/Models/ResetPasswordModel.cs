﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistraionAndLogin.Models
{
    public class ResetPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "New password required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm Password and New Password don't match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }

    }
}