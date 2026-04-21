using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsAndPolls.Application.ViewModels
{
     public class PollCreateViewModel
     {
          public string Question { get; set; }

          public bool AllowMultipleChoices { get; set; }

          public List<OptionGroupViewModel> Groups { get; set; } = new();
     }
}
