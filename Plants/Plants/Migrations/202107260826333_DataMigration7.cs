namespace Plants.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Purchases", "Check", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Purchases", "Check");
        }
    }
}
