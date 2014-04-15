using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Domain
{
    public class IdProvider
    {
        public string NewAccountId()
        {
            return "Account-" + Guid.NewGuid().ToString().Replace('-', '_');
        }
        public string NewUserId()
        {
            return "User-" + Guid.NewGuid().ToString().Replace('-', '_');
        }


        public string NewBudgetId()
        {
            return "Budget-" + Guid.NewGuid().ToString().Replace('-', '_');
        }

        public string NewCategoryId()
        {
            return "Category-" + Guid.NewGuid().ToString().Replace('-', '_');
        }

        public string NewLineId()
        {
            return "Line-" + Guid.NewGuid().ToString().Replace('-', '_');
        }
    }
}
