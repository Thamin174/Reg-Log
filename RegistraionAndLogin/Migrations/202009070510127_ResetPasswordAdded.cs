namespace RegistraionAndLogin.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResetPasswordAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ResetPassword", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ResetPassword");
        }
    }
}
