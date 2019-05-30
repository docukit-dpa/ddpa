using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DDPA.SQL.Entities;

namespace DDPA.SQL.Repositories.Context
{
    public class ApplicationDbContext : IdentityDbContext<ExtendedIdentityUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        // Add DbSet main tables here
        public DbSet<Document> Document { get; set; }
        public DbSet<DocumentField> DocumentField { get; set; }
        public DbSet<Field> Field { get; set; }
        public DbSet<FieldItem> FieldItem { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<SubModule> SubModule { get; set; }
        public DbSet<SubModuleField> ModuleField { get; set; }
        public DbSet<Dataset> Dataset { get; set; }
        public DbSet<DatasetField> DatasetField { get; set; }
        public DbSet<UserRights> UserRights { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<WorkflowInbox> WorkflowInbox { get; set; }
        public DbSet<Logs> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");

            //// Register OpenIddict
            modelBuilder.UseOpenIddict();

            //// TODO: Configure limit, constraints, table index field
            //// Register main entities
            modelBuilder.Entity<ExtendedIdentityUser>(entity =>
            {
                entity.ToTable("Users");
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Document").HasKey(c => new
                {
                    c.Id
                });

                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.DataNumber).HasColumnName("datanumber");
                entity.Property(c => c.DueDate).HasColumnName("duedate");
                entity.Property(c => c.Status).HasColumnName("status");
                entity.Property(c => c.State).HasColumnName("state");
                entity.Property(c => c.RequestType).HasColumnName("requestType");
                entity.Property(c => c.DepartmentId).HasColumnName("departmentid");
                entity.Property(c => c.DatasetId).HasColumnName("datasetid");
                entity.Property(c => c.DataSubject).HasColumnName("datasubject");
            });

