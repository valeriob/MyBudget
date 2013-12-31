using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain.ValueObjects
{
    public class Amount : IEquatable<Amount>
    {
        public readonly static Amount Unknown = new Amount(Currencies.Unknown(), 0);

        decimal _amount;
        Currency _currency;

        public Amount(Currency _currency, decimal _amount)
        {
            this._currency = _currency;
            this._amount = _amount;
        }

        public override string ToString()
        {
            if (_amount == 0)
                return "";
            //return string.Format("{0} {1}", _currency.Sign, _amount);
            return string.Format("{1} {0}", _currency.Sign, _amount);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Amount;
            return !ReferenceEquals(other, null) && other._amount == _amount && other._currency == _currency;
        }
        public override int GetHashCode()
        {
            return 3 + 5 * _amount.GetHashCode() + 7 * _currency.GetHashCode();
        }
        public bool Equals(Amount other)
        {
            return !ReferenceEquals(other, null) && other._amount == _amount && other._currency == _currency;
        }

        public static implicit operator decimal(Amount amount)
        {
            return amount._amount;
        }

        public Currency GetCurrency()
        {
            return _currency;
        }

        public static bool operator ==(Amount a1, Amount a2)
        {
            return (ReferenceEquals(a1, null) && ReferenceEquals(a2, null)) || (!ReferenceEquals(a1, null) && a1.Equals(a2));
        }
        public static bool operator !=(Amount a1, Amount a2)
        {
            return !(a1 == a2);
        }

        public static Amount operator +(Amount a1, Amount a2)
        {
            if (a1._currency != a2._currency)
                throw new ArgumentException("cannot sum different currencies");

            return new Amount(a1._currency, a1._amount + a2._amount);
        }


        public static Amount Zero(Currency currency)
        {
            return new Amount(currency, 0);
        }
    }

    public static class AmountExtensions
    {
        public static Amount Sum(this IEnumerable<Amount> source)
        {
            if (source.Any() == false)
                return Amount.Unknown;
            else
            {
                Amount result = null;
                foreach(var a in source)
                {
                    if (result == null)
                        result = a;
                    else
                        result = result + a;
                }
                return result;
            }
            
        }
    }
    
}
