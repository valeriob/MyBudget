using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImporter
{
    public class BudgetChooser
    {
         ProjectionManager _pm;

        public BudgetChooser(ProjectionManager pm)
        {
            _pm = pm;
        }


        public BudgetChooserResult ChooseBudget()
        {
            var tu = _pm.GetUsersList().AllUsers();
            tu.Wait();

            var userId = ChooseUserId(tu.Result);

            var budgets = _pm.GetBudgetsList().GetBudgetsUserCanView(new MyBudget.Domain.Users.UserId(userId));

            var budgetId = ChooseBudgetId(budgets);

            return new BudgetChooserResult 
            {
                UserId = userId,
                BudgetId = budgetId,
            };

        }

        string ChooseUserId(IEnumerable<User> users)
        {
            if (users.Count() == 1)
                return users.Select(s => s.Id).Single();

            var array = users.ToArray();
            for (int i = 0; i < array.Length; i++)
                Console.WriteLine("{0} : {1}", i, array[i].UserName);

            int index = -1;
            while (int.TryParse(Console.ReadLine(), out index) == false || index < 0 || array.Length <= index) ;

            return array[index].Id;
        }

        string ChooseBudgetId(IEnumerable<Budget> budgets)
        {
            if (budgets.Count() == 1)
                return budgets.Select(s => s.Id).Single();

            var array = budgets.ToArray();
            for (int i = 0; i < array.Length; i++)
                Console.WriteLine("{0} : {1} (owner {2})", i, array[i].Name, array[i].OwnerUsername);

            int index = -1;
            while (int.TryParse(Console.ReadLine(), out index) == false || index < 0 || array.Length <= index) ;

            return array[index].Id;
        }
    }

    public class BudgetChooserResult
    {
        public string BudgetId { get; set; }
        public string UserId { get; set; }
    }

}
