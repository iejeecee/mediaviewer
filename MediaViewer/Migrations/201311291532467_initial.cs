namespace MediaViewer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TagCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ParentId = c.Int(),
                        TagCategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tags", t => t.ParentId)
                .ForeignKey("dbo.TagCategories", t => t.TagCategoryId)
                .Index(t => t.ParentId)
                .Index(t => t.TagCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tags", "TagCategoryId", "dbo.TagCategories");
            DropForeignKey("dbo.Tags", "ParentId", "dbo.Tags");
            DropIndex("dbo.Tags", new[] { "TagCategoryId" });
            DropIndex("dbo.Tags", new[] { "ParentId" });
            DropTable("dbo.Tags");
            DropTable("dbo.TagCategories");
        }
    }
}
