namespace Plants.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addresses", "FullAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addresses", "FullAddress");
        }
    }
}
