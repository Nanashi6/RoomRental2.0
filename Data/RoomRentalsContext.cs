using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RoomRental.Models;

namespace RoomRental.Data;

public partial class RoomRentalsContext : IdentityDbContext<User>
{
    public RoomRentalsContext()
    {
    }

    public RoomRentalsContext(DbContextOptions<RoomRentalsContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomImage> RoomImages { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.BuildingId).HasName("PK__Building__979FD1CD3DCCFF84");

            entity.HasIndex(e => e.OwnerOrganizationId, "IX_Buildings_ownerOrganizationId");

            entity.Property(e => e.BuildingId).HasColumnName("buildingId");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.FloorPlan)
                .IsUnicode(false)
                .HasColumnName("floorPlan");
            entity.Property(e => e.Floors).HasColumnName("floors");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.OwnerOrganizationId).HasColumnName("ownerOrganizationId");
            entity.Property(e => e.PostalAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("postalAddress");

            entity.HasOne(d => d.OwnerOrganization).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.OwnerOrganizationId)
                .HasConstraintName("FK__Buildings__owner__37703C52");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__DDA6423A7D292366");

            entity.HasIndex(e => e.RentalOrganizationId, "IX_Invoices_rentalOrganizationId");

            entity.HasIndex(e => e.ResponsiblePersonId, "IX_Invoices_responsiblePersonId");

            entity.HasIndex(e => e.RoomId, "IX_Invoices_roomId");

            entity.Property(e => e.InvoiceId).HasColumnName("invoiceId");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.ConclusionDate).HasColumnName("conclusionDate");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentDate");
            entity.Property(e => e.RentalOrganizationId).HasColumnName("rentalOrganizationId");
            entity.Property(e => e.ResponsiblePersonId).HasColumnName("responsiblePersonId");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.RentalOrganization).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.RentalOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__rental__42E1EEFE");

            entity.HasOne(d => d.ResponsiblePerson).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ResponsiblePersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__respon__44CA3770");

            entity.HasOne(d => d.Room).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__roomId__43D61337");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("PK__Organiza__29747D5940E6BB82");

            entity.Property(e => e.OrganizationId).HasColumnName("organizationId");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PostalAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("postalAddress");
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.RentalId).HasName("PK__Rentals__1D4A79C91609EF5D");

            entity.HasIndex(e => e.RentalOrganizationId, "IX_Rentals_rentalOrganizationId");

            entity.HasIndex(e => e.RoomId, "IX_Rentals_roomId");

            entity.Property(e => e.RentalId).HasColumnName("rentalId");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CheckInDate).HasColumnName("checkInDate");
            entity.Property(e => e.CheckOutDate).HasColumnName("checkOutDate");
            entity.Property(e => e.RentalOrganizationId).HasColumnName("rentalOrganizationId");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.RentalOrganization).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.RentalOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rentals__rentalO__3E1D39E1");

            entity.HasOne(d => d.Room).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rentals__roomId__3D2915A8");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__6C3BF5BEA3A3945D");

            entity.HasIndex(e => e.BuildingId, "IX_Rooms_buildingId");

            entity.Property(e => e.RoomId).HasColumnName("roomId");
            entity.Property(e => e.Area)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("area");
            entity.Property(e => e.BuildingId).HasColumnName("buildingId");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.RoomNumber).HasColumnName("roomNumber");

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.BuildingId)
                .HasConstraintName("FK__Rooms__buildingI__3A4CA8FD");
        });

        modelBuilder.Entity<RoomImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__RoomImag__336E9B55CAABD719");

            entity.HasIndex(e => e.RoomId, "IX_RoomImages_roomId");

            entity.Property(e => e.ImageId).HasColumnName("imageId");
            entity.Property(e => e.ImagePath)
                .IsUnicode(false)
                .HasColumnName("imagePath");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomImages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__RoomImage__roomI__47A6A41B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
