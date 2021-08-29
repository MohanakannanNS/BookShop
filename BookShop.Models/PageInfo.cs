using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models
{
    public class PageInfo
    {
        public int TotalItem { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public string URLParam { get; set; }
        public int TotalPage => (int)Math.Ceiling((decimal)TotalItem / ItemsPerPage);
    }
}
