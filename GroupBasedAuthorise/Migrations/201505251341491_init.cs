namespace GroupBasedAuthorise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        GroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        CompanyId = c.Guid(nullable: false),
                        Permission_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.Permission_Id)
                .Index(t => t.CompanyId)
                .Index(t => t.Permission_Id);
            
            CreateTable(
                "dbo.GroupPermissions",
                c => new
                    {
                        PermissionId = c.String(nullable: false, maxLength: 128),
                        GroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PermissionId, t.GroupId })
                .ForeignKey("dbo.AspNetRoles", t => t.PermissionId, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.PermissionId)
                .Index(t => t.GroupId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Description = c.String(),
                        Global = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                        IdentityRole_Id = c.String(maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.IdentityRole_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.IdentityUser_Id)
                .Index(t => t.IdentityRole_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        GlobalGroup_Id = c.Int(),
                        Group_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GlobalGroups", t => t.GlobalGroup_Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .Index(t => t.GlobalGroup_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.IdentityUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.ApplicationUserCompanies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CompanyId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.ApplicationUserGlobalGroups",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        GlGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GlGroupId })
                .ForeignKey("dbo.GlobalGroups", t => t.GlGroupId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GlGroupId);
            
            CreateTable(
                "dbo.GlobalGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GlobalGroupPermissions",
                c => new
                    {
                        GlPermissionId = c.String(nullable: false, maxLength: 128),
                        GlGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GlPermissionId, t.GlGroupId })
                .ForeignKey("dbo.AspNetRoles", t => t.GlPermissionId, cascadeDelete: true)
                .ForeignKey("dbo.GlobalGroups", t => t.GlGroupId, cascadeDelete: true)
                .Index(t => t.GlPermissionId)
                .Index(t => t.GlGroupId);
            
            CreateTable(
                "dbo.ApplicationUserGroups",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        GroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GroupId })
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GroupId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AspNetUsers", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "IdentityUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "IdentityUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.IdentityUserClaims", "IdentityUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "IdentityRole_Id", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUsers", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.ApplicationUserGroups", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserGroups", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.ApplicationUserGlobalGroups", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserGlobalGroups", "GlGroupId", "dbo.GlobalGroups");
            DropForeignKey("dbo.AspNetUsers", "GlobalGroup_Id", "dbo.GlobalGroups");
            DropForeignKey("dbo.GlobalGroupPermissions", "GlGroupId", "dbo.GlobalGroups");
            DropForeignKey("dbo.GlobalGroupPermissions", "GlPermissionId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ApplicationUserCompanies", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserCompanies", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.GroupPermissions", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.GroupPermissions", "PermissionId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Groups", "Permission_Id", "dbo.AspNetRoles");
            DropForeignKey("dbo.Groups", "CompanyId", "dbo.Companies");
            DropIndex("dbo.AspNetUserLogins", new[] { "IdentityUser_Id" });
            DropIndex("dbo.ApplicationUserGroups", new[] { "GroupId" });
            DropIndex("dbo.ApplicationUserGroups", new[] { "UserId" });
            DropIndex("dbo.GlobalGroupPermissions", new[] { "GlGroupId" });
            DropIndex("dbo.GlobalGroupPermissions", new[] { "GlPermissionId" });
            DropIndex("dbo.ApplicationUserGlobalGroups", new[] { "GlGroupId" });
            DropIndex("dbo.ApplicationUserGlobalGroups", new[] { "UserId" });
            DropIndex("dbo.ApplicationUserCompanies", new[] { "CompanyId" });
            DropIndex("dbo.ApplicationUserCompanies", new[] { "UserId" });
            DropIndex("dbo.IdentityUserClaims", new[] { "IdentityUser_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "Group_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "GlobalGroup_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "IdentityUser_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "IdentityRole_Id" });
            DropIndex("dbo.GroupPermissions", new[] { "GroupId" });
            DropIndex("dbo.GroupPermissions", new[] { "PermissionId" });
            DropIndex("dbo.Groups", new[] { "Permission_Id" });
            DropIndex("dbo.Groups", new[] { "CompanyId" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.ApplicationUserGroups");
            DropTable("dbo.GlobalGroupPermissions");
            DropTable("dbo.GlobalGroups");
            DropTable("dbo.ApplicationUserGlobalGroups");
            DropTable("dbo.ApplicationUserCompanies");
            DropTable("dbo.IdentityUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.GroupPermissions");
            DropTable("dbo.Groups");
            DropTable("dbo.Companies");
        }
    }
}
