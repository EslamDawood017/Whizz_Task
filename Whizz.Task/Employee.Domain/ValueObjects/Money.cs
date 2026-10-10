namespace Employee.Domain.ValueObjects
{
    public record Money
    {
        public int AmountMinor { get; init; }
        public string CurrencyCode { get; init; } = string.Empty;

        private Money() { }

        public Money(int amountMinor, string currencyCode)
        {
            if (amountMinor < 0)
            {
                throw new ArgumentException("Money amount cannot be negative.", nameof(amountMinor));
            }

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
            {
                throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(currencyCode));
            }

            AmountMinor = amountMinor;
            CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        }

        public static Money Zero(string currencyCode) => new(0, currencyCode);

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(AmountMinor + other.AmountMinor, CurrencyCode);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            if (AmountMinor < other.AmountMinor)
            {
                throw new InvalidOperationException("Resulting money amount cannot be negative.");
            }
            return new Money(AmountMinor - other.AmountMinor, CurrencyCode);
        }

        public Money Multiply(decimal factor)
        {
            if (factor < 0)
            {
                throw new ArgumentException("Multiplication factor cannot be negative.", nameof(factor));
            }
            var newAmount = (int)Math.Round(AmountMinor * factor, MidpointRounding.AwayFromZero);
            return new Money(newAmount, CurrencyCode);
        }

        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

        private void EnsureSameCurrency(Money other)
        {
            if (!string.Equals(CurrencyCode, other.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cannot perform operations between different currencies: '{CurrencyCode}' and '{other.CurrencyCode}'.");
            }
        }

        public decimal ToDecimal() => AmountMinor / 100m;

        public override string ToString() => $"{ToDecimal():N2} {CurrencyCode}";
    }
}
