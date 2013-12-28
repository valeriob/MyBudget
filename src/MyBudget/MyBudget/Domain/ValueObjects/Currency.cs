using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public class Currency
    {
        public string IsoCode { get; private set; }
        public string Sign { get; private set; }
        public string Name { get; private set; }
        
        public Currency(string isoCode, string sign, string name)
        {
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
    }

    public static class Currencies
    {
        public static IEnumerable<Currency> GetAll()
        {
            yield return Euro();
            yield return UsaDollar();
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
