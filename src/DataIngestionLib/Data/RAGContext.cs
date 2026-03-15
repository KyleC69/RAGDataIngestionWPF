// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using DataIngestionLib.RAGModels;

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataIngestionLib.Data;





public class RAGContext : DbContext
    {

    public virtual DbSet<Document> Documents { get; init; }

    public virtual DbSet<Metadata> Metadata { get; init; }

    public virtual DbSet<RemoteRag> RemoteRags { get; init; }








    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        if (!optionsBuilder.IsConfigured)
            {
            _ = optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CONN_STRING"));
            }
        }








    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        ModelBuilder unused33 = modelBuilder.Entity<Document>(entity =>
        {
            KeyBuilder unused32 = entity.HasKey(e => e.DocId);

            PropertyBuilder<Guid> unused31 = entity.Property(e => e.DocId).HasDefaultValueSql("(newid())");
            PropertyBuilder<string?> unused30 = entity.Property(e => e.Breadcrumb).HasMaxLength(350);
            PropertyBuilder<DateTime> unused29 = entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            PropertyBuilder<string?> unused28 = entity.Property(e => e.DocHtml).HasColumnName("DocHTML");
            PropertyBuilder<string?> unused27 = entity.Property(e => e.Hash)
                    .HasMaxLength(450)
                    .IsUnicode(false);
            PropertyBuilder<string> unused26 = entity.Property(e => e.Title).HasMaxLength(512);
            PropertyBuilder<string?> unused25 = entity.Property(e => e.Url).HasMaxLength(350);
        })
        .Entity<Metadata>(entity =>
        {
            KeyBuilder unused24 = entity.HasKey(e => e.MetaId).HasName("PK__Metadata__60EE5418A4699143");

            IndexBuilder<Metadata> unused23 = entity.HasIndex(e => e.DocId, "IX_Metadata_DocId");

            PropertyBuilder<Guid> unused22 = entity.Property(e => e.MetaId).HasDefaultValueSql("(newid())");
        })
        .Entity<RemoteRag>(entity =>
        {
            KeyBuilder unused21 = entity.HasKey(e => e.Id).HasName("PK__RemoteRA__3214EC075F4501BD");

            EntityTypeBuilder<RemoteRag> unused20 = entity.ToTable("RemoteRAG", tb =>
            {
                TableTriggerBuilder unused19 = tb.HasTrigger("tr_generate_embeddings");
                TableTriggerBuilder unused18 = tb.HasTrigger("trg_RemoteRAG_CalculateScore");
            });

            IndexBuilder<RemoteRag> unused17 = entity.HasIndex(e => e.MsDate, "IX_RemoteRAG_ms_date").IsDescending();

            IndexBuilder<RemoteRag> unused16 = entity.HasIndex(e => e.Score, "IX_RemoteRAG_score").IsDescending();

            IndexBuilder<RemoteRag> unused15 = entity.HasIndex(e => e.UpdatedAt, "IX_RemoteRAG_updated_at_filtered")
                    .IsDescending()
                    .HasFilter("([embedding] IS NOT NULL)");

            IndexBuilder<RemoteRag> unused14 = entity.HasIndex(e => e.DocumentId, "UX_RemoteRAG_document_id").IsUnique();

            IndexBuilder<RemoteRag> unused13 = entity.HasIndex(e => e.OgUrl, "UX_RemoteRAG_og_url").IsUnique();

            IndexBuilder<RemoteRag> unused12 = entity.HasIndex(e => e.Embedding, "VIX_RemoteRAG_embedding");

            PropertyBuilder<string> unused11 = entity.Property(e => e.Description).HasColumnName("description");
            PropertyBuilder<Guid> unused10 = entity.Property(e => e.DocumentId)
                    .HasDefaultValueSql("(newid())", "DF__RemoteRAG__DocId__4F47C5E3")
                    .HasColumnName("document_id");
            PropertyBuilder<SqlVector<float>?> unused9 = entity.Property(e => e.Embedding)
                    .HasMaxLength(1024)
                    .HasColumnName("embedding");
            PropertyBuilder<string?> unused8 = entity.Property(e => e.Keywords)
                    .HasMaxLength(500)
                    .HasColumnName("keywords");
            PropertyBuilder<DateTime> unused7 = entity.Property(e => e.MsDate).HasColumnName("ms_date");
            PropertyBuilder<string> unused6 = entity.Property(e => e.OgUrl)
                    .HasMaxLength(500)
                    .HasColumnName("og_url");
            PropertyBuilder<double?> unused5 = entity.Property(e => e.Score).HasColumnName("score");
            PropertyBuilder<string?> unused4 = entity.Property(e => e.Summary).HasColumnName("summary");
            PropertyBuilder<string> unused3 = entity.Property(e => e.Title)
                    .HasMaxLength(450)
                    .HasColumnName("title");
            PropertyBuilder<int?> unused2 = entity.Property(e => e.TokenCount).HasColumnName("token_count");
            PropertyBuilder<DateTime?> unused1 = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            PropertyBuilder<int?> unused = entity.Property(e => e.Version).HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
        }








    private static void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
        ModelBuilder unused4 = modelBuilder.Entity<Document>(entity =>
        {
            IndexBuilder<Document> unused3 = entity.HasIndex(e => e.Hash, "IX_Document_Hash").IsUnique();
            IndexBuilder<Document> unused2 = entity.HasIndex(e => e.Url, "IX_Document_Url").IsUnique();
        })
            .Entity<Metadata>(entity => entity.HasOne<Document>().WithMany().HasForeignKey(e => e.DocId).OnDelete(DeleteBehavior.Cascade))
            .Entity<RemoteRag>(entity =>
        {
            PropertyBuilder<string> unused1 = entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);

            ReferenceCollectionBuilder<Document, RemoteRag> unused = entity.HasOne<Document>().WithMany().HasForeignKey(e => e.DocumentId).OnDelete(DeleteBehavior.Restrict);
        });
        }
    }