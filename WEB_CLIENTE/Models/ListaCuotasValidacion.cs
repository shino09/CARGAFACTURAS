using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB_CLIENTE.Models
{
    public class ListaCuotasValidacion
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public int numeroCuota { get; set; }
        public decimal monto { get; set; }


    }
}