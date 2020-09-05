using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RegistraionAndLogin.Models
{
    public class LoginContext: DbContext
    {
        public LoginContext() : base("name=LoginContext")
        {

        }

        public DbSet<User> Users { get; set; }
    }
}