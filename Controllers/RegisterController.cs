using Microsoft.AspNetCore.Mvc;
using Register.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using Register.Services;
using Microsoft.Extensions.Configuration;

namespace Register.Controllers
{
    public class RegisterController : Controller
    {
        private readonly DBTaxes_RegisterContext DBContext;
        private readonly EmailService email;
        public RegisterController(DBTaxes_RegisterContext context,IConfiguration config)
        {
            DBContext = context;
            email = new EmailService(config);
        }


        
        public IActionResult Taxes_Register()
        {
            ViewBag.Message = null;
            return View();
        }

        [HttpPost]
        public IActionResult Taxes_Register(TaxesRegister taxes)
        {
            //Condition to know if count has reach 300 records
            if (GetTotal() != true)
            {
                //Verify if Model is completed
                if (!ModelState.IsValid)
                {
                    return View();
                }
                else
                {
                    ViewBag.ErroDate = null;
                    AddTaxesDate(taxes);
                }
            }
            else
            {
                ViewBag.Alert = "Lo sentimos, por el momento hay una sobrecarga de citas";
            }
            return View();
        }

        public void AddTaxesDate(TaxesRegister taxes)
        {
            if (Taxes_Date_Exist(taxes.FechaRegistro)!=true)
            {
                DBContext.TaxesRegister.Add(taxes);
                DBContext.SaveChanges();
               
                email.SendEmail(taxes.Correo,String.Concat(taxes.Nombre," ",taxes.Apellidos),taxes.FechaRegistro);
                //SendEmail(taxes);
                ViewBag.Message = "Register Created";
            }

        }
        public bool GetTotal()
        {
            bool result = false;
            if (DBContext.TaxesRegister.ToList().Count >=1000 )
            {
                result = true;
            }
            return result;
        }

        public bool Taxes_Date_Exist(DateTime? fecha) //Function to know if a taxes_date already existed
        {
            bool exist = false;
            var findDates = DBContext.TaxesRegister.Where(t => t.FechaRegistro.Equals(fecha));

            if (findDates.Count()>=1)
            {
                exist = true;
                ViewBag.ErroDate = "Lo sentimos, pero ya hay una cita a las " + fecha.ToString();
            }

            return exist;
        }

        /*public void SendEmail(TaxesRegister taxes)
        {
           try
            {
                var mailMessage = new MimeMessage();

                mailMessage.From.Add(MailboxAddress.Parse("joseluisinho@outlook.com"));

                mailMessage.To.Add(MailboxAddress.Parse(taxes.Correo));

                mailMessage.Subject = "Cita Confirmada";

                mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)

                {

                    Text = "Hola " + taxes.Nombre + "<br/>"
                    + " Hemos registrado su cita de manera exitosa. <br/>" +
                    "Datos de la cita.<br/>" +
                    "<b>Nombre:</b>" + String.Concat(taxes.Nombre, taxes.Apellidos) + "<br/>" +
                    "<b>Fecha:</b>" + taxes.FechaRegistro.ToString()
                };

                using var smtp = new SmtpClient();
                
                
                //smtp.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                //smtp.Authenticate("pruebait1103@gmail.com", "Valonqar123");
                smtp.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("joseluisinho@outlook.com", "America123");
                smtp.Send(mailMessage);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {

            }


        }*/
    }
}
