using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public class Currency : IEquatable<Currency>
    {
        public string IsoCode { get; private set; }
        public string Sign { get; private set; }
        public string Name { get; private set; }
        
        public Currency(string isoCode, string sign, string name)
        {
            if (string.IsNullOrEmpty(isoCode))
                throw new ArgumentNullException("isoCode");

            IsoCode = isoCode;
            Sign = sign;
            Name = name;
        }

        internal bool Is(string isoCode)
        {
            return IsoCode == isoCode;
        }
        public override string ToString()
        {
            return string.Format("{0}, {1}", IsoCode, Sign);
        }
        public override int GetHashCode()
        {
            return IsoCode.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var other = obj as Currency;
            return !ReferenceEquals(other, null) && IsoCode == other.IsoCode;
        }

        public bool Equals(Currency other)
        {
            return !ReferenceEquals(other, null) && IsoCode == other.IsoCode;
        }

        public static bool operator ==(Currency c1, Currency c2)
        {
            return (ReferenceEquals(c1, null) && ReferenceEquals(c2, null)) || (!ReferenceEquals(c1, null) && c1.Equals(c2));
        }
        public static bool operator !=(Currency c1, Currency c2)
        {
            return !(c1 == c2);
        }
    }

    public static class Currencies
    {
        public static IEnumerable<Currency> GetAll()
        {
            yield return Euro();
            yield return UsaDollar();
        }

        public static Currency Unknown()
        {
            return new Currency("NA", "", "Unknown");
        }

        public static Currency Euro()
        {
            return new Currency("EUR", "€", "Euro");
        }

        public static Currency UsaDollar()
        {
            return new Currency("USD", "$", "United States dollar");
        }
         

        public static Currency Parse(string isoCode)
        {
            return GetAll().Single(s => s.Is(isoCode));
        }
    }
}
