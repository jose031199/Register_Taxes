using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Register.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
             _config = config;
            
        }

        
        public void SendEmail(string correo, string fullname, DateTime? date)
        {
            try
            {
                var mailMessage = new MimeMessage();

                mailMessage.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));

                mailMessage.To.Add(MailboxAddress.Parse(correo));

                mailMessage.Subject = "Cita Confirmada";

                mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)

                {
                    Text = "<p>Hola " + fullname + "</p><br/>"
                    + "Hemos registrado su cita de manera exitosa. <br/>" +
                    "Datos de la cita.<br/>" +
                    "<b>Nombre:</b>" + fullname + "<br/>" +
                    "<b>Fecha:</b>" + date.ToString()
                };

                using var smtp = new SmtpClient();


                //smtp.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                //smtp.Authenticate("pruebait1103@gmail.com", "Valonqar123");
                smtp.Connect(_config.GetSection("EmailHost").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
                smtp.Send(mailMessage);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
