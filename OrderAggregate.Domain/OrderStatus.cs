using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public enum OrderStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2
}
