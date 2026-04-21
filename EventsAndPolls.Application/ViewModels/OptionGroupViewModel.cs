using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsAndPolls.Application.ViewModels
{
     public class OptionGroupViewModel
     {
          public string GroupName { get; set; }

          public List<string> Options { get; set; } = new();
     }
}
