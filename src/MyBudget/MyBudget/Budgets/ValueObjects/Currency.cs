using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Budgets.ValueObjects
{
    public class Currency
    {
        string _name;
        string _symbol;
        public Currency(string symbol, string name)
        {
            _symbol = symbol;
            _name = name;
        }
    }

    public static class Currencies
    {
        public static IEnumerable<Currency> GetAll()
        {
            yield return Euro();
        }
        public static Currency Euro()
        {
            return new Currency("€", "Euro");
        }
    }
}
