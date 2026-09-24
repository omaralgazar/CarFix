using CarFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infrastructure.Persistence
{
    public class CarFixDbContext : DbContext
    {
        public CarFixDbContext(DbContextOptions<CarFixDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<CenterCapability> CenterCapabilities { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<RepairOffer> RepairOffers { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<ScopeChangeRequest> ScopeChangeRequests { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<TokenTransaction> TokenTransactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarFixDbContext).Assembly);
        }
    }
}