using System;
using System.Collections.Generic;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DCI_UKEHARAI_INVENTORY_API.Contexts;

public partial class DBBCS : DbContext
{
    public DBBCS()
    {
    }

    public DBBCS(DbContextOptions<DBBCS> options)
        : base(options)
    {
    }

    public virtual DbSet<ResPartList> ResPartLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.86;Database=dbBCS;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<ResPartList>(entity =>
        {
            entity.HasKey(e => e.Pid);

            entity.ToTable("RES_PART_LIST");

            entity.Property(e => e.Pid).HasColumnName("PID");
            entity.Property(e => e.Catmat)
                .HasMaxLength(1)
                .HasColumnName("CATMAT");
            entity.Property(e => e.Cm)
                .HasMaxLength(1)
                .HasColumnName("CM");
            entity.Property(e => e.Cnvcode)
                .HasMaxLength(1)
                .HasColumnName("CNVCODE");
            entity.Property(e => e.Cnvwt)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("CNVWT");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(25)
                .HasColumnName("CREATE_BY");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.Exp)
                .HasMaxLength(1)
                .HasColumnName("EXP");
            entity.Property(e => e.Ivunit)
                .HasMaxLength(10)
                .HasColumnName("IVUNIT");
            entity.Property(e => e.Lvl)
                .HasMaxLength(20)
                .HasColumnName("LVL");
            entity.Property(e => e.Model)
                .HasMaxLength(20)
                .HasColumnName("MODEL");
            entity.Property(e => e.Page)
                .HasMaxLength(20)
                .HasColumnName("PAGE");
            entity.Property(e => e.Partno)
                .HasMaxLength(20)
                .HasColumnName("PARTNO");
            entity.Property(e => e.Reqqty)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("REQQTY");
            entity.Property(e => e.Route)
                .HasMaxLength(1)
                .HasColumnName("ROUTE");
            entity.Property(e => e.Sno)
                .HasMaxLength(20)
                .HasColumnName("SNO");
            entity.Property(e => e.Vender)
                .HasMaxLength(10)
                .HasColumnName("VENDER");
            entity.Property(e => e.Whunit)
                .HasMaxLength(10)
                .HasColumnName("WHUNIT");
            entity.Property(e => e.Ym)
                .HasMaxLength(6)
                .HasColumnName("YM");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
