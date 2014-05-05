using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBudget.Web.AspNet.Models
{
    public class CategoryViewModel
    {
        public string BudgetId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BudgetName { get; set; }
    }
}