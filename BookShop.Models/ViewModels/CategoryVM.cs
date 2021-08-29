using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModels
{
    public class CategoryVM
    {
        public IEnumerable<Category> Category { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
