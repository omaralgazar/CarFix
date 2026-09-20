using CarFix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Entities
{
    public class TokenTransaction
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public TokenTransactionType Type { get; set; }
        public int TokensAmount { get; set; } = 0;
        public Guid? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TokenTransaction() { }
    }
}
