using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Domain.ValueObjects
{
    public record Money(int AmountMinor, string CurrencyCode);
}
