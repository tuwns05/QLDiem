using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLDiem.Models;

public partial class QuanLyDiemContext : DbContext
{
    public QuanLyDiemContext()
    {
    }

    public QuanLyDiemContext(DbContextOptions<QuanLyDiemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DangKyMonHoc> DangKyMonHocs { get; set; }

    public virtual DbSet<Diem> Diems { get; set; }

    public virtual DbSet<DiemRenLuyen> DiemRenLuyens { get; set; }

    public virtual DbSet<Gpa> Gpas { get; set; }

    public virtual DbSet<HocPhan> HocPhans { get; set; }

    public virtual DbSet<LopHocPhan> LopHocPhans { get; set; }

    public virtual DbSet<MoLopHocPhan> MoLopHocPhans { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-IUIOQFKS\\VIETTUAN;Database=QuanLyDiem;Trusted_Connection=True;TrustServerCertificate=True");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DangKyMonHoc>(entity =>
        {
            entity.HasKey(e => e.MaDk).HasName("PK__DangKyMo__2725866C29C5DE9C");

            entity.ToTable("DangKyMonHoc");

            entity.Property(e => e.MaDk).HasColumnName("MaDK");
            entity.Property(e => e.MaLopHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaLopHP");
            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaLopHpNavigation).WithMany(p => p.DangKyMonHocs)
                .HasForeignKey(d => d.MaLopHp)
                .HasConstraintName("FK_DangKyMonHoc_LopHocPhan");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.DangKyMonHocs)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK_DangKyMonHoc_SinhVien");
        });

        modelBuilder.Entity<Diem>(entity =>
        {
            entity.HasKey(e => e.MaDiem).HasName("PK__Diem__33326025797ED59A");

            entity.ToTable("Diem");

            entity.Property(e => e.DiemCk).HasColumnName("DiemCK");
            entity.Property(e => e.DiemQt).HasColumnName("DiemQT");
            entity.Property(e => e.DiemTk).HasColumnName("DiemTK");
            entity.Property(e => e.MaLopHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaLopHP");
            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");

            entity.HasOne(d => d.MaLopHpNavigation).WithMany(p => p.Diems)
                .HasForeignKey(d => d.MaLopHp)
                .HasConstraintName("FK_Diem_LopHocPhan");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Diems)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK_Diem_SV");
        });

        modelBuilder.Entity<DiemRenLuyen>(entity =>
        {
            entity.HasKey(e => e.MaDrl).HasName("PK__DiemRenL__3D88F94D4351E073");

            entity.ToTable("DiemRenLuyen");

            entity.HasIndex(e => new { e.MaSv, e.HocKy, e.NamHoc }, "UQ_DRL").IsUnique();

            entity.Property(e => e.MaDrl).HasColumnName("MaDRL");
            entity.Property(e => e.DiemRl).HasColumnName("DiemRL");
            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.DiemRenLuyens)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK_DRL_SV");
        });

        modelBuilder.Entity<Gpa>(entity =>
        {
            entity.HasKey(e => e.MaGpa).HasName("PK__GPA__3CD633E99D51AAEB");

            entity.ToTable("GPA");

            entity.HasIndex(e => new { e.MaSv, e.HocKy, e.NamHoc }, "UQ_GPA").IsUnique();

            entity.Property(e => e.MaGpa).HasColumnName("MaGPA");
            entity.Property(e => e.GpaHocKy).HasColumnName("GPA_HocKy");
            entity.Property(e => e.GpaTichLuy).HasColumnName("GPA_TichLuy");
            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoTcHocKy).HasColumnName("SoTC_HocKy");
            entity.Property(e => e.SoTcTichLuy).HasColumnName("SoTC_TichLuy");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Gpas)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK_GPA_SV");
        });

        modelBuilder.Entity<HocPhan>(entity =>
        {
            entity.HasKey(e => e.MaHp).HasName("PK__HocPhan__2725A6EC01B22655");

            entity.ToTable("HocPhan");

            entity.Property(e => e.MaHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaHP");
            entity.Property(e => e.TenHp)
                .HasMaxLength(100)
                .HasColumnName("TenHP");
        });

        modelBuilder.Entity<LopHocPhan>(entity =>
        {
            entity.HasKey(e => e.MaLopHp);

            entity.ToTable("LopHocPhan");

            entity.Property(e => e.MaLopHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaLopHP");
            entity.Property(e => e.MaHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaHP");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.MaHpNavigation).WithMany(p => p.LopHocPhans)
                .HasForeignKey(d => d.MaHp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHP_HocPhan");
        });

        modelBuilder.Entity<MoLopHocPhan>(entity =>
        {
            entity.HasKey(e => e.MaMoLop).HasName("PK__MoLopHoc__180232613E41476C");

            entity.ToTable("MoLopHocPhan");

            entity.Property(e => e.MaHp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaHP");
            entity.Property(e => e.NamHoc).HasMaxLength(20);
            entity.Property(e => e.NgayDong).HasColumnType("datetime");
            entity.Property(e => e.NgayMo).HasColumnType("datetime");

            entity.HasOne(d => d.MaHpNavigation).WithMany(p => p.MoLopHocPhans)
                .HasForeignKey(d => d.MaHp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MoLopHocPhan_HocPhan");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MaSv).HasName("PK__SinhVien__2725081A72F82F04");

            entity.ToTable("SinhVien");

            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.Lop).HasMaxLength(50);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__272500708F005C56");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC02BF4729D").IsUnique();

            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.MaSv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaSV");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
