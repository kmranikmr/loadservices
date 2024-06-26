using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace DataAccess.Models
{
    public partial class DAPDbContext : DbContext
    {
        public DAPDbContext()
        {
        }

        public string _connectionString { get; set; }
        public DAPDbContext(DbContextOptions<DAPDbContext> options, string connectionString = "")
            : base(options)
        {
            if (connectionString != "")
            {
                _connectionString = connectionString;
            }
            else
            {
                //_connectionString = ((SqlServerOptionsExtension)options.FindExtensionExtensions.Last()).ConnectionString;
                var sqlServerOptionsExtension =
                      options.FindExtension<SqlServerOptionsExtension>();
                if (sqlServerOptionsExtension != null)
                {
                    _connectionString = sqlServerOptionsExtension.ConnectionString;
                }
            }
        }

        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<JobStatus> JobStatuses { get; set; }
        public virtual DbSet<ModelMetadata> ModelMetadatas { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectFile> ProjectFiles { get; set; }
        public virtual DbSet<ProjectReader> ProjectReaders { get; set; }
        public virtual DbSet<ProjectSchema> ProjectSchemas { get; set; }
        public virtual DbSet<ProjectUser> ProjectUsers { get; set; }
        public virtual DbSet<ProjectWriter> ProjectWriters { get; set; }
        public virtual DbSet<Reader> Readers { get; set; }
        public virtual DbSet<ReaderType> ReaderTypes { get; set; }
        public virtual DbSet<SchemaModel> SchemaModels { get; set; }
        public virtual DbSet<SourceType> SourceTypes { get; set; }
        public virtual DbSet<Writer> Writers { get; set; }
        public virtual DbSet<WriterType> WriterTypes { get; set; }
        public virtual DbSet<ProjectAutomation> ProjectAutomations { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
                ///#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                //    optionsBuilder.UseSqlServer("Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True;");// Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123");// "Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True;");
            }
            //Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True;
            //Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123
            //Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123"
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.ProjectFileId });

                entity.ToTable("job");

                entity.Property(e => e.JobId).HasColumnName("job_id");

                entity.Property(e => e.ProjectFileId).HasColumnName("project_file_id");

                entity.Property(e => e.CompletedOn).HasColumnName("completed_on");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.JobDescription)
                    .HasColumnName("job_description")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.JobStatusId).HasColumnName("job_status_id");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.StartedOn).HasColumnName("started_on");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.JobStatus)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.JobStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__job_status___72C60C4A");

                entity.HasOne(d => d.ProjectFile)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.ProjectFileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__project_fil__71D1E811");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__project_id__73BA3083");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.SchemaId)
                    .HasConstraintName("FK__job__schema_id__74AE54BC");
            });

            modelBuilder.Entity<JobStatus>(entity =>
            {
                entity.HasKey(e => e.JobStatusId);

                entity.ToTable("job_status");

                entity.Property(e => e.JobStatusId).HasColumnName("job_status_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.StatusName)
                    .HasColumnName("status_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProjectAutomation>(entity =>
            {
                entity.ToTable("project_automation");

                entity.Property(e => e.ProjectAutomationId).HasColumnName("project_automation_id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.FolderPath)
                    .IsRequired()
                    .HasColumnName("folder_path");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ProjectSchemaId).HasColumnName("project_schema_id");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__proje__3C34F16F");

                entity.HasOne(d => d.ProjectSchema)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ProjectSchemaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__proje__3E1D39E1");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ReaderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__reade__3D2915A8");
            });


            modelBuilder.Entity<ModelMetadata>(entity =>
            {
                entity.HasKey(e => e.MetadataId);

                entity.ToTable("model_metadata");

                entity.Property(e => e.MetadataId).HasColumnName("metadata_id");

                entity.Property(e => e.ColumnName)
                    .HasColumnName("column_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DataType)
                    .HasColumnName("data_type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.HasOne(d => d.Model)
                    .WithMany(p => p.ModelMetadatas)
                    .HasForeignKey(d => d.ModelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__model_met__model__66603565");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ModelMetadatas)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__model_met__proje__656C112C");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("project");

                entity.HasIndex(e => e.ProjectName)
                    .HasName("UQ__project__4A0B0D69DA901691")
                    .IsUnique();

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsFavorite).HasColumnName("is_favorite");

                entity.Property(e => e.LastAccessedOn)
                    .HasColumnName("last_accessed_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProjectDescription)
                    .IsRequired()
                    .HasColumnName("project_description")
                    .HasMaxLength(450);

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasColumnName("project_name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ProjectFile>(entity =>
            {
                entity.ToTable("project_file");

                entity.Property(e => e.ProjectFileId).HasColumnName("project_file_id");

                entity.Property(e => e.FileName)
                    .HasColumnName("file_name")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FilePath)
                    .HasColumnName("file_path")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.SourceConfiguration).HasColumnName("source_configuration");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.UploadDate)
                    .HasColumnName("upload_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_f__proje__5629CD9C");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.ReaderId)
                    .HasConstraintName("FK__project_f__reade__59063A47");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.SchemaId)
                    .HasConstraintName("FK__project_f__schem__59FA5E80");

                entity.HasOne(d => d.SourceType)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.SourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_f__sourc__571DF1D5");
            });

            modelBuilder.Entity<ProjectReader>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.ReaderId });

                entity.ToTable("project_reader");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectReaders)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_r__proje__4CA06362");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectReaders)
                    .HasForeignKey(d => d.ReaderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_r__reade__4D94879B");
            });

            modelBuilder.Entity<ProjectSchema>(entity =>
            {
                entity.HasKey(e => e.SchemaId);

                entity.ToTable("project_schema");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaName)
                    .IsRequired()
                    .HasColumnName("schema_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeConfig)
                    .IsRequired()
                    .HasColumnName("type_config")
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectSchemas)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_s__proje__5070F446");
            });

            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.UserId });

                entity.ToTable("project_user");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PermissionBit).HasColumnName("permission_bit");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_u__proje__2B3F6F97");
            });

            modelBuilder.Entity<ProjectWriter>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.WriterId });

                entity.ToTable("project_writer");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectWriters)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_w__proje__48CFD27E");

                entity.HasOne(d => d.Writer)
                    .WithMany(p => p.ProjectWriters)
                    .HasForeignKey(d => d.WriterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_w__write__49C3F6B7");
            });

            modelBuilder.Entity<Reader>(entity =>
            {
                entity.ToTable("reader");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.Property(e => e.ConfigurationName)
                    .HasColumnName("configuration_name")
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ReaderConfiguration).HasColumnName("reader_configuration");

                entity.Property(e => e.ReaderTypeId).HasColumnName("reader_type_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.ReaderType)
                    .WithMany(p => p.Readers)
                    .HasForeignKey(d => d.ReaderTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__reader__reader_t__4316F928");
            });

            modelBuilder.Entity<ReaderType>(entity =>
            {
                entity.ToTable("reader_type");

                entity.HasIndex(e => e.ReaderTypeName)
                    .HasName("UQ__reader_t__8BCDA5A9C5231DEB")
                    .IsUnique();

                entity.Property(e => e.ReaderTypeId).HasColumnName("reader_type_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ReaderTypeName)
                    .IsRequired()
                    .HasColumnName("reader_type_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SchemaModel>(entity =>
            {
                entity.HasKey(e => e.ModelId);

                entity.ToTable("schema_model");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModelSize)
                   .HasColumnName("model_size")
                   .HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelConfig).HasColumnName("model_config");

                entity.Property(e => e.ModelName)
                    .HasColumnName("model_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.SchemaModels)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__schema_mo__proje__5FB337D6");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.SchemaModels)
                    .HasForeignKey(d => d.SchemaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__schema_mo__schem__5EBF139D");
            });

            modelBuilder.Entity<SourceType>(entity =>
            {
                entity.ToTable("source_type");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.SourceTypeName)
                    .HasColumnName("source_type_name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Writer>(entity =>
            {
                entity.ToTable("writer");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DestinationPath)
                    .HasColumnName("destination_path")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WriterTypeId).HasColumnName("writer_type_id");

                entity.HasOne(d => d.WriterType)
                    .WithMany(p => p.Writers)
                    .HasForeignKey(d => d.WriterTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__writer__writer_t__3D5E1FD2");
            });

            modelBuilder.Entity<WriterType>(entity =>
            {
                entity.ToTable("writer_type");

                entity.HasIndex(e => e.WriterTypeName)
                    .HasName("UQ__writer_t__5476CA9868F6C0AC")
                    .IsUnique();

                entity.Property(e => e.WriterTypeId).HasColumnName("writer_type_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.WriterTypeName)
                    .IsRequired()
                    .HasColumnName("writer_type_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.HasSequence<int>("job_sequence");
        }
    }
}