            modelBuilder.Entity<DocumentField>(entity =>
            {
                entity.ToTable("DocumentFields").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.DocumentId).HasColumnName("docid").IsRequired();
                entity.Property(c => c.FieldId).HasColumnName("fieldId").IsRequired();
                entity.Property(c => c.SubModuleId).HasColumnName("submoduleid").IsRequired();
                entity.Property(c => c.Value).HasColumnName("value");
                entity.Property(c => c.NewValue).HasColumnName("newvalue");

                entity.Property(c => c.FilePath).HasColumnName("filepath");

                entity.HasOne(e => e.Document).WithMany(e => e.DocumentField).HasForeignKey(cc => cc.DocumentId).IsRequired();
            });

            modelBuilder.Entity<DocumentDatasetField>(entity =>
            {
                entity.ToTable("DocumentDatasetFields").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.DocumentId).HasColumnName("docid").IsRequired();
                entity.Property(c => c.FieldId).HasColumnName("fieldId").IsRequired();
                entity.Property(c => c.DatasetId).HasColumnName("datasetid").IsRequired();
                entity.Property(c => c.Value).HasColumnName("value");
                entity.Property(c => c.FilePath).HasColumnName("filepath");

                entity.HasOne(e => e.Document).WithMany(e => e.DocumentDatasetField).HasForeignKey(cc => cc.DocumentId).IsRequired();
            });

            modelBuilder.Entity<Field>(entity =>
            {
                entity.ToTable("Field").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.IsDefault).HasColumnName("isdefault");
                entity.Property(c => c.IsRequired).HasColumnName("isrequired");
                entity.Property(c => c.IsLifeCycle).HasColumnName("islifecycle");
                entity.Property(c => c.Type).HasColumnName("type");
                entity.Property(c => c.Purpose).HasColumnName("purpose");
            });

            modelBuilder.Entity<FieldItem>(entity =>
            {
                entity.ToTable("FieldItems").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.FieldId).HasColumnName("fieldId").IsRequired();
                entity.HasOne(e => e.Field).WithMany(e => e.FieldItem).HasForeignKey(cc => cc.FieldId).IsRequired();
            });

            modelBuilder.Entity<Module>(entity =>
            {
                entity.ToTable("Module").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.isEnabled).HasColumnName("isenabled");
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.Url).HasColumnName("url");
            });

            modelBuilder.Entity<SubModule>(entity =>
            {
                entity.ToTable("SubModule").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.ModuleId).HasColumnName("moduleid").IsRequired();
                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.Display).HasColumnName("display");
                entity.Property(c => c.isEnabled).HasColumnName("isenabled");
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.Url).HasColumnName("url");

                entity.HasOne(e => e.Module).WithMany(e => e.SubModule).HasForeignKey(cc => cc.ModuleId).IsRequired();
            });

            modelBuilder.Entity<SubModuleField>(entity =>
            {
                entity.ToTable("SubModuleField").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.SubModuleId).HasColumnName("submoduleid");
                entity.Property(c => c.FieldId).HasColumnName("fieldid").IsRequired();
                entity.Property(c => c.Order).HasColumnName("order").IsRequired();
                entity.HasOne(e => e.SubModule).WithMany(e => e.SubModuleField).HasForeignKey(cc => cc.SubModuleId).IsRequired();
                entity.HasOne(e => e.Field).WithMany(e => e.SubModuleField).HasForeignKey(cc => cc.FieldId).IsRequired();
            });

            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("Dataset").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.Description).HasColumnName("description");
            });

            modelBuilder.Entity<DatasetField>(entity =>
            {
                entity.ToTable("DatasetField").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

                entity.Property(c => c.DatasetId).HasColumnName("datasetid");
                entity.Property(c => c.FieldId).HasColumnName("fieldid").IsRequired();
                entity.Property(c => c.Order).HasColumnName("order").IsRequired();
                entity.HasOne(e => e.Dataset).WithMany(e => e.DatasetField).HasForeignKey(cc => cc.DatasetId).IsRequired();
                entity.ToTable("DatasetField");
                entity.HasOne(e => e.Field).WithMany(e => e.DatasetField).HasForeignKey(cc => cc.FieldId).IsRequired();
            });
            modelBuilder.Entity<UserRights>(entity =>
            {
                entity.ToTable("UserRights").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();

                entity.Property(c => c.UserId).HasColumnName("userId");
                entity.Property(c => c.ModuleId).HasColumnName("moduleId");
                entity.Property(c => c.View).HasColumnName("view");
                entity.Property(c => c.Add).HasColumnName("add");
                entity.Property(c => c.Edit).HasColumnName("edit");
                entity.Property(c => c.Delete).HasColumnName("delete");

            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.Status).HasColumnName("status");
            });

            modelBuilder.Entity<WorkflowInbox>(entity =>
            {
                entity.ToTable("WorkflowInbox").HasKey(c => new
                {
                    c.Id
                });

                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.ApproverRole).HasColumnName("approverrole");
                entity.Property(c => c.CreatedBy).HasColumnName("createdby");
                entity.Property(c => c.DepartmentId).HasColumnName("departmentid");
                entity.Property(c => c.DocumentId).HasColumnName("documentid");
                entity.Property(c => c.Status).HasColumnName("status");

            });

            modelBuilder.Entity<Logs>(entity =>
            {
                entity.ToTable("Logs").HasKey(c => new
                {
                    c.Id
                });

                entity.ToTable("Logs").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();

                entity.Property(c => c.DocId).HasColumnName("docId");
                entity.Property(c => c.DataNumber).HasColumnName("dataNumber");
                entity.Property(c => c.UserId).HasColumnName("userid");
                entity.Property(c => c.Action).HasColumnName("action");
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.Comment).HasColumnName("comment");
                entity.Property(c => c.ActionDate).HasColumnName("actionDate");


            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.ToTable("Resource").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.RowVersion).HasColumnName("rowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.NameOfDocument).HasColumnName("nameofdocument").IsRequired();
                entity.Property(c => c.TypeOfDocument).HasColumnName("typeofdocument").IsRequired();
                entity.Property(c => c.FilePath).HasColumnName("filepath").IsRequired();
            });

            modelBuilder.Entity<Issues>(entity =>
            {
                entity.ToTable("Issue").HasKey(c => new
                {
                    c.Id
                });

                entity.ToTable("Issue").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.CreatedDate).HasColumnName("createdDate").HasColumnType("date").IsRequired();
                entity.Property(c => c.CreatedBy).HasColumnName("createdBy").IsRequired();
                entity.Property(c => c.ModifiedDate).HasColumnName("modifiedDate").HasColumnType("date");
                entity.Property(c => c.ModifiedBy).HasColumnName("modifiedBy");

                entity.Property(c => c.DocId).HasColumnName("docId");
                entity.Property(c => c.Issue).HasColumnName("issue");
                entity.Property(c => c.SeverityLevel).HasColumnName("severityLevel");
                entity.Property(c => c.Action).HasColumnName("action");
                entity.Property(c => c.Status).HasColumnName("status");
                entity.HasOne(e => e.Document).WithMany(e => e.Issue).HasForeignKey(cc => cc.DocId).IsRequired();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company").HasKey(c => new
                {
                    c.Id
                });
                entity.ToTable("Company").HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.Name).HasColumnName("name");
                entity.Property(c => c.ContactNo).HasColumnName("contactNo");
                entity.Property(c => c.Email).HasColumnName("email");
                entity.Property(c => c.Description).HasColumnName("description");
            });
        }
    }
}