using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public WalletOwnerType OwnerType { get; set; }
        public decimal Balance { get; set; }
        public decimal HeldBalance { get; set; }
        public int TokenBalance { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();

        public Wallet() { }
    }

}
