namespace RegistraionAndLogin.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeNameResetPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ResetPasswordCode", c => c.String(maxLength: 100));
            DropColumn("dbo.Users", "ResetPassword");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "ResetPassword", c => c.String(maxLength: 100));
            DropColumn("dbo.Users", "ResetPasswordCode");
        }
    }
}
