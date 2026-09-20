using System;
using System.Collections.Generic;

namespace CarFix.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public RepairOrder Order { get; set; }
        public decimal BaseCost { get; set; }
        public decimal ExtraScopeCost { get; set; } = 0;
        public decimal PenaltyDeduction { get; set; } = 0;
        public decimal InspectionFee { get; set; } = 0;
        public decimal PlatformCommission { get; set; }
        public decimal NetAmountToCenter { get; set; }
        public decimal TotalPaidByCustomer { get; set; }
        public int TokensEarned { get; set; } = 0;
        public int TokensRedeemed { get; set; } = 0;
        public decimal TokenDiscountAmount { get; set; } = 0;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
        public ICollection<TokenTransaction> TokenTransactions { get; set; } = new List<TokenTransaction>();

    }
}