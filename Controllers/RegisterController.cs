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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
                    ViewBag.Fechas = null;
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
                AddSaveTaxes(taxes);
                //DBContext.TaxesRegister.Add(taxes);
                //DBContext.SaveChanges();
               
                email.SendEmail(taxes.Correo,String.Concat(taxes.Nombre," ",taxes.Apellidos),taxes.FechaRegistro);
                //SendEmail(taxes);
                ViewBag.Message = "Register Created";
            }  

        }

        public void AddSaveTaxes(TaxesRegister taxes) // Function in case the taxes_date is repeated will update or will Added
        {
            //var searchTaxes = DBContext.TaxesRegister.Where(t=>t.Nombre.Equals(taxes.Nombre)).ToList();
            var searchTaxes = DBContext.TaxesRegister.Where(t => t.Nombre.Equals(taxes.Nombre) && t.Apellidos.Equals(taxes.Apellidos)&& t.Correo.Equals(taxes.Correo)).OrderBy(t=>t.FechaRegistro).ToArray();

            if (searchTaxes.Length>=1)
            {
                if (searchTaxes.Length>1)
                {
                    for (int i =0;i<searchTaxes.Length-1; i++)
                    {
                        DBContext.TaxesRegister.Remove(searchTaxes[i]);
                    }
                    DBContext.SaveChanges();
                }
                var UpdateTaxes = DBContext.TaxesRegister.Where(t => t.Nombre.Equals(taxes.Nombre) && t.Apellidos.Equals(taxes.Apellidos) && t.Correo.Equals(taxes.Correo)).FirstOrDefault();
                UpdateTaxes.FechaRegistro = taxes.FechaRegistro;
                UpdateTaxes.NoTelefono = taxes.NoTelefono;
                //DBContext.TaxesRegister.Remove(searchTaxes.)
            }
            else
            {
                DBContext.TaxesRegister.Add(taxes);
            }

            DBContext.SaveChanges();
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
               string message =  Available_Hours(fecha);
                ViewBag.ErroDate = "Lo sentimos, pero ya hay una cita a las " + fecha.ToString();
                ViewBag.Fechas = message;
            }

            return exist;
        }

        public string Available_Hours(DateTime? fecha)
        {
            var findHours = DBContext.TaxesRegister.Where(t => t.FechaRegistro.Value.Date.Equals(fecha.Value.Date)).ToArray();
            string[] hours = { "9", "10", "11", "12", "15", "16", "17" } ;
            string error = "La horas disponibles para el "+fecha.Value.Date.ToString("dd/MM/yyyy")+ " son: ";
            int pos = 0;

            for (int i=0;i<hours.Length;i++)
            {
                if (findHours[pos].FechaRegistro.Value.Hour.ToString().Equals(hours[i]))
                {
                    if (findHours.Length-1!= pos)
                    {
                        pos++;
                    }

                }
                else
                {
                    error += hours[i] + ":00, ";
                }


            }

            
            return error;
        }

    }
}
