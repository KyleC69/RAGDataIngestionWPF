// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RAGContext.cs
// Author: Kyle L. Crowder
// Build Num: 202356



using DataIngestionLib.ExternalKnowledge.RAGModels;
using DataIngestionLib.RAGModels;

using Microsoft.EntityFrameworkCore;




namespace DataIngestionLib.Data;





public partial class RAGContext : DbContext
{

    public virtual DbSet<Document> Documents { get; init; }

    public virtual DbSet<Metadata> Metadata { get; init; }

    public virtual DbSet<RemoteRag> RemoteRags { get; init; }








    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CONN_STRING"));
        }
    }








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocId);

            entity.Property(e => e.DocId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Breadcrumb).HasMaxLength(350);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DocHtml).HasColumnName("DocHTML");
            entity.Property(e => e.Hash)
                    .HasMaxLength(450)
                    .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(512);
            entity.Property(e => e.Url).HasMaxLength(350);
        });

        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.HasKey(e => e.MetaId).HasName("PK__Metadata__60EE5418A4699143");

            entity.HasIndex(e => e.DocId, "IX_Metadata_DocId");

            entity.Property(e => e.MetaId).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RemoteRag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RemoteRA__3214EC075F4501BD");

            entity.ToTable("RemoteRAG", tb =>
            {
                tb.HasTrigger("tr_generate_embeddings");
                tb.HasTrigger("trg_RemoteRAG_CalculateScore");
            });

            entity.HasIndex(e => e.MsDate, "IX_RemoteRAG_ms_date").IsDescending();

            entity.HasIndex(e => e.Score, "IX_RemoteRAG_score").IsDescending();

            entity.HasIndex(e => e.UpdatedAt, "IX_RemoteRAG_updated_at_filtered")
                    .IsDescending()
                    .HasFilter("([embedding] IS NOT NULL)");

            entity.HasIndex(e => e.DocumentId, "UX_RemoteRAG_document_id").IsUnique();

            entity.HasIndex(e => e.OgUrl, "UX_RemoteRAG_og_url").IsUnique();

            entity.HasIndex(e => e.Embedding, "VIX_RemoteRAG_embedding");

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DocumentId)
                    .HasDefaultValueSql("(newid())", "DF__RemoteRAG__DocId__4F47C5E3")
                    .HasColumnName("document_id");
            entity.Property(e => e.Embedding)
                    .HasMaxLength(1024)
                    .HasColumnName("embedding");
            entity.Property(e => e.Keywords)
                    .HasMaxLength(500)
                    .HasColumnName("keywords");
            entity.Property(e => e.MsDate).HasColumnName("ms_date");
            entity.Property(e => e.OgUrl)
                    .HasMaxLength(500)
                    .HasColumnName("og_url");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.Title)
                    .HasMaxLength(450)
                    .HasColumnName("title");
            entity.Property(e => e.TokenCount).HasColumnName("token_count");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Version).HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }








    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}