using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Register.Services
{
    interface IEmailService
    {
        void SendEmail(string correo, string fullname, DateTime? date);
    }
}
