using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case.Data.Model.Notification
{
    public class MailRequest : BaseNotification
    {
        public string Subject { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
