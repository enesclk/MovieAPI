using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case.Data.Model.Notification
{
    public abstract class BaseNotification
    {
        public string To { get; set; }
        public string Message { get; set; }
    }
}
