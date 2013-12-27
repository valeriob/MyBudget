using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public class Amount
    {
        decimal _amount;
        Currency _currency;

        public Amount(Currency _currency, decimal _amount)
        {
            this._currency = _currency;
            this._amount = _amount;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", _currency.Sign, _amount);
        }

        public static implicit operator decimal(Amount amount)
        {
            return amount._amount;
        }

        public Currency GetCurrency()
        {
            return _currency;
        }

    }



}
