using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarFix.Domain.Enums
{
    public enum RepairOrderStatus
    {
        Accepted,
        InProgress,
        PendingScopeChange,
        ReadyForPickup,
        Completed,
        Cancelled
    }
}
