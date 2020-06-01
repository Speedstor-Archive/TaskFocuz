using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFocuz.ViewModels.Commands;

namespace TaskFocuz.ViewModels
{
    public class ViewModelBase
    {
        public SimpleCommand SimpleCommand { get; set; }

        public ViewModelBase()
        {
            this.SimpleCommand = new SimpleCommand(this);
        }

        public void SimpleMethod()
        {
            Debug.WriteLine("Auto generated method");
        }
    }
}
