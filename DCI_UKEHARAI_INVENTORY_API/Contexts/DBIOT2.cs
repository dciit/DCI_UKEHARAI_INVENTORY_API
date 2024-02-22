using System;
using System.Collections.Generic;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DCI_UKEHARAI_INVENTORY_API.Contexts;

public partial class DBIOT2 : DbContext
{
    public DBIOT2()
    {
    }

    public DBIOT2(DbContextOptions<DBIOT2> options)
        : base(options)
    {
    }

    public virtual DbSet<EtdLeakCheck> EtdLeakChecks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.145;Database=dbIotFac2;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EtdLeakCheck>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("etd_leak_check", tb =>
                {
                    tb.HasTrigger("Data_Brazing_Cert");
                    tb.HasTrigger("Data_Brazing_Cert_New");
                    tb.HasTrigger("Main_Part_Stock_Out");
                });

            entity.HasIndex(e => new { e.Brazing, e.LineName }, "<Name of Missing Index, sysname,>");

            entity.HasIndex(e => new { e.LineName, e.StampTime }, "NonClusteredIndex-20200921-161500");

            entity.HasIndex(e => new { e.SerialNo, e.StampTime, e.LineName }, "NonClusteredIndex-20210426-104700");

            entity.HasIndex(e => e.StampTime, "NonClusteredIndex-20211216-094839");

            entity.Property(e => e.Brazing).HasMaxLength(50);
            entity.Property(e => e.EmpCode).HasMaxLength(50);
            entity.Property(e => e.LineName).HasMaxLength(50);
            entity.Property(e => e.SerialNo).HasMaxLength(50);
            entity.Property(e => e.StampTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
