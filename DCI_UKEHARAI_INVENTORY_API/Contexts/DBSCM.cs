﻿using System;
using System.Collections.Generic;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DCI_UKEHARAI_INVENTORY_API.Contexts;

public partial class DBSCM : DbContext
{
    public DBSCM()
    {
    }

    public DBSCM(DbContextOptions<DBSCM> options)
        : base(options)
    {
    }

    public virtual DbSet<AlGsdActpln> AlGsdActplns { get; set; }

    public virtual DbSet<AlGsdCurpln> AlGsdCurplns { get; set; }

    public virtual DbSet<AlSaleForecaseMonth> AlSaleForecaseMonths { get; set; }

    public virtual DbSet<DictMstr> DictMstrs { get; set; }

    public virtual DbSet<EkbWipPartStock> EkbWipPartStocks { get; set; }

    public virtual DbSet<PnCompressor> PnCompressors { get; set; }

    public virtual DbSet<UkeCurpln> UkeCurplns { get; set; }

    public virtual DbSet<WmsMdw27ModelMaster> WmsMdw27ModelMasters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.86;Database=dbSCM;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<AlGsdActpln>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("AL_GSD_ACTPLN");

            entity.HasIndex(e => new { e.Wcno, e.Prdymd, e.Model }, "NonClusteredIndex-20200225-102253");

