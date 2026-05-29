using System.Reflection;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Domain.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<VerificationToken> VerificationTokens { get; set; }    
    public DbSet<Device> Devices { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<KycProfile> KycProfiles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.Property(x => x.FirstName)
                .HasMaxLength(100);
            user.Property(x => x.LastName)
                .HasMaxLength(100);
            user.Property(x => x.MiddleName)
                .HasMaxLength(100);
            user.Property(x => x.Gender)
                .HasMaxLength(20);
            user.Property(x => x.ProfileImageUrl)
                .HasMaxLength(1000);
            user.Property(x => x.Status)
                .HasConversion<string>();
            user.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<VerificationToken>(token =>
        {
            token.ToTable("VerificationTokens");
            token.HasKey(x => x.Id);
            token.HasIndex(x => x.UserId)
                .IsUnique(true);
            token.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(8);
            token.Property(x => x.ExpiresAt)
                .IsRequired();
            token.Property(x => x.IsUsed)
                .IsRequired();
            token.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.ToTable("RefreshTokens");
            refreshToken.HasKey(x => x.Id);
            refreshToken.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(1000);
            refreshToken.Property(x => x.ExpiresAt)
                .IsRequired();
            refreshToken.Property(x => x.RevokedAt)
                .IsRequired(false);
            refreshToken.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Device>(device =>
        {
            device.ToTable("Devices");
            device.HasKey(x => x.Id);
            device.Property(x => x.DeviceName)
                .IsRequired()
                .HasMaxLength(200);
            device.Property(x => x.DeviceType)
                .IsRequired()
                .HasMaxLength(100);
            device.Property(x => x.IpAddress)
                .HasMaxLength(100);
            device.HasIndex(x => x.UserId)
                .IsUnique(false);
            device.HasOne(x => x.User)
                .WithMany(x => x.Devices)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            device.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<BankAccount>(bankAccount =>
        {
            bankAccount.ToTable("BankAccounts");
            bankAccount.HasKey(x => x.Id);
            bankAccount.Property(x => x.BankName)
                .IsRequired()
                .HasMaxLength(200);
            bankAccount.Property(x => x.AccountName)
                .IsRequired()
                .HasMaxLength(200);
            bankAccount.Property(x => x.AccountNumber)
                .IsRequired()
                .HasMaxLength(20);
            bankAccount.HasIndex(x => x.AccountNumber)
                .IsUnique(false);
            bankAccount.HasOne(x => x.User)
                .WithMany(x => x.BankAccounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            bankAccount.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<UserAddress>(address =>
        {
            address.ToTable("UserAddresses");
            address.HasKey(x => x.Id);
            address.Property(x => x.AddressLine1)
                .IsRequired()
                .HasMaxLength(300);
            address.Property(x => x.AddressLine2)
                .HasMaxLength(300);
            address.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);
            address.Property(x => x.State)
                .IsRequired()
                .HasMaxLength(100);
            address.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);
            address.Property(x => x.PostalCode)
                .HasMaxLength(20);
            address.HasOne(x => x.User)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            address.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<KycProfile>(kyc =>
        {
            kyc.ToTable("KycProfiles");
            kyc.HasKey(x => x.Id);
            kyc.Property(x => x.BVN)
                .HasMaxLength(20);
            kyc.Property(x => x.NIN)
                .HasMaxLength(20);
            kyc.Property(x => x.VerificationProvider)
                .HasMaxLength(200);
            kyc.Property(x => x.SelfieUrl)
                .HasMaxLength(1000);
            kyc.Property(x => x.IdDocumentUrl)
                .HasMaxLength(1000);
            kyc.Property(x => x.RiskScore)
                .HasPrecision(18, 2);
            kyc.Property(x => x.VerificationStatus)
                .HasConversion<string>();
            kyc.HasIndex(x => x.BVN)
                .IsUnique();
            kyc.HasIndex(x => x.NIN)
                .IsUnique();
            kyc.HasOne(x => x.User)
                .WithOne(x => x.KycProfile)
                .HasForeignKey<KycProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            kyc.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // =========================
    // SaveChanges
    // =========================
    public override int SaveChanges()
    {
        ApplyAuditRules();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();

        return base.SaveChangesAsync(cancellationToken);
    }
    private void ApplyAuditRules()
    {
        var entries = ChangeTracker.Entries()
            .Where(entry => entry.Entity is BaseEntity)
            .ToList();
        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entity.ModifiedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}