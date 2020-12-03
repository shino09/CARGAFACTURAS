using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB_CLIENTE.Models
{
    public class ClsRutinasGenerales
    {
        public void eLog(string mensaje)
        {

            String ArchLog = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"ProcesoAutomatico.log";

            string fecha = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");

            mensaje = fecha + "; " + mensaje + ";";

            Console.WriteLine(mensaje);

            System.IO.StreamWriter sw = new System.IO.StreamWriter(ArchLog, true);
            sw.WriteLine(mensaje);
            sw.Close();
        }

        public DateTime formatearFecha(int date)
        {
            int d = date % 100;
            int m = (date / 100) % 100;
            int y = date / 10000;

            return new DateTime(y, m, d);
        }


        public DateTime formatearFecha2(int date)
        {
            int d = date % 100;
            int m = (date / 100) % 100;
            int y = date / 10000;

            return new DateTime(d, m, y);
        }

        public decimal truncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        /*public DateTime getFechaInicial(int mes)
        {
            DateTime fecha = new DateTime();
            fecha = fecha.AddDays(01);
            fecha = fecha.AddMonths(mes);
            fecha = fecha.AddYears(Constante.YEAR);
            return fecha;
        }*/

        public DateTime agregarFecha(DateTime fecha, int dia, int mes, int year)
        {
            fecha = fecha.AddDays(dia - 1);
            fecha = fecha.AddMonths(mes - 1);
            fecha = fecha.AddYears(year - 1);

            return fecha;
        }

        public string getString(string valor)
        {
            string resultado = "";
            string[] resultadoAux = valor.Split('.');

            if (resultadoAux.Length >= 1)
            {
                resultado = resultadoAux[0];
            }
            return resultado;
        }

    }
}