            entity.Property(e => e.Cdate)
                .HasColumnType("datetime")
                .HasColumnName("CDATE");
            entity.Property(e => e.DataDate)
                .HasColumnType("datetime")
                .HasColumnName("dataDate");
            entity.Property(e => e.DataType)
                .HasMaxLength(1)
                .HasColumnName("DATA_TYPE");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("MODEL");
            entity.Property(e => e.Prdymd)
                .HasMaxLength(8)
                .HasColumnName("PRDYMD");
            entity.Property(e => e.Qty)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("QTY");
            entity.Property(e => e.Udate)
                .HasColumnType("datetime")
                .HasColumnName("UDATE");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("USERID");
            entity.Property(e => e.Wcno).HasColumnName("WCNO");
        });

        modelBuilder.Entity<AlGsdCurpln>(entity =>
        {
            entity.HasKey(e => new { e.Wcno, e.Prdym, e.Model, e.Sebango });

            entity.ToTable("AL_GSD_CURPLN");

            entity.HasIndex(e => new { e.Wcno, e.Prdym, e.Model }, "IX_AL_GSD_CURPLN");

            entity.Property(e => e.Wcno).HasColumnName("WCNO");
            entity.Property(e => e.Prdym)
                .HasMaxLength(6)
                .HasColumnName("PRDYM");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("MODEL");
            entity.Property(e => e.Sebango).HasColumnName("SEBANGO");
            entity.Property(e => e.Cdate)
                .HasColumnType("datetime")
                .HasColumnName("CDATE");
            entity.Property(e => e.DataDate)
                .HasColumnType("datetime")
                .HasColumnName("dataDate");
            entity.Property(e => e.Day01)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY01");
            entity.Property(e => e.Day02)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY02");
            entity.Property(e => e.Day03)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY03");
            entity.Property(e => e.Day04)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY04");
            entity.Property(e => e.Day05)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY05");
            entity.Property(e => e.Day06)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY06");
            entity.Property(e => e.Day07)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY07");
            entity.Property(e => e.Day08)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY08");
            entity.Property(e => e.Day09)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY09");
            entity.Property(e => e.Day10)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY10");
            entity.Property(e => e.Day11)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY11");
            entity.Property(e => e.Day12)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY12");
            entity.Property(e => e.Day13)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY13");
            entity.Property(e => e.Day14)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY14");
            entity.Property(e => e.Day15)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY15");
            entity.Property(e => e.Day16)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY16");
            entity.Property(e => e.Day17)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY17");
            entity.Property(e => e.Day18)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY18");
            entity.Property(e => e.Day19)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY19");
            entity.Property(e => e.Day20)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY20");
            entity.Property(e => e.Day21)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY21");
            entity.Property(e => e.Day22)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY22");
            entity.Property(e => e.Day23)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY23");
            entity.Property(e => e.Day24)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY24");
            entity.Property(e => e.Day25)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY25");
            entity.Property(e => e.Day26)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY26");
            entity.Property(e => e.Day27)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY27");
            entity.Property(e => e.Day28)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY28");
            entity.Property(e => e.Day29)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY29");
            entity.Property(e => e.Day30)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY30");
            entity.Property(e => e.Day31)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY31");
            entity.Property(e => e.Lrev)
                .HasDefaultValueSql("((1))")
                .HasColumnName("LREV");
            entity.Property(e => e.Rev)
                .HasDefaultValueSql("((1))")
                .HasColumnName("REV");
            entity.Property(e => e.Salmodel)
                .HasMaxLength(50)
                .HasColumnName("SALMODEL");
            entity.Property(e => e.Udate)
                .HasColumnType("datetime")
                .HasColumnName("UDATE");
            entity.Property(e => e.YmQty)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("YM_QTY");
        });

        modelBuilder.Entity<AlSaleForecaseMonth>(entity =>
        {
            entity.ToTable("AL_SaleForecaseMonth");

            entity.HasIndex(e => new { e.Ym, e.Lrev }, "IX_AL_SaleForecaseMonth_1").IsDescending();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Customer).HasMaxLength(30);
            entity.Property(e => e.D01).HasDefaultValueSql("((0))");
            entity.Property(e => e.D02).HasDefaultValueSql("((0))");
            entity.Property(e => e.D03).HasDefaultValueSql("((0))");
            entity.Property(e => e.D04).HasDefaultValueSql("((0))");
            entity.Property(e => e.D05).HasDefaultValueSql("((0))");
            entity.Property(e => e.D06).HasDefaultValueSql("((0))");
            entity.Property(e => e.D07).HasDefaultValueSql("((0))");
            entity.Property(e => e.D08).HasDefaultValueSql("((0))");
            entity.Property(e => e.D09).HasDefaultValueSql("((0))");
            entity.Property(e => e.D10).HasDefaultValueSql("((0))");
            entity.Property(e => e.D11).HasDefaultValueSql("((0))");
            entity.Property(e => e.D12).HasDefaultValueSql("((0))");
            entity.Property(e => e.D13).HasDefaultValueSql("((0))");
            entity.Property(e => e.D14).HasDefaultValueSql("((0))");
            entity.Property(e => e.D15).HasDefaultValueSql("((0))");
            entity.Property(e => e.D16).HasDefaultValueSql("((0))");
            entity.Property(e => e.D17).HasDefaultValueSql("((0))");
            entity.Property(e => e.D18).HasDefaultValueSql("((0))");
            entity.Property(e => e.D19).HasDefaultValueSql("((0))");
            entity.Property(e => e.D20).HasDefaultValueSql("((0))");
            entity.Property(e => e.D21).HasDefaultValueSql("((0))");
            entity.Property(e => e.D22).HasDefaultValueSql("((0))");
            entity.Property(e => e.D23).HasDefaultValueSql("((0))");
            entity.Property(e => e.D24).HasDefaultValueSql("((0))");
            entity.Property(e => e.D25).HasDefaultValueSql("((0))");
            entity.Property(e => e.D26).HasDefaultValueSql("((0))");
            entity.Property(e => e.D27).HasDefaultValueSql("((0))");
            entity.Property(e => e.D28).HasDefaultValueSql("((0))");
            entity.Property(e => e.D29).HasDefaultValueSql("((0))");
            entity.Property(e => e.D30).HasDefaultValueSql("((0))");
            entity.Property(e => e.D31).HasDefaultValueSql("((0))");
            entity.Property(e => e.Diameter).HasMaxLength(20);
            entity.Property(e => e.Lrev)
                .HasMaxLength(3)
                .HasColumnName("LREV");
            entity.Property(e => e.ModelCode).HasMaxLength(50);
            entity.Property(e => e.ModelName).HasMaxLength(50);
            entity.Property(e => e.Pltype)
                .HasMaxLength(30)
                .HasColumnName("PLTYPE");
            entity.Property(e => e.Rev)
                .HasMaxLength(3)
                .HasColumnName("REV");
            entity.Property(e => e.Sebango).HasMaxLength(30);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ym)
                .HasMaxLength(6)
                .HasColumnName("YM");
        });

        modelBuilder.Entity<DictMstr>(entity =>
        {
            entity.HasKey(e => e.DictId);

            entity.ToTable("DictMstr");

            entity.Property(e => e.DictId).HasColumnName("DICT_ID");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("CODE");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.DictStatus)
                .HasMaxLength(20)
                .HasColumnName("DICT_STATUS");
            entity.Property(e => e.DictSystem)
                .HasMaxLength(20)
                .HasColumnName("DICT_SYSTEM");
            entity.Property(e => e.DictType)
                .HasMaxLength(50)
                .HasColumnName("DICT_TYPE");
            entity.Property(e => e.Note)
                .HasMaxLength(50)
                .HasColumnName("NOTE");
            entity.Property(e => e.Ref1)
                .HasMaxLength(20)
                .HasColumnName("REF1");
            entity.Property(e => e.Ref2)
                .HasMaxLength(20)
                .HasColumnName("REF2");
            entity.Property(e => e.Ref3)
                .HasMaxLength(20)
                .HasColumnName("REF3");
            entity.Property(e => e.Ref4)
                .HasMaxLength(20)
                .HasColumnName("REF4");
            entity.Property(e => e.RefCode)
                .HasMaxLength(200)
                .HasColumnName("REF_CODE");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .HasColumnName("UPDATE_BY");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DATE");
        });

        modelBuilder.Entity<EkbWipPartStock>(entity =>
        {
            entity.HasKey(e => new { e.Ym, e.Wcno, e.Partno, e.Cm }).HasName("PK_EKB_LINE_STOCK_MONIOTR");

            entity.ToTable("EKB_WIP_PART_STOCK");

            entity.Property(e => e.Ym)
                .HasMaxLength(8)
                .HasColumnName("YM");
            entity.Property(e => e.Wcno)
                .HasMaxLength(3)
                .HasColumnName("WCNO");
            entity.Property(e => e.Partno)
                .HasMaxLength(25)
                .HasColumnName("PARTNO");
            entity.Property(e => e.Cm)
                .HasMaxLength(2)
                .HasColumnName("CM");
            entity.Property(e => e.Bal)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("BAL");
            entity.Property(e => e.Issqty)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("ISSQTY");
            entity.Property(e => e.Lbal)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("LBAL");
            entity.Property(e => e.Ptype).HasMaxLength(15);
            entity.Property(e => e.Recqty)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("RECQTY");
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PnCompressor>(entity =>
        {
            entity.HasKey(e => new { e.ModelCode, e.Model, e.Line });

            entity.ToTable("PN_Compressor");

            entity.Property(e => e.ModelCode).HasMaxLength(20);
            entity.Property(e => e.Model).HasMaxLength(250);
            entity.Property(e => e.CreateBy).HasMaxLength(20);
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.ModelGroup).HasMaxLength(50);
            entity.Property(e => e.ModelType)
                .HasMaxLength(10)
                .HasDefaultValueSql("(N'1YC')");
            entity.Property(e => e.Rmk1)
                .HasMaxLength(250)
                .HasComment("Model customer")
                .HasColumnName("rmk1");
            entity.Property(e => e.Rmk10)
                .HasMaxLength(250)
                .HasComment("data for check original line (backflush)")
                .HasColumnName("rmk10");
            entity.Property(e => e.Rmk2)
                .HasMaxLength(250)
                .HasComment("Pallate")
                .HasColumnName("rmk2");
            entity.Property(e => e.Rmk3)
                .HasMaxLength(250)
                .HasComment("Magnet")
                .HasColumnName("rmk3");
            entity.Property(e => e.Rmk4)
                .HasMaxLength(250)
                .HasColumnName("rmk4");
            entity.Property(e => e.Rmk5)
                .HasMaxLength(250)
                .HasComment("bit for check terminal cover")
                .HasColumnName("rmk5");
            entity.Property(e => e.Rmk6)
                .HasMaxLength(250)
                .HasColumnName("rmk6");
            entity.Property(e => e.Rmk7)
                .HasMaxLength(250)
                .HasColumnName("rmk7");
            entity.Property(e => e.Rmk8)
                .HasMaxLength(250)
                .HasColumnName("rmk8");
            entity.Property(e => e.Rmk9)
                .HasMaxLength(250)
                .HasColumnName("rmk9");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("(N'active')");
            entity.Property(e => e.UpdateBy).HasMaxLength(20);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<UkeCurpln>(entity =>
        {
            entity.HasKey(e => new { e.Prdym, e.Model, e.Wcno, e.Sebango });

            entity.ToTable("UKE_CURPLN");

            entity.Property(e => e.Prdym)
                .HasMaxLength(6)
                .HasColumnName("PRDYM");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("MODEL");
            entity.Property(e => e.Wcno).HasColumnName("WCNO");
            entity.Property(e => e.Sebango).HasColumnName("SEBANGO");
            entity.Property(e => e.Cdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CDATE");
            entity.Property(e => e.DataDate)
                .HasColumnType("datetime")
                .HasColumnName("dataDate");
            entity.Property(e => e.Day01)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY01");
            entity.Property(e => e.Day02)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY02");
            entity.Property(e => e.Day03)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY03");
            entity.Property(e => e.Day04)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY04");
            entity.Property(e => e.Day05)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY05");
            entity.Property(e => e.Day06)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY06");
            entity.Property(e => e.Day07)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY07");
            entity.Property(e => e.Day08)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY08");
            entity.Property(e => e.Day09)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY09");
            entity.Property(e => e.Day10)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY10");
            entity.Property(e => e.Day11)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY11");
            entity.Property(e => e.Day12)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY12");
            entity.Property(e => e.Day13)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY13");
            entity.Property(e => e.Day14)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY14");
            entity.Property(e => e.Day15)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY15");
            entity.Property(e => e.Day16)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY16");
            entity.Property(e => e.Day17)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY17");
            entity.Property(e => e.Day18)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY18");
            entity.Property(e => e.Day19)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY19");
            entity.Property(e => e.Day20)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY20");
            entity.Property(e => e.Day21)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY21");
            entity.Property(e => e.Day22)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY22");
            entity.Property(e => e.Day23)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY23");
            entity.Property(e => e.Day24)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY24");
            entity.Property(e => e.Day25)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY25");
            entity.Property(e => e.Day26)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY26");
            entity.Property(e => e.Day27)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY27");
            entity.Property(e => e.Day28)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY28");
            entity.Property(e => e.Day29)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY29");
            entity.Property(e => e.Day30)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY30");
            entity.Property(e => e.Day31)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DAY31");
            entity.Property(e => e.Lrev)
                .HasDefaultValueSql("((1))")
                .HasColumnName("LREV");
            entity.Property(e => e.Rev)
                .HasDefaultValueSql("((1))")
                .HasColumnName("REV");
            entity.Property(e => e.RowNum).HasDefaultValueSql("((0))");
            entity.Property(e => e.Salmodel)
                .HasMaxLength(50)
                .HasDefaultValueSql("('')")
                .HasColumnName("SALMODEL");
            entity.Property(e => e.Udate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("UDATE");
            entity.Property(e => e.YmQty)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("YM_QTY");
        });

        modelBuilder.Entity<WmsMdw27ModelMaster>(entity =>
        {
            entity.HasKey(e => new { e.Model, e.Modelgroup, e.Pltype, e.Strloc, e.Rev, e.Lrev, e.Strdate, e.Enddate, e.Remark, e.Sebango, e.Diameter }).HasName("PK_WMS_MDW27_MODEL_MASTER_1");

            entity.ToTable("WMS_MDW27_MODEL_MASTER");

            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("MODEL");
            entity.Property(e => e.Modelgroup)
                .HasMaxLength(3)
                .HasColumnName("MODELGROUP");
            entity.Property(e => e.Pltype)
                .HasMaxLength(50)
                .HasColumnName("PLTYPE");
            entity.Property(e => e.Strloc)
                .HasMaxLength(50)
                .HasColumnName("STRLOC");
            entity.Property(e => e.Rev).HasColumnName("REV");
            entity.Property(e => e.Lrev).HasColumnName("LREV");
            entity.Property(e => e.Strdate)
                .HasMaxLength(8)
                .HasColumnName("STRDATE");
            entity.Property(e => e.Enddate)
                .HasMaxLength(8)
                .HasColumnName("ENDDATE");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasColumnName("REMARK");
            entity.Property(e => e.Sebango)
                .HasMaxLength(50)
                .HasColumnName("SEBANGO");
            entity.Property(e => e.Diameter)
                .HasMaxLength(50)
                .HasColumnName("DIAMETER");
            entity.Property(e => e.Active)
                .HasMaxLength(20)
                .HasDefaultValueSql("(N'INACTIVE')")
                .HasColumnName("ACTIVE");
            entity.Property(e => e.Area)
                .HasMaxLength(20)
                .HasColumnName("AREA");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("(N'every 8.30 AM')")
                .HasColumnName("CREATE_BY");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("CUSTOMER");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DATE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
