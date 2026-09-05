using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarFix.Domain.ConstantsClass
{
    public static class AuditActions
    {
        public const string CenterBanned = "CenterBanned";
        public const string CenterUnbanned = "CenterUnbanned";
        public const string RefundIssued = "RefundIssued";
        public const string DisputeResolved = "DisputeResolved";
    }
}
