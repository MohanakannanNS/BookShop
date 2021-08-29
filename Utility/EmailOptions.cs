using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class EmailOptions
    {
        public string SendGridKey { get; set; }
        public string SendGridName { get; set; }
    }
}
