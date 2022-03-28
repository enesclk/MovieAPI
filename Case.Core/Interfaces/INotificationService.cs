using Case.Data.Model.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(BaseNotification notification);
    }
}
