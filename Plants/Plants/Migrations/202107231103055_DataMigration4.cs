namespace Plants.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuantityProducts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        PurchaseId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Purchases", t => t.PurchaseId)
                .Index(t => t.PurchaseId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuantityProducts", "PurchaseId", "dbo.Purchases");
            DropIndex("dbo.QuantityProducts", new[] { "PurchaseId" });
            DropTable("dbo.QuantityProducts");
        }
    }
}
