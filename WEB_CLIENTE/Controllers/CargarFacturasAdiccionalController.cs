using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InterfazCavali;
using System.Xml;
using System.Data;
using WEB_CLIENTE.Models;
using System.IO;
using WEB_CLIENTE.DataSetWSTipoTableAdapters;
using System.Collections;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Extends;
using InterfazCavali.InputService04003;
using dim.rutinas;
using System.Text;
using CsvHelper;
using System.Threading.Tasks;
using System.Diagnostics;
using Aspose.Cells;
using System.Net.Mime;
using System.Net;
using System.IO.Compression;
using Renci.SshNet;
using System.Security.Principal;
using System.Text;
using SharpCifs.Smb;
using System.Net.Mail;
using System.Net.Mime;
namespace WEB_CLIENTE.Controllers
{
    //*****VERSION NUEVA DEL EXCEL, (NO SE OCUPARAN TABLAS AUXILIARES)*****
    public class CargarFacturasAdiccionalController : Controller
    {
        RutinasGenerales rg = new RutinasGenerales();
        //datos consumidor 
        public static string type = null;
        public static string participantCode = null;
        public static string ruc = null;
        public static string processNumber = "0";
        public static string usuario = "USUARIO_WEB";
        //lista de archivo y modelo
        public static FileModel3 archivo = new FileModel3();
        public static bool sinErrores;
        public static string idTransSinErrores;

        //Declarar variables a enviar al WS_4003, cabecera, consumidor,proceso, lista coutas y lista facturas
        public static InterfazCavali.InputService04003.AddInvoiceInformationResponseDetail_Type AddInvoiceInformationResponseDetail_Type = new InterfazCavali.InputService04003.AddInvoiceInformationResponseDetail_Type();
        public static InterfazCavali.InputService04003.CABECERA_Type cabecera = new InterfazCavali.InputService04003.CABECERA_Type();
        public static InterfazCavali.InputService04003.Consumer_Type consumidor = new InterfazCavali.InputService04003.Consumer_Type();
        public static List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> listaFacturas4003 = new List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type>();
        public static List<InterfazCavali.InputService04003.Payment_Type> listaCuotas4003 = new List<InterfazCavali.InputService04003.Payment_Type>();
        public static InterfazCavali.InputService04003.InvoiceCode_Type invoiceCode = new InterfazCavali.InputService04003.InvoiceCode_Type();
        public static InterfazCavali.InputService04003.InvoiceInformationAdditional_Type[] lista2Facturas4003;
        public static InterfazCavali.InputService04003.Payment_Type[] lista2Cuotas4003;

        public ActionResult Index()

        {
            WS_GET_TYPE.WS_TEST ws_get_type = new WS_GET_TYPE.WS_TEST();
            WS_GET_TYPE.Parametro[] lista;
            lista = ws_get_type.GetTipoParticipante();

            return View();
        }

        //Obtener datos del tipo de participante para mostrar en select type
        public ActionResult getType()
        {
            try
            {
                //Enviar lista del tipo de consumidor obtenida desde el WS_TEST para mostrarlos en el  select del formulario
                WS_GET_TYPE.WS_TEST ws_get_type = new WS_GET_TYPE.WS_TEST();
                WS_GET_TYPE.Parametro[] lista;
                lista = ws_get_type.GetTipoParticipante();

                return Json(lista.Select(x => new
                {
                    codigo = x.codigo,
                    descripcion = x.descripcion
                }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                rg.eLog("Error al obtener los campos del select del type desde el WS: " + ex.ToString());
                WS_GET_TYPE.Parametro[] lista;
                lista = null;
                return Json(lista.Select(x => new
                {
                    codigo = "",
                    descripcion = ""
                }).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        
        //Subir el archivo Xlsx al servidor
        //public JsonResult UploadFilesAjax()
        //{
        //    try
        //    {

        //        int enviado = 0;
        //        //enviado = emailMALO();
        //        //enviado = email();
        //        //enviado = email2();
        //        sinErrores = false;
        //        bool esValido = false;
        //        idTransSinErrores = null;

        //        //Eliminar el archivo y limpiar el archivo si ya existe
        //        if (archivo.name != null && archivo.ruta != null)
        //        {
        //            //Elimino archivo del servidor
        //            if (System.IO.File.Exists(archivo.ruta + archivo.name))
        //            {
        //                System.IO.File.Delete(archivo.ruta + archivo.name);
        //            }
        //        }
        //        archivo.name = null;
        //        archivo.ruta = null;
        //        archivo.nameOriginal = null;
        //        archivo.extension = null;

        //        //Subir archivo Xlsx
        //        //agregamos una ruta para la creación guardar los archivos
        //        string Directorio = Properties.Settings.Default.rutaXls;
        //        string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

        //        // Valida y crea la carpeta definida en el config
        //        if (!(Directory.Exists(ruta)))
        //        {
        //            Directory.CreateDirectory(ruta);
        //        }
        //        string path = ruta;
        //        //Recibir archivo y guardalo con  un nombre unico (ej:1709201991611_test.xlsx)        

        //        var file = Request.Files[0]; //Get file
        //        string nombre_archivo = DateTime.Now.ToString().Replace("/", "") + "_" + file.FileName;
        //        string nombreArchivoExtension = file.FileName;
        //        string[] extensionArchivo = nombreArchivoExtension.Split('.');
        //        string extension = extensionArchivo[1];
        //        nombre_archivo = nombre_archivo.Replace(":", "");
        //        nombre_archivo = nombre_archivo.Replace(" ", "");
        //        file.SaveAs(path + nombre_archivo);
        //        archivo.name = nombre_archivo;
        //        archivo.ruta = path;
        //        archivo.extension = extension;

        //        //verificar si el archivo xls trae errores
        //        archivo.errores = "0";
                
        //        Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
        //        sw.Start(); // Iniciar la medición.
        //        if (archivo.extension == "XLS" || archivo.extension == "xls")
        //        {
        //            esValido = ValidarExcel2003();
        //        }
        //        else {
        //            esValido = ValidarExcel();
        //        }
        //        sw.Stop(); // Detener la medición.
        //        Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

        //        //errores de campos vacios o tipo de datos
        //        if (esValido == false)
        //        {
        //            archivo.errores = "1";

        //        }

        //        //errores de fechas y coutas
        //        if (esValido == true)
        //        {
        //            archivo.errores = "0";
        //            sinErrores = true;
        //        }
                

        //    }
        //    catch (Exception ex)
        //    {

        //        rg.eLog("Error al subir el archivo: " + ex.ToString());
        //        return Json(archivo, JsonRequestBehavior.AllowGet);
        //        //ws_carga_info_adic.VaciarTablas();
        //    }

        //    return Json(archivo, JsonRequestBehavior.AllowGet);

        //}

        //public JsonResult UploadFilesAjaxCONVALIDACIONES()
            public JsonResult UploadFilesAjax()
        {
            try
            {
               // String[] A=null;
                String De;
                String Asunto;
                String Cuerpo;
                String[] files=null;
                String Servidor;
                 //A = "bambasten9@gmail.com";
                //string[] A = { "bambasten9@gmail.com","isobarzo@dim.cl" };
                string[] A = { "isobarzo@dim.cl" };
                //string[] A = { "soporte.Confirming@dimension.cl" };
                //De = "soporte.Confirming@dimension.cl";
                De = "isobarzo@dim.cl";
                Asunto = "test";
                Cuerpo = "dasdfdsfdfa";
                //files[0] = null;
                bool envio = false;
            
                // De="soporte.Confirming@dimension.cl";

                Servidor = "192.168.0.37";
                //Servidor = "smtp.gmail.com";
               // envio=EnviarMail(A,De,Asunto,Cuerpo,files,Servidor);
                int enviado = 0;
                int id_Trans = 0;
                int erroresBD = 0;
                archivo.errores = "0";
                //enviado = emailMALO();
                //enviado = email();
                //enviado = email2();
                sinErrores = false;
                bool esValido = false;
                idTransSinErrores = null;
                DataSet ds_erroresBD = null;
                // Send();
                // shared();
                //Eliminar el archivo y limpiar el archivo si ya existe

                Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                sw.Start(); // Iniciar la medición.
                if (archivo.name != null && archivo.ruta != null)
                {
                    //Elimino archivo del servidor
                    if (System.IO.File.Exists(archivo.ruta + archivo.name))
                    {
                        System.IO.File.Delete(archivo.ruta + archivo.name);
                    }
                }
                archivo.name = null;
                archivo.ruta = null;
                archivo.nameOriginal = null;
                archivo.extension = null;

                //Subir archivo Xlsx
                //agregamos una ruta para la creación guardar los archivos
                string Directorio = Properties.Settings.Default.rutaXlsOrigen;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio;// + "/";
                string DirectorioServidor = Properties.Settings.Default.rutaServidor;
                //string rutaServidor = System.AppDomain.CurrentDomain.BaseDirectory + DirectorioServidor + "/";
                string rutaServidor = DirectorioServidor;
                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }
                string path = ruta;
                //Recibir archivo y guardalo con  un nombre unico (ej:1709201991611_test.xlsx)        

                var file = Request.Files[0]; //Get file
                string nombre_archivo = DateTime.Now.ToString().Replace("/", "") + "_" + file.FileName;
                string nombreArchivoExtension = file.FileName;
                string[] extensionArchivo = nombreArchivoExtension.Split('.');
                string extension = extensionArchivo[1];
                nombre_archivo = nombre_archivo.Replace(":", "");
                nombre_archivo = nombre_archivo.Replace(" ", "");
                file.SaveAs(path + nombre_archivo);
                archivo.name = nombre_archivo;
                archivo.ruta = path;
                archivo.rutaCompleta = archivo.ruta + archivo.name;
                archivo.extension = extension;
                string rutaOrigen = "";
                rutaOrigen = rutaOrigen + archivo.ruta + archivo.name;
                string rutaDestino = "";

                rutaDestino = rutaDestino + rutaServidor;
                rutaDestino = rutaDestino + archivo.name;
                archivo.rutaServidor = rutaServidor;
                archivo.rutaCompletaServidor = rutaDestino;
                // rutaDestino = rutaDestino + rutaDestinoAux;

                System.IO.File.Copy(rutaOrigen, rutaDestino, true);

                string hojaFinal;
                List<string> hoja;
                hoja = obtenerHoja(archivo.rutaCompleta);
                hojaFinal = hoja[0];
                archivo.hoja = hojaFinal;
                WS_GET_TYPE.WS_TEST ws_test = new WS_GET_TYPE.WS_TEST();
                ws_test.Timeout = -1;
                //ws_test.GuardarContenidoExcel(archivo.name,archivo.hoja,archivo.rutaServidor,archivo.rutaCompletaServidor,archivo.extension);
                id_Trans = ws_test.GuardarContenidoExcel(archivo.name, archivo.hoja, archivo.rutaServidor, archivo.rutaCompletaServidor, archivo.extension);
                sw.Stop(); // Detener la medición.
                Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

               // Stopwatch sw2 = new Stopwatch(); // Creación del Stopwatch.
                //sw2.Start(); // Iniciar la medición.
                if (id_Trans != 0)
                {
                    ds_erroresBD = ws_test.ValidarExcelBD(id_Trans);
                }

                //sw2.Stop(); // Detener la medición.
                //Console.WriteLine("Time elapsed: {0}", sw2.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                if (ds_erroresBD != null && ds_erroresBD.Tables[0].Rows.Count > 0)
                {
                    GenerarArchivoErroresBD(ds_erroresBD);
                    archivo.errores = "1";
                }
                //verificar si el archivo xls trae errores
                /* archivo.errores = "0";

                 Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                 sw.Start(); // Iniciar la medición.
                 if (archivo.extension == "XLS" || archivo.extension == "xls")
                 {
                     esValido = ValidarExcel2003();
                 }
                 else
                 {
                     esValido = ValidarExcel();
                 }*/
                sw.Stop(); // Detener la medición.
                Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                //errores de campos vacios o tipo de datos
                //if (esValido == false)
                //{
                //    archivo.errores = "1";

                //}

                ////errores de fechas y coutas
                //if (esValido == true)
                //{
                //    archivo.errores = "0";
                //    sinErrores = true;
                //}


            }
            catch (Exception ex)
            {

                rg.eLog("Error al subir el archivo: " + ex.ToString());
                return Json(archivo, JsonRequestBehavior.AllowGet);
                //ws_carga_info_adic.VaciarTablas();
            }

            return Json(archivo, JsonRequestBehavior.AllowGet);

        }


        /*public JsonResult UploadFilesAjax()
        {
            try
            {

                archivo.errores = "1";
                Download();
            
               
           


            }
            catch (Exception ex)
            {

                rg.eLog("Error al subir el archivo: " + ex.ToString());
                return Json(archivo, JsonRequestBehavior.AllowGet);
                //ws_carga_info_adic.VaciarTablas();
            }

            return Json(archivo, JsonRequestBehavior.AllowGet);

        }*/

        /*
        public static int emailMALO()
        {
            try
            {
                string EmailOrigen = "isobarzo@dim.cl";
                string EmailDestino = "isobarzo@dim.cl";
                string Contraseña = "isob*1146/";

                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                string path = ruta;//Server.MapPath("~/UploadedFiles/");
                                   //string filePath = Server.MapPath("~/UploadedFiles/Errores.csv");
                string filePath = path + "Errores.csv";


                //  string path2 = @"C:\turuta\a.jpg";

                MailMessage oMailMessage = new MailMessage(EmailOrigen, EmailDestino, "este es un asunto", "<b>soy texto negro</b>");
                oMailMessage.Attachments.Add(new Attachment(filePath));
                // oMailMessage.Attachments.Add(new Attachment(path2));

                oMailMessage.IsBodyHtml = true;

                SmtpClient oSmtpCliente = new SmtpClient("192.168.0.37");
                oSmtpCliente.EnableSsl = true;
                oSmtpCliente.UseDefaultCredentials = false;
                oSmtpCliente.Port = 995;
                oSmtpCliente.Credentials = new System.Net.NetworkCredential(EmailOrigen, Contraseña);
                                
                //oSmtpCliente.Credentials = NetworkCred;

                oSmtpCliente.Send(oMailMessage);

                oSmtpCliente.Dispose();
                return 1;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                //rg.eLog("Error al enviar email: " + ex.ToString());
                return 0;
            }
        }

        public static int email()
        {
            try
            {

                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                string path = ruta;//Server.MapPath("~/UploadedFiles/");
                                   //string filePath = Server.MapPath("~/UploadedFiles/Errores.csv");
                string filename = path + "Errores.csv";

                Attachment data = new Attachment(filename, MediaTypeNames.Application.Octet);

                SmtpClient client = new SmtpClient();
                client.Port = 995;
                // utilizamos el servidor SMTP de gmail
                client.Host = "192.168.0.37";
                //client.Host = "192.168.0.37";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                // nos autenticamos con nuestra cuenta de gmail
                client.Credentials = new NetworkCredential("isobarzo@dim.cl", "isob*1146/");

                MailMessage mail = new MailMessage("bambasten9@gmail.com", "isobarzo@dim.cl", "test 2", "test email usando C#");
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mail.Attachments.Add(data);
                client.Send(mail);
                return 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }


        public static int email2() { 
        MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("isobarzo@dim.cl"));
        msg.Body = "Hola";
        msg.From = new MailAddress("bambasten9@gmail.com");

        SmtpClient smtp = new SmtpClient();
        smtp.Host = "192.168.0.37";
 smtp.Port = 995;
 smtp.Credentials = new NetworkCredential("isobarzodim@.cl", "isob*1146/");

        smtp.Send(msg);
            return 1;
            }
//Eliminar archivo de la lista y el servidor */
        public JsonResult Delete(int index)
        {
            try
            {
                //Eliminar el archivo de la lista y devolver la nueva lista 
                if (archivo.name != null && archivo.ruta != null)
                {
                    //Elimino archivos del servidor
                    if (System.IO.File.Exists(archivo.ruta + archivo.name))
                    {
                        System.IO.File.Delete(archivo.ruta + archivo.name);
                    }
                }
                archivo.name = null;
                archivo.ruta = null;
                archivo.nameOriginal = null;
                return Json(archivo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                rg.eLog("Error al eliminar el archivo: " + ex.ToString());
                return Json(archivo, JsonRequestBehavior.AllowGet);

            }
        }

        //Funcion Validar Datos excel
        public bool ValidarExcel()
        {

            DateTime hoy = DateTime.Today;
            string providerRucCabecera = null;
            string seriesCabecera = null;
            string numerationCabecera = null;
            string invoiceTypeCabecera = null;
            string authorizationNumberCabecera = null;
            string expirationDateCabecera = null;
            string departmentCabecera = null;
            string provinceCabecera = null;
            string districtCabecera = null;
            string addressSupplierCabecera = null;
            string acqDepartmentCabecera = null;
            string acqProvinceCabecera = null;
            string acqDistrictCabecera = null;
            string addressAcquirerCabecera = null;
            string typePaymentCabecera = null;
            string numberQuotaCabecera = null;
            string deliverDateAcqCabecera = null;
            string aceptedDateCabecera = null;
            string paymentDateCabecera = null;
            string netAmountCabecera = null;
            string other1Cabecera = null;
            string additionalField1Cabecera = null;
            string netAmountCuotaCabecera = null;
            string paymentDateCuotaCabecera = null;
            bool exito = false;
            int contadorFaltanCamposFacturasExcelTotal = 0;
            string parteEntera;
            string parteDecimal;
            int dimensionreal = 0;

            try
            {
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {

                    Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                    sw.Start(); // Iniciar la medición.
                    byte[] fileBytes = new byte[file.Length];

                    //abrir el archivo xls y crear el archivo errores
                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));
                    string Directorio = Properties.Settings.Default.rutaErrores;
                    string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                    // Valida y crea la carpeta definida en el config
                    if (!(Directory.Exists(ruta)))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    string path = ruta;
                    string filePath = path + "/Errores.csv";
                    //creo el archivo Errores.csv
                    using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))

                    {
                        //cabecera del archivo errores
                        var csvWriter = new CsvWriter(textWriter);
                        csvWriter.Configuration.Delimiter = ";";
                        csvWriter.WriteField("Fila del Error");
                        csvWriter.WriteField("Observación");
                        csvWriter.NextRecord();
                        using (var package = new ExcelPackage(file))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            //recorres fila por fila del xls

                            dimensionreal = workSheet.Dimension.End.Row;

                            if (workSheet.Dimension.End.Row > 25000)
                            {
                                for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                                {
                                    dimensionreal = rowIterator;
                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        dimensionreal = rowIterator - 1;
                                        break;
                                    }

                                }
                            }

                            if (dimensionreal > 26000)
                            {

                                textWriter.Close();
                                GenerarArchivoErroresSobreCapacidad();
                                contadorFaltanCamposFacturasExcelTotal = 1;
                                return false;
                            }


                            //for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                            for (int rowIterator = 1; rowIterator <= dimensionreal; rowIterator++)
                            {

                                //si viene sin cabecera o sin datos crear nuevo csv
                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (rowIterator == 1 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura
                                if (rowIterator == 2 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                if (rowIterator == 1)
                                {
                                    //guardo los nombres de las cabeceras de los campos
                                    invoiceTypeCabecera = workSheet.Cells[rowIterator, 1].Value.ToString();
                                    providerRucCabecera = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    seriesCabecera = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    numerationCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    expirationDateCabecera = workSheet.Cells[rowIterator, 5].Value.ToString();
                                    departmentCabecera = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    provinceCabecera = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    districtCabecera = workSheet.Cells[rowIterator, 8].Value.ToString();
                                    addressSupplierCabecera = workSheet.Cells[rowIterator, 9].Value.ToString();
                                    acqDepartmentCabecera = workSheet.Cells[rowIterator, 10].Value.ToString();
                                    acqProvinceCabecera = workSheet.Cells[rowIterator, 11].Value.ToString();
                                    acqDistrictCabecera = workSheet.Cells[rowIterator, 12].Value.ToString();
                                    addressAcquirerCabecera = workSheet.Cells[rowIterator, 13].Value.ToString();
                                    typePaymentCabecera = workSheet.Cells[rowIterator, 14].Value.ToString();
                                    numberQuotaCabecera = workSheet.Cells[rowIterator, 15].Value.ToString();
                                    deliverDateAcqCabecera = workSheet.Cells[rowIterator, 16].Value.ToString();
                                    aceptedDateCabecera = workSheet.Cells[rowIterator, 17].Value.ToString();
                                    paymentDateCabecera = workSheet.Cells[rowIterator, 18].Value.ToString();
                                    netAmountCabecera = workSheet.Cells[rowIterator, 19].Value.ToString();
                                    other1Cabecera = workSheet.Cells[rowIterator, 20].Value.ToString();
                                    additionalField1Cabecera = workSheet.Cells[rowIterator, 21].Value.ToString();
                                    //authorizationNumberCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    //cuotas
                                    netAmountCuotaCabecera = workSheet.Cells[rowIterator, 22].Value.ToString();
                                    paymentDateCuotaCabecera = workSheet.Cells[rowIterator, 23].Value.ToString();

                                }
                                if (rowIterator > 1)
                                {
                                    int contadorFaltanCamposFacturasExcel = 0;
                                    string observacion1 = null;

                                    //**VALIDAR CAMPOS VACIOS, SOLO OBLIFATORTIOS**
                                    //provider,series,numeration,invoiceType
                                    if (workSheet.Cells[rowIterator, 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + invoiceTypeCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 2].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + providerRucCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 3].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + seriesCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 4].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numerationCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + typePaymentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //campos no obligatorios, expirationdate, department,province,district,adreessSupplier,acqDepartment,acqProvince,acqDistrict,addressAcquierer,other1, additionalField1
                                    /*if (workSheet.Cells[rowIterator, 5].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + expirationDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 6].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + departmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + provinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + districtCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressSupplierCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDepartmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqProvinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDistrictCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressAcquirerCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 16].Value == null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + deliverDateAcqCabecera ; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + aceptedDateCabecera; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + other1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + additionalField1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    */
                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 18].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + paymentDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 19].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + netAmountCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numberQuotaCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) >= 1 && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) <= 120)
                                    {
                                        //coutas definidas por el numerQuota 
                                        long numberQuota = long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString());
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value == null || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo MONTO_NETO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value == null || workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo FECHA_PAGO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }


                                    //**VALIDAR TIPO DE DATOS ERROREOS**, TODOS LOS QUE NO SON STRING
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 5].Value != null && esDateTime(workSheet.Cells[rowIterator, 5].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + expirationDateCabecera + " :" + workSheet.Cells[rowIterator, 5].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //if ((workSheet.Cells[rowIterator, 1].Value != null && esNumerico(workSheet.Cells[rowIterator, 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " no es de tipo Numerico"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 18].Value != null && esDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + " :" + workSheet.Cells[rowIterator, 18].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //validar  si es decimal
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 27)
                                            {
                                                if (esDecimal(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 22)
                                            {

                                                if (esNumerico(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() != "")
                                            {
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 27)
                                                    {

                                                        if (esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {
                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 22)
                                                    {
                                                        if (esNumerico(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {

                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                            }

                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == false) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }

                                    //expirationDate,deliveryDateAcq y aceptedDate
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value != null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 17].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + aceptedDateCabecera + " :" + workSheet.Cells[rowIterator, 17].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //**VALIDAR LARGO DEFINIDO EN 
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && workSheet.Cells[rowIterator, 2].Value.ToString().Length != 11)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " debe ser de 11 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 1].Value != null && workSheet.Cells[rowIterator, 1].Value.ToString().Length > 2)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " :" + workSheet.Cells[rowIterator, 1].Value.ToString() + "  debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 3].Value != null && workSheet.Cells[rowIterator, 3].Value.ToString().Length > 4)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + seriesCabecera + " :" + workSheet.Cells[rowIterator, 3].Value.ToString() + "  debe tener un maximo de 4 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) != false) && workSheet.Cells[rowIterator, 4].Value.ToString().Length > 8) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  debe tener un maximo de 8 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) != false) && workSheet.Cells[rowIterator, 14].Value.ToString().Length > 1) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " debe ser de 1 digito"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    if (workSheet.Cells[rowIterator, 6].Value != null && workSheet.Cells[rowIterator, 6].Value.ToString() != "" && workSheet.Cells[rowIterator, 6].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + departmentCabecera + " :" + workSheet.Cells[rowIterator, 6].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value != null && workSheet.Cells[rowIterator, 7].Value.ToString() != "" && workSheet.Cells[rowIterator, 7].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + provinceCabecera + " :" + workSheet.Cells[rowIterator, 7].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 8].Value != null && workSheet.Cells[rowIterator, 8].Value.ToString() != "" && workSheet.Cells[rowIterator, 8].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + districtCabecera + " :" + workSheet.Cells[rowIterator, 8].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value != null && workSheet.Cells[rowIterator, 9].Value.ToString() != "" && workSheet.Cells[rowIterator, 9].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressSupplierCabecera + " :" + workSheet.Cells[rowIterator, 9].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value != null && workSheet.Cells[rowIterator, 10].Value.ToString() != "" && workSheet.Cells[rowIterator, 10].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDepartmentCabecera + " :" + workSheet.Cells[rowIterator, 10].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value != null && workSheet.Cells[rowIterator, 11].Value.ToString() != "" && workSheet.Cells[rowIterator, 11].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqProvinceCabecera + " :" + workSheet.Cells[rowIterator, 11].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value != null && workSheet.Cells[rowIterator, 12].Value.ToString() != "" && workSheet.Cells[rowIterator, 12].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDistrictCabecera + " :" + workSheet.Cells[rowIterator, 12].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value != null && workSheet.Cells[rowIterator, 13].Value.ToString() != "" && workSheet.Cells[rowIterator, 13].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressAcquirerCabecera + " :" + workSheet.Cells[rowIterator, 13].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value != null && workSheet.Cells[rowIterator, 20].Value.ToString() != "" && workSheet.Cells[rowIterator, 20].Value.ToString().Length > 255) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + other1Cabecera + " :" + workSheet.Cells[rowIterator, 20].Value.ToString() + " debe tener un maximo de 255 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value != null && workSheet.Cells[rowIterator, 21].Value.ToString() != "" && workSheet.Cells[rowIterator, 21].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + additionalField1Cabecera + " :" + workSheet.Cells[rowIterator, 21].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        parteEntera = null;
                                        parteDecimal = null;
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                        {

                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length > 22)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split('.');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];
                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split(',');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];

                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false) && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) > 120) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " debe ser menor o gual a 100"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            //if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false &&  workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22 ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 27)
                                            {
                                                parteEntera = null;
                                                parteDecimal = null;
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                                {

                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true)
                                                {
                                                    string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().ToString().Split('.');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];
                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    string[] monto = hoy.ToString().Split(',');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];

                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                            }

                                        }
                                    }


                                    //**VALIDAR FECHAS**
                                    //validar si deliveryDateAcq  es menor o igual a la fecha actual
                                    string[] hoyseparado = hoy.ToString().Split(' ');
                                    string hoy2 = hoyseparado[0];
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " es mayor a la fecha actual: " + hoy2; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    //typePayment es 0, se verifica que paymantDate de la fila 18, sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 18].Value != null
                                       && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && workSheet.Cells[rowIterator, 18].Value.ToString() != "" && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy
                                       && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) >= Convert.ToDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()))
                                    {
                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + ": " + workSheet.Cells[rowIterator, 18].Value.ToString() + " debe ser mayor a: " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                    }

                                    //typePayment es 1, se verifica que paymantDate de la fila (23+2n) sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                        && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120 && (workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != ""
                                        && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) <= hoy))
                                    {
                                        {
                                            //coutas definidas por el numerQuota 
                                            int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                            for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                            {
                                                int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                                if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) <= Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()))
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " debe ser mayor a " + deliverDateAcqCabecera + " : " + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }

                                    //typePayment es 1, se verifica que las FECHA_PAGO_CUOTA sean dsitintas 
                                    /* if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                         && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                     {
                                         {
                                             //coutas definidas por el numerQuota 
                                             int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                             for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                             {
                                                 for (int contadorCuotas2 = 2; contadorCuotas2 <= numberQuota; contadorCuotas2++)
                                                 {
                                                     if (contadorCuotas != contadorCuotas2)
                                                     {
                                                         int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                                         int filapaymentCoutaCuota2 = 23 + (2 * (contadorCuotas2 - 1));

                                                         if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()))
                                                         {
                                                             observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas2 + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() + " no puede ser igual a  FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                         }
                                                     }
                                                 }
                                             }
                                         }
                                     }*/
                                    //typePayment es 1, se verifica que las FECHA_PAGO_CUOTA_(n) sea menor a FECHA_PAGO_CUOTA(n+1)
                                    /*if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                        && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120  )
                                    {
                                        {
                                            //coutas definidas por el numerQuota 
                                            int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                            for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                            {
                                                for (int contadorCuotas2 = contadorCuotas+1; contadorCuotas2 <= numberQuota; contadorCuotas2++)
                                                {
                                                    if (contadorCuotas != contadorCuotas2)
                                                    {
                                                        int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                                        int filapaymentCoutaCuota2 = 23 + (2 * (contadorCuotas2 - 1));

                                                        if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) > Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()))
                                                            if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) <  Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) )
                                                            //    if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && DateTime.Compare(Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()), Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString())) < 0)
                                                                {
                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas2 + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() + " debe ser mayor a FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }      }
                                        }
                                    }*/

                                    List<ListaCuotasValidacion> listaExcel = new List<ListaCuotasValidacion>();
                                    List<ListaCuotasValidacion> listaExcelSiguiente = new List<ListaCuotasValidacion>();

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) >= 1 && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) <= 120)
                                    {
                                        //coutas definidas por el numerQuota 
                                        long numberQuota = long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString());
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                            int contadorCuotasSiguiente = contadorCuotas + 1;
                                            int filaNetAmountCuotaSiguiente = 22 + (2 * ((contadorCuotasSiguiente) - 1));

                                            int filapaymentCoutaSiguiente = 23 + (2 * ((contadorCuotasSiguiente) - 1));


                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() != ""
                                                && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == true
                                                && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == true)
                                            {

                                                if (contadorCuotas < numberQuota - 1 && contadorCuotasSiguiente <= numberQuota)
                                                {
                                                    if (contadorCuotas != contadorCuotasSiguiente)
                                                    {
                                                        ListaCuotasValidacion elemento = new ListaCuotasValidacion();
                                                        elemento.id = rowIterator;
                                                        elemento.numeroCuota = contadorCuotas;
                                                        elemento.monto = Convert.ToDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                                        elemento.fecha = Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString());
                                                        listaExcel.Add(elemento);

                                                        ListaCuotasValidacion elementoSiguiente = new ListaCuotasValidacion();
                                                        elementoSiguiente.id = rowIterator;
                                                        elementoSiguiente.numeroCuota = contadorCuotasSiguiente;
                                                        elementoSiguiente.monto = Convert.ToDecimal(workSheet.Cells[rowIterator, filaNetAmountCuotaSiguiente].Value.ToString());
                                                        elementoSiguiente.fecha = Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaSiguiente].Value.ToString());
                                                        listaExcelSiguiente.Add(elementoSiguiente);
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    Console.Write(listaExcel);
                                    Console.Write(listaExcelSiguiente);
                                    //listaExcel = listaExcel.OrderBy(p => p.fecha).ToList();
                                    listaExcel = listaExcel.OrderByDescending(p => p.numeroCuota).ToList();
                                    //var sonIguales = listaExcel.SetEquals(listaExcelSiguiente);




                                    //var sonIguales = listaExcel.SetEquals(listaExcelSiguiente);
                                    //listaExcel = listaExcel.OrderBy(p => p.numeroCuota).ToList();
                                    //listaExcel.RemoveAt(119);
                                    //listaExcelSiguiente.RemoveAt(119);
                                    Console.Write(listaExcel);
                                    Console.Write(listaExcelSiguiente);
                                    List<ListaCuotasValidacion> fechasPagosMayores = new List<ListaCuotasValidacion>();
                                    fechasPagosMayores = listaExcel.Except(listaExcelSiguiente).ToList();

                                    //fechasPagosMayores = (from t in listaExcel where listaExcelSiguiente.Any(x => x.fecha < t.fecha && x.numeroCuota != t.numeroCuota) select t).ToList();
                                    Console.Write(fechasPagosMayores);
                                    //fechasPagosMayores = listaExcel.Where(s => s.fecha > s1 =>s1.fecha).Select(s => s, s1 => s1);
                                    // fechasPagosMayores = listaExcel.Where(l1 => list2.Any(l2 => l2.g4 == l1.g2));
                                    // var query = list1.Where(l1 => list2.Any(l2 => l2.g4 == l1.g2));

                                    //**VALIDAR providerRuc**
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && ValidationRUC(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " : " + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es un RUC válido"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        break;
                                    }
                                    if (contadorFaltanCamposFacturasExcel > 0)
                                    {
                                        contadorFaltanCamposFacturasExcelTotal = contadorFaltanCamposFacturasExcelTotal + 1;
                                    }
                                }
                            }

                        }
                        textWriter.Close();
                    }
                    sw.Stop(); // Detener la medición.
                    Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                }



                file.Close();
                file.Dispose();

                if (contadorFaltanCamposFacturasExcelTotal > 0)
                {
                    exito = false;
                }
                else
                {
                    exito = true;
                }
                return exito;

            }
            catch (Exception ex)
            {
                rg.eLog("Error a validar campos vacios y tipo de datos: " + ex.ToString());
                return false;
            }
        }
        public bool ValidarExcelRESPALDO()
        {

            DateTime hoy = DateTime.Today;
            string providerRucCabecera = null;
            string seriesCabecera = null;
            string numerationCabecera = null;
            string invoiceTypeCabecera = null;
            string authorizationNumberCabecera = null;
            string expirationDateCabecera = null;
            string departmentCabecera = null;
            string provinceCabecera = null;
            string districtCabecera = null;
            string addressSupplierCabecera = null;
            string acqDepartmentCabecera = null;
            string acqProvinceCabecera = null;
            string acqDistrictCabecera = null;
            string addressAcquirerCabecera = null;
            string typePaymentCabecera = null;
            string numberQuotaCabecera = null;
            string deliverDateAcqCabecera = null;
            string aceptedDateCabecera = null;
            string paymentDateCabecera = null;
            string netAmountCabecera = null;
            string other1Cabecera = null;
            string additionalField1Cabecera = null;
            string netAmountCuotaCabecera = null;
            string paymentDateCuotaCabecera = null;
            bool exito = false;
            int contadorFaltanCamposFacturasExcelTotal = 0;
            string parteEntera;
            string parteDecimal;
            int dimensionreal = 0;

            try
            {
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {

                    Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                    sw.Start(); // Iniciar la medición.
                    byte[] fileBytes = new byte[file.Length];

                    //abrir el archivo xls y crear el archivo errores
                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));
                    string Directorio = Properties.Settings.Default.rutaErrores;
                    string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                    // Valida y crea la carpeta definida en el config
                    if (!(Directory.Exists(ruta)))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    string path = ruta;
                    string filePath = path + "/Errores.csv";
                    //creo el archivo Errores.csv
                    using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))

                    {
                        //cabecera del archivo errores
                        var csvWriter = new CsvWriter(textWriter);
                        csvWriter.Configuration.Delimiter = ";";
                        csvWriter.WriteField("Fila del Error");
                        csvWriter.WriteField("Observación");
                        csvWriter.NextRecord();
                        using (var package = new ExcelPackage(file))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            //recorres fila por fila del xls

                            dimensionreal = workSheet.Dimension.End.Row;

                            if (workSheet.Dimension.End.Row > 25000)
                            {
                                for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                                {
                                    dimensionreal = rowIterator;
                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        dimensionreal = rowIterator - 1;
                                        break;
                                    }

                                }
                            }

                            if (dimensionreal > 25000)
                            {

                                textWriter.Close();
                                GenerarArchivoErroresSobreCapacidad();
                                contadorFaltanCamposFacturasExcelTotal = 1;
                                return false;
                            }


                            //for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                            for (int rowIterator = 1; rowIterator <= dimensionreal; rowIterator++)
                            {

                                //si viene sin cabecera o sin datos crear nuevo csv
                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (rowIterator == 1 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura
                                if (rowIterator == 2 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                if (rowIterator == 1)
                                {
                                    //guardo los nombres de las cabeceras de los campos
                                    invoiceTypeCabecera = workSheet.Cells[rowIterator, 1].Value.ToString();
                                    providerRucCabecera = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    seriesCabecera = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    numerationCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    expirationDateCabecera = workSheet.Cells[rowIterator, 5].Value.ToString();
                                    departmentCabecera = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    provinceCabecera = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    districtCabecera = workSheet.Cells[rowIterator, 8].Value.ToString();
                                    addressSupplierCabecera = workSheet.Cells[rowIterator, 9].Value.ToString();
                                    acqDepartmentCabecera = workSheet.Cells[rowIterator, 10].Value.ToString();
                                    acqProvinceCabecera = workSheet.Cells[rowIterator, 11].Value.ToString();
                                    acqDistrictCabecera = workSheet.Cells[rowIterator, 12].Value.ToString();
                                    addressAcquirerCabecera = workSheet.Cells[rowIterator, 13].Value.ToString();
                                    typePaymentCabecera = workSheet.Cells[rowIterator, 14].Value.ToString();
                                    numberQuotaCabecera = workSheet.Cells[rowIterator, 15].Value.ToString();
                                    deliverDateAcqCabecera = workSheet.Cells[rowIterator, 16].Value.ToString();
                                    aceptedDateCabecera = workSheet.Cells[rowIterator, 17].Value.ToString();
                                    paymentDateCabecera = workSheet.Cells[rowIterator, 18].Value.ToString();
                                    netAmountCabecera = workSheet.Cells[rowIterator, 19].Value.ToString();
                                    other1Cabecera = workSheet.Cells[rowIterator, 20].Value.ToString();
                                    additionalField1Cabecera = workSheet.Cells[rowIterator, 21].Value.ToString();
                                    //authorizationNumberCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    //cuotas
                                    netAmountCuotaCabecera = workSheet.Cells[rowIterator, 22].Value.ToString();
                                    paymentDateCuotaCabecera = workSheet.Cells[rowIterator, 23].Value.ToString();

                                }
                                if (rowIterator > 1)
                                {
                                    int contadorFaltanCamposFacturasExcel = 0;
                                    string observacion1 = null;

                                    //**VALIDAR CAMPOS VACIOS, SOLO OBLIFATORTIOS**
                                    //provider,series,numeration,invoiceType
                                    if (workSheet.Cells[rowIterator, 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + invoiceTypeCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 2].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + providerRucCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 3].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + seriesCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 4].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numerationCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + typePaymentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //campos no obligatorios, expirationdate, department,province,district,adreessSupplier,acqDepartment,acqProvince,acqDistrict,addressAcquierer,other1, additionalField1
                                    /*if (workSheet.Cells[rowIterator, 5].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + expirationDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 6].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + departmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + provinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + districtCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressSupplierCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDepartmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqProvinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDistrictCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressAcquirerCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 16].Value == null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + deliverDateAcqCabecera ; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + aceptedDateCabecera; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + other1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + additionalField1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    */
                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 18].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + paymentDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 19].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + netAmountCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numberQuotaCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) >= 1 && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) <= 120)
                                    {
                                        //coutas definidas por el numerQuota 
                                        long numberQuota = long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString());
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value == null || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo MONTO_NETO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value == null || workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo FECHA_PAGO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }


                                    //**VALIDAR TIPO DE DATOS ERROREOS**, TODOS LOS QUE NO SON STRING
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 5].Value != null && esDateTime(workSheet.Cells[rowIterator, 5].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + expirationDateCabecera + " :" + workSheet.Cells[rowIterator, 5].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //if ((workSheet.Cells[rowIterator, 1].Value != null && esNumerico(workSheet.Cells[rowIterator, 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " no es de tipo Numerico"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 18].Value != null && esDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + " :" + workSheet.Cells[rowIterator, 18].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //validar  si es decimal
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 27)
                                            {
                                                if (esDecimal(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 22)
                                            {

                                                if (esNumerico(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() != "")
                                            {
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 27)
                                                    {

                                                        if (esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {
                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 22)
                                                    {
                                                        if (esNumerico(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {

                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                            }

                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == false) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }

                                    //expirationDate,deliveryDateAcq y aceptedDate
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value != null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 17].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + aceptedDateCabecera + " :" + workSheet.Cells[rowIterator, 17].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //**VALIDAR LARGO DEFINIDO EN 
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && workSheet.Cells[rowIterator, 2].Value.ToString().Length != 11)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " debe ser de 11 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 1].Value != null && workSheet.Cells[rowIterator, 1].Value.ToString().Length > 2)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " :" + workSheet.Cells[rowIterator, 1].Value.ToString() + "  debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 3].Value != null && workSheet.Cells[rowIterator, 3].Value.ToString().Length > 4)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + seriesCabecera + " :" + workSheet.Cells[rowIterator, 3].Value.ToString() + "  debe tener un maximo de 4 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) != false) && workSheet.Cells[rowIterator, 4].Value.ToString().Length > 8) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  debe tener un maximo de 8 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) != false) && workSheet.Cells[rowIterator, 14].Value.ToString().Length > 1) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " debe ser de 1 digito"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    if (workSheet.Cells[rowIterator, 6].Value != null && workSheet.Cells[rowIterator, 6].Value.ToString() != "" && workSheet.Cells[rowIterator, 6].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + departmentCabecera + " :" + workSheet.Cells[rowIterator, 6].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value != null && workSheet.Cells[rowIterator, 7].Value.ToString() != "" && workSheet.Cells[rowIterator, 7].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + provinceCabecera + " :" + workSheet.Cells[rowIterator, 7].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 8].Value != null && workSheet.Cells[rowIterator, 8].Value.ToString() != "" && workSheet.Cells[rowIterator, 8].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + districtCabecera + " :" + workSheet.Cells[rowIterator, 8].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value != null && workSheet.Cells[rowIterator, 9].Value.ToString() != "" && workSheet.Cells[rowIterator, 9].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressSupplierCabecera + " :" + workSheet.Cells[rowIterator, 9].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value != null && workSheet.Cells[rowIterator, 10].Value.ToString() != "" && workSheet.Cells[rowIterator, 10].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDepartmentCabecera + " :" + workSheet.Cells[rowIterator, 10].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value != null && workSheet.Cells[rowIterator, 11].Value.ToString() != "" && workSheet.Cells[rowIterator, 11].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqProvinceCabecera + " :" + workSheet.Cells[rowIterator, 11].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value != null && workSheet.Cells[rowIterator, 12].Value.ToString() != "" && workSheet.Cells[rowIterator, 12].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDistrictCabecera + " :" + workSheet.Cells[rowIterator, 12].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value != null && workSheet.Cells[rowIterator, 13].Value.ToString() != "" && workSheet.Cells[rowIterator, 13].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressAcquirerCabecera + " :" + workSheet.Cells[rowIterator, 13].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value != null && workSheet.Cells[rowIterator, 20].Value.ToString() != "" && workSheet.Cells[rowIterator, 20].Value.ToString().Length > 255) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + other1Cabecera + " :" + workSheet.Cells[rowIterator, 20].Value.ToString() + " debe tener un maximo de 255 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value != null && workSheet.Cells[rowIterator, 21].Value.ToString() != "" && workSheet.Cells[rowIterator, 21].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + additionalField1Cabecera + " :" + workSheet.Cells[rowIterator, 21].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        parteEntera = null;
                                        parteDecimal = null;
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                        {

                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length > 22)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split('.');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];
                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split(',');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];

                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false) && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) > 120) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " debe ser menor o gual a 100"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            //if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false &&  workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22 ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 27)
                                            {
                                                parteEntera = null;
                                                parteDecimal = null;
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                                {

                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true)
                                                {
                                                    string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().ToString().Split('.');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];
                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    string[] monto = hoy.ToString().Split(',');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];

                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                            }

                                        }
                                    }


                                    //**VALIDAR FECHAS**
                                    //validar si deliveryDateAcq  es menor o igual a la fecha actual
                                    string[] hoyseparado = hoy.ToString().Split(' ');
                                    string hoy2 = hoyseparado[0];
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " es mayor a la fecha actual: " + hoy2; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    //typePayment es 0, se verifica que paymantDate de la fila 18, sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 18].Value != null
                                       && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && workSheet.Cells[rowIterator, 18].Value.ToString() != "" && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy
                                       && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) >= Convert.ToDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()))
                                    {
                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + ": " + workSheet.Cells[rowIterator, 18].Value.ToString() + " debe ser mayor a: " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                    }

                                    //typePayment es 1, se verifica que paymantDate de la fila (23+2n) sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                        && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120 && (workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != ""
                                        && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) <= hoy))
                                    {
                                        {
                                            //coutas definidas por el numerQuota 
                                            int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                            for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                            {
                                                int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                                if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) <= Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()))
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " debe ser mayor a " + deliverDateAcqCabecera + " : " + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }

                                    //typePayment es 1, se verifica que las FECHA_PAGO_CUOTA sean dsitintas 
                                    /* if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                         && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                     {
                                         {
                                             //coutas definidas por el numerQuota 
                                             int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                             for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                             {
                                                 for (int contadorCuotas2 = 2; contadorCuotas2 <= numberQuota; contadorCuotas2++)
                                                 {
                                                     if (contadorCuotas != contadorCuotas2)
                                                     {
                                                         int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                                         int filapaymentCoutaCuota2 = 23 + (2 * (contadorCuotas2 - 1));

                                                         if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()))
                                                         {
                                                             observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas2 + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() + " no puede ser igual a  FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                         }
                                                     }
                                                 }
                                             }
                                         }
                                     }*/
                                    //typePayment es 1, se verifica que las FECHA_PAGO_CUOTA_(n) sea menor a FECHA_PAGO_CUOTA(n+1)
                                    /*if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                        && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120  )
                                    {
                                        {
                                            //coutas definidas por el numerQuota 
                                            int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                            for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                            {
                                                for (int contadorCuotas2 = contadorCuotas+1; contadorCuotas2 <= numberQuota; contadorCuotas2++)
                                                {
                                                    if (contadorCuotas != contadorCuotas2)
                                                    {
                                                        int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                                        int filapaymentCoutaCuota2 = 23 + (2 * (contadorCuotas2 - 1));

                                                        if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) > Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()))
                                                            if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) <  Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) )
                                                            //    if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()) != false && DateTime.Compare(Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString()), Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString())) < 0)
                                                                {
                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas2 + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota2].Value.ToString() + " debe ser mayor a FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }      }
                                        }
                                    }*/

                                    List<ListaCuotasValidacion> listaExcel = new List<ListaCuotasValidacion>();
                                    List<ListaCuotasValidacion> listaExcelSiguiente = new List<ListaCuotasValidacion>();

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) >= 1 && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) <= 120)
                                    {
                                        //coutas definidas por el numerQuota 
                                        long numberQuota = long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString());
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));
                                            int contadorCuotasSiguiente = contadorCuotas + 1;
                                            int filaNetAmountCuotaSiguiente = 22 + (2 * ((contadorCuotasSiguiente) - 1));

                                            int filapaymentCoutaSiguiente = 23 + (2 * ((contadorCuotasSiguiente) - 1));


                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() != ""
                                                && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null || workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "")
                                            {

                                                if (contadorCuotas < numberQuota && contadorCuotasSiguiente <= numberQuota)
                                                {
                                                    if (contadorCuotas != contadorCuotasSiguiente)
                                                    {
                                                        ListaCuotasValidacion elemento = new ListaCuotasValidacion();
                                                        elemento.id = rowIterator;
                                                        elemento.numeroCuota = contadorCuotas;
                                                        elemento.monto = Convert.ToDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                                        elemento.fecha = Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString());
                                                        listaExcel.Add(elemento);

                                                        ListaCuotasValidacion elementoSiguiente = new ListaCuotasValidacion();
                                                        elementoSiguiente.id = rowIterator;
                                                        elementoSiguiente.numeroCuota = contadorCuotasSiguiente;
                                                        elementoSiguiente.monto = Convert.ToDecimal(workSheet.Cells[rowIterator, filaNetAmountCuotaSiguiente].Value.ToString());
                                                        elementoSiguiente.fecha = Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaSiguiente].Value.ToString());
                                                        listaExcelSiguiente.Add(elementoSiguiente);
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    Console.Write(listaExcel);
                                    Console.Write(listaExcelSiguiente);
                                    //listaExcel = listaExcel.OrderBy(p => p.fecha).ToList();
                                    listaExcel = listaExcel.OrderByDescending(p => p.numeroCuota).ToList();
                                    //var sonIguales = listaExcel.SetEquals(listaExcelSiguiente);




                                    //var sonIguales = listaExcel.SetEquals(listaExcelSiguiente);
                                    //listaExcel = listaExcel.OrderBy(p => p.numeroCuota).ToList();
                                    //listaExcel.RemoveAt(119);
                                    //listaExcelSiguiente.RemoveAt(119);
                                    Console.Write(listaExcel);
                                    Console.Write(listaExcelSiguiente);
                                    List<ListaCuotasValidacion> fechasPagosMayores = new List<ListaCuotasValidacion>();
                                    fechasPagosMayores = listaExcel.Except(listaExcelSiguiente).ToList();

                                    //fechasPagosMayores = (from t in listaExcel where listaExcelSiguiente.Any(x => x.fecha < t.fecha && x.numeroCuota != t.numeroCuota) select t).ToList();
                                    Console.Write(fechasPagosMayores);
                                    //fechasPagosMayores = listaExcel.Where(s => s.fecha > s1 =>s1.fecha).Select(s => s, s1 => s1);
                                    // fechasPagosMayores = listaExcel.Where(l1 => list2.Any(l2 => l2.g4 == l1.g2));
                                    // var query = list1.Where(l1 => list2.Any(l2 => l2.g4 == l1.g2));

                                    //**VALIDAR providerRuc**
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && ValidationRUC(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " : " + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es un RUC válido"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        break;
                                    }
                                    if (contadorFaltanCamposFacturasExcel > 0)
                                    {
                                        contadorFaltanCamposFacturasExcelTotal = contadorFaltanCamposFacturasExcelTotal + 1;
                                    }
                                }
                            }

                        }
                        textWriter.Close();
                    }
                    sw.Stop(); // Detener la medición.
                    Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                }



                file.Close();
                file.Dispose();

                if (contadorFaltanCamposFacturasExcelTotal > 0)
                {
                    exito = false;
                }
                else
                {
                    exito = true;
                }
                return exito;

            }
            catch (Exception ex)
            {
                rg.eLog("Error a validar campos vacios y tipo de datos: " + ex.ToString());
                return false;
            }
        }
        //Funcion Validar Datos excel
        public bool ValidarExcelULTIMORESPALDO()
        {

            DateTime hoy = DateTime.Today;
            string providerRucCabecera = null;
            string seriesCabecera = null;
            string numerationCabecera = null;
            string invoiceTypeCabecera = null;
            string authorizationNumberCabecera = null;
            string expirationDateCabecera = null;
            string departmentCabecera = null;
            string provinceCabecera = null;
            string districtCabecera = null;
            string addressSupplierCabecera = null;
            string acqDepartmentCabecera = null;
            string acqProvinceCabecera = null;
            string acqDistrictCabecera = null;
            string addressAcquirerCabecera = null;
            string typePaymentCabecera = null;
            string numberQuotaCabecera = null;
            string deliverDateAcqCabecera = null;
            string aceptedDateCabecera = null;
            string paymentDateCabecera = null;
            string netAmountCabecera = null;
            string other1Cabecera = null;
            string additionalField1Cabecera = null;
            string netAmountCuotaCabecera = null;
            string paymentDateCuotaCabecera = null;
            bool exito = false;
            int contadorFaltanCamposFacturasExcelTotal = 0;
            string parteEntera;
            string parteDecimal;
            int dimensionreal = 0;

            try
            {
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {

                    Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                    sw.Start(); // Iniciar la medición.
                    byte[] fileBytes = new byte[file.Length];

                    //abrir el archivo xls y crear el archivo errores
                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));
                    string Directorio = Properties.Settings.Default.rutaErrores;
                    string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                    // Valida y crea la carpeta definida en el config
                    if (!(Directory.Exists(ruta)))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    string path = ruta;
                    string filePath = path + "/Errores.csv";
                    //creo el archivo Errores.csv
                    using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))

                    {
                        //cabecera del archivo errores
                        var csvWriter = new CsvWriter(textWriter);
                        csvWriter.Configuration.Delimiter = ";";
                        csvWriter.WriteField("Fila del Error");
                        csvWriter.WriteField("Observación");
                        csvWriter.NextRecord();
                        using (var package = new ExcelPackage(file))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            //recorres fila por fila del xls

                            dimensionreal = workSheet.Dimension.End.Row;

                            if (workSheet.Dimension.End.Row > 25000)
                            {
                                for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                                {
                                    dimensionreal = rowIterator;
                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        dimensionreal = rowIterator - 1;
                                        break;
                                    }

                                }
                            }

                            if (dimensionreal > 25000)
                            {

                                textWriter.Close();
                                GenerarArchivoErroresSobreCapacidad();
                                contadorFaltanCamposFacturasExcelTotal = 1;
                                return false;
                            }


                            //for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                            for (int rowIterator = 1; rowIterator <= dimensionreal; rowIterator++)
                            {

                                //si viene sin cabecera o sin datos crear nuevo csv
                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (rowIterator == 1 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura
                                if (rowIterator == 2 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                if (rowIterator == 1)
                                {
                                    //guardo los nombres de las cabeceras de los campos
                                    invoiceTypeCabecera = workSheet.Cells[rowIterator, 1].Value.ToString();
                                    providerRucCabecera = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    seriesCabecera = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    numerationCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    expirationDateCabecera = workSheet.Cells[rowIterator, 5].Value.ToString();
                                    departmentCabecera = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    provinceCabecera = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    districtCabecera = workSheet.Cells[rowIterator, 8].Value.ToString();
                                    addressSupplierCabecera = workSheet.Cells[rowIterator, 9].Value.ToString();
                                    acqDepartmentCabecera = workSheet.Cells[rowIterator, 10].Value.ToString();
                                    acqProvinceCabecera = workSheet.Cells[rowIterator, 11].Value.ToString();
                                    acqDistrictCabecera = workSheet.Cells[rowIterator, 12].Value.ToString();
                                    addressAcquirerCabecera = workSheet.Cells[rowIterator, 13].Value.ToString();
                                    typePaymentCabecera = workSheet.Cells[rowIterator, 14].Value.ToString();
                                    numberQuotaCabecera = workSheet.Cells[rowIterator, 15].Value.ToString();
                                    deliverDateAcqCabecera = workSheet.Cells[rowIterator, 16].Value.ToString();
                                    aceptedDateCabecera = workSheet.Cells[rowIterator, 17].Value.ToString();
                                    paymentDateCabecera = workSheet.Cells[rowIterator, 18].Value.ToString();
                                    netAmountCabecera = workSheet.Cells[rowIterator, 19].Value.ToString();
                                    other1Cabecera = workSheet.Cells[rowIterator, 20].Value.ToString();
                                    additionalField1Cabecera = workSheet.Cells[rowIterator, 21].Value.ToString();
                                    //authorizationNumberCabecera = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    //cuotas
                                    netAmountCuotaCabecera = workSheet.Cells[rowIterator, 22].Value.ToString();
                                    paymentDateCuotaCabecera = workSheet.Cells[rowIterator, 23].Value.ToString();

                                }
                                if (rowIterator > 1)
                                {
                                    int contadorFaltanCamposFacturasExcel = 0;
                                    string observacion1 = null;

                                    //**VALIDAR CAMPOS VACIOS, SOLO OBLIFATORTIOS**
                                    //provider,series,numeration,invoiceType
                                    if (workSheet.Cells[rowIterator, 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + invoiceTypeCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 2].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + providerRucCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 3].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + seriesCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 4].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numerationCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + typePaymentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //campos no obligatorios, expirationdate, department,province,district,adreessSupplier,acqDepartment,acqProvince,acqDistrict,addressAcquierer,other1, additionalField1
                                    /*if (workSheet.Cells[rowIterator, 5].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + expirationDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 6].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + departmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + provinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + districtCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressSupplierCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDepartmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqProvinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDistrictCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressAcquirerCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 16].Value == null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + deliverDateAcqCabecera ; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + aceptedDateCabecera; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + other1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + additionalField1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    */
                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 18].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + paymentDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 19].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + netAmountCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numberQuotaCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) >= 1 && long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString()) <= 120)
                                    {
                                        //coutas definidas por el numerQuota 
                                        long numberQuota = long.Parse(workSheet.Cells[rowIterator, 15].Value.ToString());
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value == null || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo MONTO_NETO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value == null || workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo FECHA_PAGO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }


                                    //**VALIDAR TIPO DE DATOS ERROREOS**, TODOS LOS QUE NO SON STRING
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 5].Value != null && esDateTime(workSheet.Cells[rowIterator, 5].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + expirationDateCabecera + " :" + workSheet.Cells[rowIterator, 5].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //if ((workSheet.Cells[rowIterator, 1].Value != null && esNumerico(workSheet.Cells[rowIterator, 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " no es de tipo Numerico"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 18].Value != null && esDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + " :" + workSheet.Cells[rowIterator, 18].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //validar  si es decimal
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 27)
                                            {
                                                if (esDecimal(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length <= 22)
                                            {

                                                if (esNumerico(workSheet.Cells[rowIterator, 19].Value.ToString()) == false)
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }


                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() != "")
                                            {
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true || workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 27)
                                                    {

                                                        if (esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {
                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length <= 22)
                                                    {
                                                        if (esNumerico(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false)
                                                        {

                                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                        }
                                                    }
                                                }
                                            }

                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) == false) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                        }
                                    }

                                    //expirationDate,deliveryDateAcq y aceptedDate
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 17].Value != null && workSheet.Cells[rowIterator, 17].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 17].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + aceptedDateCabecera + " :" + workSheet.Cells[rowIterator, 17].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                    //**VALIDAR LARGO DEFINIDO EN 
                                    //provider,numeration,expirationDate,typePayment
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && workSheet.Cells[rowIterator, 2].Value.ToString().Length != 11)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + workSheet.Cells[rowIterator, 2].Value.ToString() + " debe ser de 11 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 1].Value != null && workSheet.Cells[rowIterator, 1].Value.ToString().Length > 2)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " :" + workSheet.Cells[rowIterator, 1].Value.ToString() + "  debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 3].Value != null && workSheet.Cells[rowIterator, 3].Value.ToString().Length > 4)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + seriesCabecera + " :" + workSheet.Cells[rowIterator, 3].Value.ToString() + "  debe tener un maximo de 4 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 4].Value != null && esNumerico(workSheet.Cells[rowIterator, 4].Value.ToString()) != false) && workSheet.Cells[rowIterator, 4].Value.ToString().Length > 8) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + workSheet.Cells[rowIterator, 4].Value.ToString() + "  debe tener un maximo de 8 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if ((workSheet.Cells[rowIterator, 14].Value != null && esNumerico(workSheet.Cells[rowIterator, 14].Value.ToString()) != false) && workSheet.Cells[rowIterator, 14].Value.ToString().Length > 1) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + workSheet.Cells[rowIterator, 14].Value.ToString() + " debe ser de 1 digito"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    if (workSheet.Cells[rowIterator, 6].Value != null && workSheet.Cells[rowIterator, 6].Value.ToString() != "" && workSheet.Cells[rowIterator, 6].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + departmentCabecera + " :" + workSheet.Cells[rowIterator, 6].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 7].Value != null && workSheet.Cells[rowIterator, 7].Value.ToString() != "" && workSheet.Cells[rowIterator, 7].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + provinceCabecera + " :" + workSheet.Cells[rowIterator, 7].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 8].Value != null && workSheet.Cells[rowIterator, 8].Value.ToString() != "" && workSheet.Cells[rowIterator, 8].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + districtCabecera + " :" + workSheet.Cells[rowIterator, 8].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 9].Value != null && workSheet.Cells[rowIterator, 9].Value.ToString() != "" && workSheet.Cells[rowIterator, 9].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressSupplierCabecera + " :" + workSheet.Cells[rowIterator, 9].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 10].Value != null && workSheet.Cells[rowIterator, 10].Value.ToString() != "" && workSheet.Cells[rowIterator, 10].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDepartmentCabecera + " :" + workSheet.Cells[rowIterator, 10].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 11].Value != null && workSheet.Cells[rowIterator, 11].Value.ToString() != "" && workSheet.Cells[rowIterator, 11].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqProvinceCabecera + " :" + workSheet.Cells[rowIterator, 11].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 12].Value != null && workSheet.Cells[rowIterator, 12].Value.ToString() != "" && workSheet.Cells[rowIterator, 12].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDistrictCabecera + " :" + workSheet.Cells[rowIterator, 12].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 13].Value != null && workSheet.Cells[rowIterator, 13].Value.ToString() != "" && workSheet.Cells[rowIterator, 13].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressAcquirerCabecera + " :" + workSheet.Cells[rowIterator, 13].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 20].Value != null && workSheet.Cells[rowIterator, 20].Value.ToString() != "" && workSheet.Cells[rowIterator, 20].Value.ToString().Length > 255) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + other1Cabecera + " :" + workSheet.Cells[rowIterator, 20].Value.ToString() + " debe tener un maximo de 255 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    if (workSheet.Cells[rowIterator, 21].Value != null && workSheet.Cells[rowIterator, 21].Value.ToString() != "" && workSheet.Cells[rowIterator, 21].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + additionalField1Cabecera + " :" + workSheet.Cells[rowIterator, 21].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && (workSheet.Cells[rowIterator, 19].Value != null))
                                    {
                                        parteEntera = null;
                                        parteDecimal = null;
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                        {

                                            if (workSheet.Cells[rowIterator, 19].Value.ToString().Length > 22)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(".") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split('.');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];
                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                        if (workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == true)
                                        {
                                            string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().Split(',');
                                            parteEntera = monto[0];
                                            parteDecimal = monto[1];

                                            if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                            {

                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, 19].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                    //typePayment es 1 se verifica el numberQuota
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && (workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false) && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) > 120) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + workSheet.Cells[rowIterator, 15].Value.ToString() + " debe ser menor o gual a 100"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120)
                                    {

                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            //if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && esDecimal(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString()) == false &&  workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22 ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                            if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, filaNetAmountCuota].Value != null && workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 27)
                                            {
                                                parteEntera = null;
                                                parteDecimal = null;
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == false && workSheet.Cells[rowIterator, 19].Value.ToString().Contains(",") == false)
                                                {

                                                    if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Length > 22)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(".") == true)
                                                {
                                                    string[] monto = workSheet.Cells[rowIterator, 19].Value.ToString().ToString().Split('.');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];
                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                                if (workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString().Contains(",") == true)
                                                {
                                                    string[] monto = hoy.ToString().Split(',');
                                                    parteEntera = monto[0];
                                                    parteDecimal = monto[1];

                                                    if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                            }

                                        }
                                    }


                                    //**VALIDAR FECHAS**
                                    //validar si deliveryDateAcq  es menor o igual a la fecha actual
                                    string[] hoyseparado = hoy.ToString().Split(' ');
                                    string hoy2 = hoyseparado[0];
                                    if ((workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString() + " es mayor a la fecha actual: " + hoy2; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                    //typePayment es 0, se verifica que paymantDate de la fila 18, sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "0" && workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 18].Value != null
                                       && workSheet.Cells[rowIterator, 16].Value.ToString() != "" && workSheet.Cells[rowIterator, 18].Value.ToString() != "" && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) > hoy
                                       && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) >= Convert.ToDateTime(workSheet.Cells[rowIterator, 18].Value.ToString()))
                                    {
                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + ": " + workSheet.Cells[rowIterator, 18].Value.ToString() + " debe ser mayor a: " + deliverDateAcqCabecera + " :" + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                    }

                                    //typePayment es 1, se verifica que paymantDate de la fila (23+2n) sea mayor a deliveryDateAcq
                                    if (workSheet.Cells[rowIterator, 14].Value != null && workSheet.Cells[rowIterator, 14].Value.ToString() == "1" && workSheet.Cells[rowIterator, 15].Value != null && esNumerico(workSheet.Cells[rowIterator, 15].Value.ToString()) != false
                                        && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) >= 1 && Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value) <= 120 && (workSheet.Cells[rowIterator, 16].Value != null && workSheet.Cells[rowIterator, 16].Value.ToString() != ""
                                        && esDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()) <= hoy))
                                    {
                                        {
                                            //coutas definidas por el numerQuota 
                                            int numberQuota = Convert.ToInt32(workSheet.Cells[rowIterator, 15].Value);
                                            for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                            {
                                                int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                                if (workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value != null && workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() != "" && esDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) != false && Convert.ToDateTime(workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString()) <= Convert.ToDateTime(workSheet.Cells[rowIterator, 16].Value.ToString()))
                                                {
                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString() + " debe ser mayor a " + deliverDateAcqCabecera + " : " + workSheet.Cells[rowIterator, 16].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }
                                    }

                                    //**VALIDAR providerRuc**
                                    if ((workSheet.Cells[rowIterator, 2].Value != null && esNumerico(workSheet.Cells[rowIterator, 2].Value.ToString()) != false && ValidationRUC(workSheet.Cells[rowIterator, 2].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " : " + workSheet.Cells[rowIterator, 2].Value.ToString() + " no es un RUC válido"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        break;
                                    }
                                    if (contadorFaltanCamposFacturasExcel > 0)
                                    {
                                        contadorFaltanCamposFacturasExcelTotal = contadorFaltanCamposFacturasExcelTotal + 1;
                                    }
                                }
                            }

                        }
                        textWriter.Close();
                    }
                    sw.Stop(); // Detener la medición.
                    Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                }



                file.Close();
                file.Dispose();

                if (contadorFaltanCamposFacturasExcelTotal > 0)
                {
                    exito = false;
                }
                else
                {
                    exito = true;
                }
                return exito;

            }
            catch (Exception ex)
            {
                rg.eLog("Error a validar campos vacios y tipo de datos: " + ex.ToString());
                return false;
            }
        }
        //Funcion Validar Datos excel
        public bool ValidarExcel2003()
        {

            DateTime hoy = DateTime.Today;
            string providerRucCabecera = null;
            string seriesCabecera = null;
            string numerationCabecera = null;
            string invoiceTypeCabecera = null;
            string authorizationNumberCabecera = null;
            string expirationDateCabecera = null;
            string departmentCabecera = null;
            string provinceCabecera = null;
            string districtCabecera = null;
            string addressSupplierCabecera = null;
            string acqDepartmentCabecera = null;
            string acqProvinceCabecera = null;
            string acqDistrictCabecera = null;
            string addressAcquirerCabecera = null;
            string typePaymentCabecera = null;
            string numberQuotaCabecera = null;
            string deliverDateAcqCabecera = null;
            string aceptedDateCabecera = null;
            string paymentDateCabecera = null;
            string netAmountCabecera = null;
            string other1Cabecera = null;
            string additionalField1Cabecera = null;
            string netAmountCuotaCabecera = null;
            string paymentDateCuotaCabecera = null;
            bool exito = false;
            int contadorFaltanCamposFacturasExcelTotal = 0;
            string parteEntera;
            string parteDecimal;
            int dimensionreal = 0;

            try
            {
                if (archivo.name != "" || archivo.ruta != "" && (archivo.extension == "XLS" || archivo.extension == "xls"))
                {
                    //FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);



                    Workbook wb = new Workbook(archivo.ruta + archivo.name);

                    //Get the first worksheet.
                    Worksheet worksheet = wb.Worksheets[0];

                    //Get cells
                    Cells cells = worksheet.Cells;

                    // Get row and column count
                    int rowCount = cells.MaxDataRow;
                    int columnCount = cells.MaxDataColumn;

                    // Current cell value
                    string strCell = "";




                    // if ((file != null))
                    //{
                    //byte[] fileBytes = new byte[file.Length];

                    //abrir el archivo xls y crear el archivo errores
                    //var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));
                    string Directorio = Properties.Settings.Default.rutaErrores;
                    string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                    // Valida y crea la carpeta definida en el config
                    if (!(Directory.Exists(ruta)))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    string path = ruta;
                    string filePath = path + "/Errores.csv";
                    //creo el archivo Errores.csv
                    using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))

                    {
                        //cabecera del archivo errores
                        var csvWriter = new CsvWriter(textWriter);
                        csvWriter.Configuration.Delimiter = ";";
                        csvWriter.WriteField("Fila del Error");
                        csvWriter.WriteField("Observación");
                        csvWriter.NextRecord();

                  


                        dimensionreal = rowCount;

                        if (rowCount > 25000)
                        {
                            for (int rowIterator = 1; rowIterator <= rowCount; rowIterator++)
                            {
                                dimensionreal = rowIterator;
                                //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (cells[rowIterator, 1].Value == null && cells[rowIterator, 2].Value == null && cells[rowIterator, 3].Value == null && cells[rowIterator, 4].Value == null && cells[rowIterator, 5].Value == null && cells[rowIterator, 6].Value == null && cells[rowIterator, 7].Value == null && cells[rowIterator, 8].Value == null
                                        && cells[rowIterator, 9].Value == null && cells[rowIterator, 10].Value == null && cells[rowIterator, 11].Value == null && cells[rowIterator, 12].Value == null && cells[rowIterator, 13].Value == null && cells[rowIterator, 14].Value == null && cells[rowIterator, 15].Value == null && cells[rowIterator, 16].Value == null
                                        && cells[rowIterator, 17].Value == null && cells[rowIterator, 18].Value == null && cells[rowIterator, 19].Value == null && cells[rowIterator, 20].Value == null && cells[rowIterator, 21].Value == null && cells[rowIterator, 22].Value == null && cells[rowIterator, 23].Value == null)
                                {
                                    dimensionreal = rowIterator - 1;
                                    break;
                                }

                            }
                        }

                        if (dimensionreal > 25000)
                        {

                            textWriter.Close();
                            GenerarArchivoErroresSobreCapacidadOffice2003();
                            contadorFaltanCamposFacturasExcelTotal = 1;
                            return false;
                        }
                        //recorres fila por fila del xls
                        for (int rowIterator = 0; rowIterator <= rowCount; rowIterator++) // Numeration starts from 0 to MaxDataRow
                        {
                            //for (int column = 0; column <= columnCount; column++)  // Numeration starts from 0 to MaxDataColumn
                            //{
                            //si viene sin cabecera o sin datos crear nuevo csv
                            //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                            if (rowIterator == 1 && cells[rowIterator, 1 - 1].Value == null && cells[rowIterator, 2 - 1].Value == null && cells[rowIterator, 3 - 1].Value == null && cells[rowIterator, 4 - 1].Value == null && cells[rowIterator, 5 - 1].Value == null && cells[rowIterator, 6 - 1].Value == null && cells[rowIterator, 7 - 1].Value == null && cells[rowIterator, 8 - 1].Value == null
                                && cells[rowIterator, 9 - 1].Value == null && cells[rowIterator, 10 - 1].Value == null && cells[rowIterator, 11 - 1].Value == null && cells[rowIterator, 12 - 1].Value == null && cells[rowIterator, 13 - 1].Value == null && cells[rowIterator, 14 - 1].Value == null && cells[rowIterator, 15 - 1].Value == null && cells[rowIterator, 16 - 1].Value == null
                                && cells[rowIterator, 17 - 1].Value == null && cells[rowIterator, 18 - 1].Value == null && cells[rowIterator, 19 - 1].Value == null && cells[rowIterator, 20 - 1].Value == null && cells[rowIterator, 21 - 1].Value == null && cells[rowIterator, 22 - 1].Value == null && cells[rowIterator, 23 - 1].Value == null)
                            {

                                textWriter.Close();
                                GenerarArchivoErroresVacio(rowIterator);
                                contadorFaltanCamposFacturasExcelTotal = 1;
                                return false;
                            }

                            //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura
                            if (rowIterator == 2 && cells[rowIterator, 1 - 1].Value == null && cells[rowIterator, 2 - 1].Value == null && cells[rowIterator, 3 - 1].Value == null && cells[rowIterator, 4 - 1].Value == null && cells[rowIterator, 5 - 1].Value == null && cells[rowIterator, 6 - 1].Value == null && cells[rowIterator, 7 - 1].Value == null && cells[rowIterator, 8 - 1].Value == null
                                && cells[rowIterator, 9 - 1].Value == null && cells[rowIterator, 10 - 1].Value == null && cells[rowIterator, 11 - 1].Value == null && cells[rowIterator, 12 - 1].Value == null && cells[rowIterator, 13 - 1].Value == null && cells[rowIterator, 14 - 1].Value == null && cells[rowIterator, 15 - 1].Value == null && cells[rowIterator, 16 - 1].Value == null
                                && cells[rowIterator, 17 - 1].Value == null && cells[rowIterator, 18 - 1].Value == null && cells[rowIterator, 19 - 1].Value == null && cells[rowIterator, 20 - 1].Value == null && cells[rowIterator, 21 - 1].Value == null && cells[rowIterator, 22 - 1].Value == null && cells[rowIterator, 23 - 1].Value == null)
                            {

                                textWriter.Close();
                                GenerarArchivoErroresVacio(rowIterator);
                                contadorFaltanCamposFacturasExcelTotal = 1;
                                return false;
                            }

                            if (rowIterator == 0)
                            {
                                //guardo los nombres de las cabeceras de los campos
                                invoiceTypeCabecera = cells[rowIterator, 1 - 1].Value.ToString();
                                providerRucCabecera = cells[rowIterator, 2 - 1].Value.ToString();
                                seriesCabecera = cells[rowIterator, 3 - 1].Value.ToString();
                                numerationCabecera = cells[rowIterator, 4 - 1].Value.ToString();
                                expirationDateCabecera = cells[rowIterator, 5 - 1].Value.ToString();
                                departmentCabecera = cells[rowIterator, 6 - 1].Value.ToString();
                                provinceCabecera = cells[rowIterator, 7 - 1].Value.ToString();
                                districtCabecera = cells[rowIterator, 8 - 1].Value.ToString();
                                addressSupplierCabecera = cells[rowIterator, 9 - 1].Value.ToString();
                                acqDepartmentCabecera = cells[rowIterator, 10 - 1].Value.ToString();
                                acqProvinceCabecera = cells[rowIterator, 11 - 1].Value.ToString();
                                acqDistrictCabecera = cells[rowIterator, 12 - 1].Value.ToString();
                                addressAcquirerCabecera = cells[rowIterator, 13 - 1].Value.ToString();
                                typePaymentCabecera = cells[rowIterator, 14 - 1].Value.ToString();
                                numberQuotaCabecera = cells[rowIterator, 15 - 1].Value.ToString();
                                deliverDateAcqCabecera = cells[rowIterator, 16 - 1].Value.ToString();
                                aceptedDateCabecera = cells[rowIterator, 17 - 1].Value.ToString();
                                paymentDateCabecera = cells[rowIterator, 18 - 1].Value.ToString();
                                netAmountCabecera = cells[rowIterator, 19 - 1].Value.ToString();
                                other1Cabecera = cells[rowIterator, 20 - 1].Value.ToString();
                                additionalField1Cabecera = cells[rowIterator, 21 - 1].Value.ToString();
                                //authorizationNumberCabecera = cells[rowIterator, 4-1 ].Value.ToString();
                                //cuotas
                                netAmountCuotaCabecera = cells[rowIterator, 22 - 1].Value.ToString();
                                paymentDateCuotaCabecera = cells[rowIterator, 23 - 1].Value.ToString();

                            }
                            if (rowIterator > 1)
                            {
                                int contadorFaltanCamposFacturasExcel = 0;
                                string observacion1 = null;

                                //**VALIDAR CAMPOS VACIOS, SOLO OBLIFATORTIOS**
                                //provider,series,numeration,invoiceType
                                if (cells[rowIterator, 1 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + invoiceTypeCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 2 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + providerRucCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 3 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + seriesCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 4 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numerationCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 14 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + typePaymentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //campos no obligatorios, expirationdate, department,province,district,adreessSupplier,acqDepartment,acqProvince,acqDistrict,addressAcquierer,other1, additionalField1
                                /*if (cells[rowIterator, 5-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + expirationDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 6-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + departmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 7-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + provinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 9-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + districtCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 9-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressSupplierCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 10-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDepartmentCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 11-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqProvinceCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 12-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + acqDistrictCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 13-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + addressAcquirerCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 16-1 ].Value == null && cells[rowIterator, 16-1 ].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + deliverDateAcqCabecera ; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 17-1 ].Value == null && cells[rowIterator, 17-1 ].Value.ToString() != "" ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + aceptedDateCabecera; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 20-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + other1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 21-1 ].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + additionalField1Cabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                */
                                //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && cells[rowIterator, 18 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + paymentDateCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && cells[rowIterator, 19 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + netAmountCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                //typePayment es 1 se verifica el numberQuota
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, 15 - 1].Value == null) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo " + numberQuotaCabecera; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante, esto tambien valida que la cantidad de coutas sean igaul al numberQuota
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) != false && long.Parse(cells[rowIterator, 15 - 1].Value.ToString()) >= 1 && long.Parse(cells[rowIterator, 15 - 1].Value.ToString()) <= 100)
                                {
                                    //coutas definidas por el numerQuota 
                                    long numberQuota = long.Parse(cells[rowIterator, 15 - 1].Value.ToString());
                                    for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                    {
                                        int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                        int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                        if (cells[rowIterator, filaNetAmountCuota - 1].Value == null || cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo MONTO_NETO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                        if (cells[rowIterator, filapaymentCoutaCuota - 1].Value == null || cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString() == "") { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "Falta el campo FECHA_PAGO_CUOTA_" + contadorCuotas; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    }
                                }


                                //**VALIDAR TIPO DE DATOS ERROREOS**, TODOS LOS QUE NO SON STRING
                                //provider,numeration,expirationDate,typePayment
                                if ((cells[rowIterator, 2 - 1].Value != null && esNumerico(cells[rowIterator, 2 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + cells[rowIterator, 2 - 1].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 4 - 1].Value != null && esNumerico(cells[rowIterator, 4 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + cells[rowIterator, 4 - 1].Value.ToString() + "  no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 5 - 1].Value != null && esDateTime(cells[rowIterator, 5 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + expirationDateCabecera + " :" + cells[rowIterator, 5 - 1].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 14 - 1].Value != null && esNumerico(cells[rowIterator, 14 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + cells[rowIterator, 14 - 1].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //if ((cells[rowIterator, 1-1 ].Value != null && esNumerico(cells[rowIterator, 1-1 ].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " no es de tipo Numerico"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && (cells[rowIterator, 18 - 1].Value != null && esDateTime(cells[rowIterator, 18 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + " :" + cells[rowIterator, 18 - 1].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //validar  si es decimal
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && (cells[rowIterator, 19 - 1].Value != null))
                                {
                                    if (cells[rowIterator, 19 - 1].Value.ToString().Contains(".") == true || cells[rowIterator, 19 - 1].Value.ToString().Contains(",") == true)
                                    {
                                        if (cells[rowIterator, 19 - 1].Value.ToString().Length <= 27)
                                        {
                                            if (esDecimal(cells[rowIterator, 19 - 1].Value.ToString()) == false)
                                            {
                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, 19 - 1].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (cells[rowIterator, 19 - 1].Value.ToString().Length <= 22)
                                        {

                                            if (esNumerico(cells[rowIterator, 19 - 1].Value.ToString()) == false)
                                            {
                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, 19 - 1].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                }


                                //typePayment es 1 se verifica el numberQuota
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && (cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + cells[rowIterator, 15 - 1].Value.ToString() + " no es de tipo Numérico"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) != false && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) >= 1 && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) <= 100)
                                {

                                    //coutas definidas por el numerQuota 
                                    int numberQuota = Convert.ToInt32(cells[rowIterator, 15 - 1].Value);
                                    for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                    {
                                        int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                        int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                        if (cells[rowIterator, filaNetAmountCuota - 1].Value != null && cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() != "")
                                        {
                                            if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Contains(".") == true || cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Contains(",") == true)
                                            {
                                                if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Length <= 27)
                                                {

                                                    if (esDecimal(cells[rowIterator, filaNetAmountCuota - 1].Value.ToString()) == false)
                                                    {
                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Length <= 22)
                                                {
                                                    if (esNumerico(cells[rowIterator, filaNetAmountCuota - 1].Value.ToString()) == false)
                                                    {

                                                        observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() + " no es de tipo Decimal"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                    }
                                                }
                                            }
                                        }

                                        if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, filapaymentCoutaCuota - 1].Value != null && cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString() != "" && esDateTime(cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString()) == false) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString() + " no es del tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                    }
                                }

                                //expirationDate,deliveryDateAcq y aceptedDate
                                if ((cells[rowIterator, 16 - 1].Value != null && cells[rowIterator, 16 - 1].Value.ToString() != "" && esDateTime(cells[rowIterator, 16 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + cells[rowIterator, 16 - 1].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 17 - 1].Value != null && cells[rowIterator, 17 - 1].Value.ToString() != "" && esDateTime(cells[rowIterator, 17 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + aceptedDateCabecera + " :" + cells[rowIterator, 17 - 1].Value.ToString() + " no es de tipo Fecha"; csvWriter.WriteField("El registro que se encuentra en la linea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }


                                //**VALIDAR LARGO DEFINIDO EN 
                                //provider,numeration,expirationDate,typePayment
                                if ((cells[rowIterator, 2 - 1].Value != null && esNumerico(cells[rowIterator, 2 - 1].Value.ToString()) != false && cells[rowIterator, 2 - 1].Value.ToString().Length != 11)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " :" + cells[rowIterator, 2 - 1].Value.ToString() + " debe ser de 11 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 1 - 1].Value != null && cells[rowIterator, 1 - 1].Value.ToString().Length > 2)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + invoiceTypeCabecera + " :" + cells[rowIterator, 1 - 1].Value.ToString() + "  debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 3 - 1].Value != null && cells[rowIterator, 3 - 1].Value.ToString().Length > 4)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + seriesCabecera + " :" + cells[rowIterator, 3 - 1].Value.ToString() + "  debe tener un maximo de 4 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 4 - 1].Value != null && esNumerico(cells[rowIterator, 4 - 1].Value.ToString()) != false) && cells[rowIterator, 4 - 1].Value.ToString().Length > 8) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numerationCabecera + " :" + cells[rowIterator, 4 - 1].Value.ToString() + "  debe tener un maximo de 8 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if ((cells[rowIterator, 14 - 1].Value != null && esNumerico(cells[rowIterator, 14 - 1].Value.ToString()) != false) && cells[rowIterator, 14 - 1].Value.ToString().Length > 1) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + typePaymentCabecera + " :" + cells[rowIterator, 14 - 1].Value.ToString() + " debe ser de 1 digito"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                if (cells[rowIterator, 6 - 1].Value != null && cells[rowIterator, 6 - 1].Value.ToString() != "" && cells[rowIterator, 6 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + departmentCabecera + " :" + cells[rowIterator, 6 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 7 - 1].Value != null && cells[rowIterator, 7 - 1].Value.ToString() != "" && cells[rowIterator, 7 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + provinceCabecera + " :" + cells[rowIterator, 7 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 8 - 1].Value != null && cells[rowIterator, 8 - 1].Value.ToString() != "" && cells[rowIterator, 8 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + districtCabecera + " :" + cells[rowIterator, 8 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 9 - 1].Value != null && cells[rowIterator, 9 - 1].Value.ToString() != "" && cells[rowIterator, 9 - 1].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressSupplierCabecera + " :" + cells[rowIterator, 9 - 1].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 10 - 1].Value != null && cells[rowIterator, 10 - 1].Value.ToString() != "" && cells[rowIterator, 10 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDepartmentCabecera + " :" + cells[rowIterator, 10 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 11 - 1].Value != null && cells[rowIterator, 11 - 1].Value.ToString() != "" && cells[rowIterator, 11 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqProvinceCabecera + " :" + cells[rowIterator, 11 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 12 - 1].Value != null && cells[rowIterator, 12 - 1].Value.ToString() != "" && cells[rowIterator, 12 - 1].Value.ToString().Length > 2) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + acqDistrictCabecera + " :" + cells[rowIterator, 12 - 1].Value.ToString() + " debe tener un maximo de 2 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 13 - 1].Value != null && cells[rowIterator, 13 - 1].Value.ToString() != "" && cells[rowIterator, 13 - 1].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + addressAcquirerCabecera + " :" + cells[rowIterator, 13 - 1].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 20 - 1].Value != null && cells[rowIterator, 20 - 1].Value.ToString() != "" && cells[rowIterator, 20 - 1].Value.ToString().Length > 255) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + other1Cabecera + " :" + cells[rowIterator, 20 - 1].Value.ToString() + " debe tener un maximo de 255 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                if (cells[rowIterator, 21 - 1].Value != null && cells[rowIterator, 21 - 1].Value.ToString() != "" && cells[rowIterator, 21 - 1].Value.ToString().Length > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + additionalField1Cabecera + " :" + cells[rowIterator, 21 - 1].Value.ToString() + " debe tener un maximo de 100 caracteres"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //typePayment es 0, se verifica el netAmount y paymantDate de la fila 18 y 19 y el numerQouta y coutas se omiten
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && (cells[rowIterator, 19 - 1].Value != null))
                                {
                                    parteEntera = null;
                                    parteDecimal = null;
                                    if (cells[rowIterator, 19 - 1].Value.ToString().Contains(".") == false && cells[rowIterator, 19 - 1].Value.ToString().Contains(",") == false)
                                    {

                                        if (cells[rowIterator, 19 - 1].Value.ToString().Length > 22)
                                        {

                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, 19 - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                        }
                                    }
                                    if (cells[rowIterator, 19 - 1].Value.ToString().Contains(".") == true)
                                    {
                                        string[] monto = cells[rowIterator, 19 - 1].Value.ToString().Split('.');
                                        parteEntera = monto[0];
                                        parteDecimal = monto[1];
                                        if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                        {

                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, 19 - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                        }
                                    }
                                    if (cells[rowIterator, 19 - 1].Value.ToString().Contains(",") == true)
                                    {
                                        string[] monto = cells[rowIterator, 19 - 1].Value.ToString().Split(',');
                                        parteEntera = monto[0];
                                        parteDecimal = monto[1];

                                        if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                        {

                                            observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, 19 - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                        }
                                    }
                                }
                                //typePayment es 1 se verifica el numberQuota
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && (cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) != false) && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) >= 1 && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) > 100) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + numberQuotaCabecera + " :" + cells[rowIterator, 15 - 1].Value.ToString() + " debe ser menor o gual a 100"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //si es typePayment es 1 se verifican las coutas que estan de la columna 22-23 en adelante
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) != false && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) >= 1 && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) <= 100)
                                {

                                    //coutas definidas por el numerQuota 
                                    int numberQuota = Convert.ToInt32(cells[rowIterator, 15 - 1].Value);
                                    for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                    {
                                        int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                        int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                        //if (cells[rowIterator, 14-1 ].Value != null && cells[rowIterator, 14-1 ].Value.ToString() == "1" && cells[rowIterator, filaNetAmountCuota-1 ].Value != null && esDecimal(cells[rowIterator, filaNetAmountCuota-1 ].Value.ToString()) == false &&  cells[rowIterator, filaNetAmountCuota-1 ].Value.ToString().Length > 22 ) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filaNetAmountCuota-1 ].Value.ToString() + " debe tener un maximo de 22 digitos"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                        if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, filaNetAmountCuota - 1].Value != null && cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Length > 27)
                                        {
                                            parteEntera = null;
                                            parteDecimal = null;
                                            if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Contains(".") == false && cells[rowIterator, 19 - 1].Value.ToString().Contains(",") == false)
                                            {

                                                if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Length > 22)
                                                {

                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + netAmountCabecera + " :" + cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                            if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Contains(".") == true)
                                            {
                                                string[] monto = cells[rowIterator, 19 - 1].Value.ToString().ToString().Split('.');
                                                parteEntera = monto[0];
                                                parteDecimal = monto[1];
                                                if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                {

                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                            if (cells[rowIterator, filaNetAmountCuota - 1].Value.ToString().Contains(",") == true)
                                            {
                                                string[] monto = hoy.ToString().Split(',');
                                                parteEntera = monto[0];
                                                parteDecimal = monto[1];

                                                if (parteEntera.Length > 22 || parteDecimal.Length > 4)
                                                {

                                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo MONTO_NETO_CUOTA_" + contadorCuotas + " :" + cells[rowIterator, filaNetAmountCuota - 1].Value.ToString() + " debe tener un maximo de 22 numeros y 4 decimales"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                                }
                                            }
                                        }

                                    }
                                }


                                //**VALIDAR FECHAS**
                                //validar si deliveryDateAcq  es menor o igual a la fecha actual
                                string[] hoyseparado = hoy.ToString().Split(' ');
                                string hoy2 = hoyseparado[0];
                                if ((cells[rowIterator, 16 - 1].Value != null && cells[rowIterator, 16 - 1].Value.ToString() != "" && esDateTime(cells[rowIterator, 16 - 1].Value.ToString()) != false && Convert.ToDateTime(cells[rowIterator, 16 - 1].Value.ToString()) > hoy)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + deliverDateAcqCabecera + " :" + cells[rowIterator, 16 - 1].Value.ToString() + " es mayor a la fecha actual: " + hoy2; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }
                                //typePayment es 0, se verifica que paymantDate de la fila 18, sea mayor a deliveryDateAcq
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "0" && cells[rowIterator, 16 - 1].Value != null && cells[rowIterator, 18 - 1].Value != null
                                   && cells[rowIterator, 16 - 1].Value.ToString() != "" && cells[rowIterator, 18 - 1].Value.ToString() != "" && Convert.ToDateTime(cells[rowIterator, 16 - 1].Value.ToString()) > hoy
                                   && Convert.ToDateTime(cells[rowIterator, 16 - 1].Value.ToString()) >= Convert.ToDateTime(cells[rowIterator, 18 - 1].Value.ToString()))
                                {
                                    observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + paymentDateCabecera + ": " + cells[rowIterator, 18 - 1].Value.ToString() + " debe ser mayor a: " + deliverDateAcqCabecera + " :" + cells[rowIterator, 16 - 1].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                }

                                //typePayment es 1, se verifica que paymantDate de la fila (23+2n) sea mayor a deliveryDateAcq
                                if (cells[rowIterator, 14 - 1].Value != null && cells[rowIterator, 14 - 1].Value.ToString() == "1" && cells[rowIterator, 15 - 1].Value != null && esNumerico(cells[rowIterator, 15 - 1].Value.ToString()) != false
                                    && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) >= 1 && Convert.ToInt32(cells[rowIterator, 15 - 1].Value) <= 100 && (cells[rowIterator, 16 - 1].Value != null && cells[rowIterator, 16 - 1].Value.ToString() != ""
                                    && esDateTime(cells[rowIterator, 16 - 1].Value.ToString()) != false && Convert.ToDateTime(cells[rowIterator, 16 - 1].Value.ToString()) <= hoy))
                                {
                                    {
                                        //coutas definidas por el numerQuota 
                                        int numberQuota = Convert.ToInt32(cells[rowIterator, 15 - 1].Value);
                                        for (int contadorCuotas = 1; contadorCuotas <= numberQuota; contadorCuotas++)
                                        {
                                            int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));

                                            if (cells[rowIterator, filapaymentCoutaCuota - 1].Value != null && cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString() != "" && esDateTime(cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString()) != false && Convert.ToDateTime(cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString()) <= Convert.ToDateTime(cells[rowIterator, 16 - 1].Value.ToString()))
                                            {
                                                observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo FECHA_PAGO_CUOTA_" + contadorCuotas + " : " + cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString() + " debe ser mayor a " + deliverDateAcqCabecera + " : " + cells[rowIterator, 16 - 1].Value.ToString(); csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord();
                                            }
                                        }
                                    }
                                }

                                //**VALIDAR providerRuc**
                                if ((cells[rowIterator, 2 - 1].Value != null && esNumerico(cells[rowIterator, 2 - 1].Value.ToString()) != false && ValidationRUC(cells[rowIterator, 2 - 1].Value.ToString()) == false)) { observacion1 = null; contadorFaltanCamposFacturasExcel = contadorFaltanCamposFacturasExcel + 1; observacion1 = observacion1 + "El campo " + providerRucCabecera + " : " + cells[rowIterator, 2 - 1].Value.ToString() + " no es un RUC válido"; csvWriter.WriteField("El registro que se encuentra en la línea " + rowIterator + " contiene el siguiente error."); csvWriter.WriteField(observacion1); csvWriter.NextRecord(); }

                                //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (cells[rowIterator, 1 - 1].Value == null && cells[rowIterator, 2 - 1].Value == null && cells[rowIterator, 3 - 1].Value == null && cells[rowIterator, 4 - 1].Value == null && cells[rowIterator, 5 - 1].Value == null && cells[rowIterator, 6 - 1].Value == null && cells[rowIterator, 7 - 1].Value == null && cells[rowIterator, 8 - 1].Value == null
                                        && cells[rowIterator, 9 - 1].Value == null && cells[rowIterator, 10 - 1].Value == null && cells[rowIterator, 11 - 1].Value == null && cells[rowIterator, 12 - 1].Value == null && cells[rowIterator, 13 - 1].Value == null && cells[rowIterator, 14 - 1].Value == null && cells[rowIterator, 15 - 1].Value == null && cells[rowIterator, 16 - 1].Value == null
                                        && cells[rowIterator, 17 - 1].Value == null && cells[rowIterator, 18 - 1].Value == null && cells[rowIterator, 19 - 1].Value == null && cells[rowIterator, 20 - 1].Value == null && cells[rowIterator, 21 - 1].Value == null && cells[rowIterator, 22 - 1].Value == null && cells[rowIterator, 23 - 1].Value == null)
                                {
                                    break;
                                }
                                if (contadorFaltanCamposFacturasExcel > 0)
                                {
                                    contadorFaltanCamposFacturasExcelTotal = contadorFaltanCamposFacturasExcelTotal + 1;
                                }
                                //}
                            }

                        }
                        textWriter.Close();
                    }


                    if (contadorFaltanCamposFacturasExcelTotal > 0)
                    {
                        exito = false;
                    }
                    else
                    {
                        exito = true;
                    }
                    return exito;

                }
                else
                {
                    return false;
                }
            }


            catch (Exception ex)
            {
                rg.eLog("Error a validar campos vacios y tipo de datos Xls: " + ex.ToString());
                return false;
            }
        }


        //Generar el archivo de errores
        public void GenerarArchivoErroresVacio(int fila)
{

    try
    {
        //abremos el archivo errores y guardamos los errores recibbidos
        string Directorio = Properties.Settings.Default.rutaErrores;
        string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

        // Valida y crea la carpeta definida en el config
        if (!(Directory.Exists(ruta)))
        {
            Directory.CreateDirectory(ruta);
        }
        if (Directory.Exists(ruta))
        {
            string pathEliminar = ruta;
            string filePathEliminar = pathEliminar + "/Errores.csv";
            if (filePathEliminar != null)
            {
                //Elimino archivos del servidor
                if (System.IO.File.Exists(filePathEliminar))
                {
                    System.IO.File.Delete(filePathEliminar);
                }
            }
        }
        string path = ruta;//Server.MapPath("~/UploadedFiles/");
                           //string filePath = Server.MapPath("~/UploadedFiles/Errores.csv");
        string filePath = path + "/Errores.csv";
        using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))
        {
            var csvWriter = new CsvWriter(textWriter);
            csvWriter.Configuration.Delimiter = ";";
            csvWriter.WriteField("El registro que se encuentra en la línea " + fila + " contiene el siguiente error.");
            csvWriter.WriteField("Todos los campos están vacíos, por favor elija una factura que si contenga datos.");
            csvWriter.NextRecord();
            textWriter.Close();
        }
    

    }
            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener el archivo errores.csv cuando el excel esta vacio: " + ex.ToString());

            }

        }


        //Generar el archivo de errores si superar capacidad excel
        public void GenerarArchivoErroresSobreCapacidad()
        {

            try
            {
                //abremos el archivo errores y guardamos los errores recibbidos
                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }
                if (Directory.Exists(ruta))
                {
                    string pathEliminar = ruta;
                    string filePathEliminar = pathEliminar + "/Errores.csv";
                    if (filePathEliminar != null)
                    {
                        //Elimino archivos del servidor
                        if (System.IO.File.Exists(filePathEliminar))
                        {
                            System.IO.File.Delete(filePathEliminar);
                        }
                    }
                }
                string path = ruta;//Server.MapPath("~/UploadedFiles/");
                                   //string filePath = Server.MapPath("~/UploadedFiles/Errores.csv");
                string filePath = path + "/Errores.csv";
                using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))
                {
                    var csvWriter = new CsvWriter(textWriter);
                    csvWriter.Configuration.Delimiter = ";";
                    csvWriter.WriteField("Fila del Error");
                    csvWriter.WriteField("Observación");
                    csvWriter.NextRecord();
                    csvWriter.WriteField("El registro que se encuentra en la línea " + 1 + " contiene el siguiente error.");
                    csvWriter.WriteField("El archivo XLS debe contener un máximo de 25.000 registros.");
                    csvWriter.NextRecord();
                    textWriter.Close();
                }


            }
            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener el archivo errores.csv cuando el excel esta vacio: " + ex.ToString());

            }

        }

        //Generar el archivo de errores si superar capacidad excel
        public void GenerarArchivoErroresSobreCapacidadOffice2003()
        {

            try
            {
                //abremos el archivo errores y guardamos los errores recibbidos
                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }
                if (Directory.Exists(ruta))
                {
                    string pathEliminar = ruta;
                    string filePathEliminar = pathEliminar + "/Errores.csv";
                    if (filePathEliminar != null)
                    {
                        //Elimino archivos del servidor
                        if (System.IO.File.Exists(filePathEliminar))
                        {
                            System.IO.File.Delete(filePathEliminar);
                        }
                    }
                }
                string path = ruta;//Server.MapPath("~/UploadedFiles/");
                                   //string filePath = Server.MapPath("~/UploadedFiles/Errores.csv");
                string filePath = path + "/Errores.csv";
                using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))
                {
                    var csvWriter = new CsvWriter(textWriter);
                    csvWriter.Configuration.Delimiter = ";";
                    csvWriter.WriteField("Fila del Error");
                    csvWriter.WriteField("Observación");
                    csvWriter.NextRecord();
                    csvWriter.WriteField("El registro que se encuentra en la línea " + 1 + " contiene el siguiente error.");
                    csvWriter.WriteField("El archivo XLS debe contener un máximo de 10.000 registros.");
                    csvWriter.NextRecord();
                    textWriter.Close();
                }


            }
            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener el archivo errores.csv cuando el excel esta vacio: " + ex.ToString());

            }

        }

        //Desacargar archivo de errores
        public string DownloadVIEJO(string file)
        {
            try
            {
                //se descarga el archivo errores en la misma pagina

                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }
                string path = ruta;
                string actualFilePath = path + "/Errores.csv";
                HttpContext.Response.ContentType = "APPLICATION/OCTET-STREAM";
                string filename = Path.GetFileName(actualFilePath);
                String Header = "Attachment; Filename=" + filename;
                HttpContext.Response.AppendHeader("Content-Disposition", Header);
                HttpContext.Response.WriteFile(actualFilePath);
                HttpContext.Response.End();
                //Eliminar el archivo y limpiar el archivo si ya existe
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                if (archivo.name != null && archivo.ruta != null)
                {
                    //Elimino archivo del servidor
                    if (System.IO.File.Exists(archivo.ruta + archivo.name))
                    {
                       
                        System.IO.File.Delete(archivo.ruta + archivo.name);
                        //System.IO.File.Delete(path + nombre_archivo);
                    }
                }
                archivo.name = null;
                archivo.ruta = null;
                archivo.nameOriginal = null;
                return "";
            }
            catch (Exception ex)
            {
                rg.eLog("Error descargar archivo de errores: " + ex.ToString());
                return "";
            }
            
        }
        //Desacargar archivo de errores
        //public string Download(string file)
              public string Download()
        {
            try
            {
                //se descarga el archivo errores en la misma pagina

                string Directorio = Properties.Settings.Default.rutaErrores;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                

                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }
                string path = ruta;
                string actualFilePath = path + "/Errores.csv";
                //string rutaComprimido =path +"/Errores.zip";

                string archivoOriginal = actualFilePath;
                string directorioComprimidos = Properties.Settings.Default.rutaComprimidos;
                string rutaComprimidos = System.AppDomain.CurrentDomain.BaseDirectory + directorioComprimidos + "/";
                string directotorioDestino = rutaComprimidos + "/Errores.zip";

                // verificar si existe el archivo y borrarlo para sobre escribirlo
                if (System.IO.File.Exists(directotorioDestino))
                {
                    System.IO.File.Delete(directotorioDestino);
                }

                //Comprimir
                ZipFile.CreateFromDirectory(path, directotorioDestino);

                HttpContext.Response.ContentType = "APPLICATION/OCTET-STREAM";
                //string filename = Path.GetFileName(actualFilePath);
                string filename = Path.GetFileName(directotorioDestino);
                String Header = "Attachment; Filename=" + filename;
                HttpContext.Response.AppendHeader("Content-Disposition", Header);
                //HttpContext.Response.WriteFile(actualFilePath);
                HttpContext.Response.WriteFile(directotorioDestino);

                HttpContext.Response.End();
                //Eliminar el archivo y limpiar el archivo si ya existe
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                if (archivo.name != null && archivo.ruta != null)
                {
                    //Elimino archivo del servidor
                    if (System.IO.File.Exists(archivo.ruta + archivo.name))
                    {

                        System.IO.File.Delete(archivo.ruta + archivo.name);
                        //System.IO.File.Delete(path + nombre_archivo);
                    }
                }
                archivo.name = null;
                archivo.ruta = null;
                archivo.nameOriginal = null;
                //Elimino archivo del servidor
                if (System.IO.File.Exists(actualFilePath))
                {

                    System.IO.File.Delete(actualFilePath);
                    //System.IO.File.Delete(path + nombre_archivo);
                }

                //Elimino archivo del servidor
                if (System.IO.File.Exists(directotorioDestino))
                {

                    System.IO.File.Delete(directotorioDestino);
                    //System.IO.File.Delete(path + nombre_archivo);
                }
                return "";
            }
            catch (Exception ex)
            {
                rg.eLog("Error descargar archivo de errores: " + ex.ToString());
                return "";
            }

        }

        //Comprobar si el tipo de dato es numerico
        public bool esNumerico(String numero)
        {
            try
            {
                //Convert.ToInt64(numero);
                long.Parse(numero);

                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }
        //Comprobar si el tipo de dato es de tipo fecha
        public bool esDateTime(String numero)
        {
            try
            {
                Convert.ToDateTime(numero);
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }

        //Comprobar si el tipo de dato es decimal
        public bool esDecimal(String numero)
        {
            try
            {
                Convert.ToDecimal(numero);
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }

        //comprobar si el RUC peruano es valido
        public static bool ValidationRUC(string ruc)
        {

            string msj = string.Empty;

            if (ruc.Length != 11)
            {
                msj = "NUMERO DE DIGITOS INVALIDO!!!.";
                return false;
            }

            int dig01 = Convert.ToInt32(ruc.Substring(0, 1)) * 5;
            int dig02 = Convert.ToInt32(ruc.Substring(1, 1)) * 4;
            int dig03 = Convert.ToInt32(ruc.Substring(2, 1)) * 3;
            int dig04 = Convert.ToInt32(ruc.Substring(3, 1)) * 2;
            int dig05 = Convert.ToInt32(ruc.Substring(4, 1)) * 7;
            int dig06 = Convert.ToInt32(ruc.Substring(5, 1)) * 6;
            int dig07 = Convert.ToInt32(ruc.Substring(6, 1)) * 5;
            int dig08 = Convert.ToInt32(ruc.Substring(7, 1)) * 4;
            int dig09 = Convert.ToInt32(ruc.Substring(8, 1)) * 3;
            int dig10 = Convert.ToInt32(ruc.Substring(9, 1)) * 2;
            int dig11 = Convert.ToInt32(ruc.Substring(10, 1));

            int suma = dig01 + dig02 + dig03 + dig04 + dig05 + dig06 + dig07 + dig08 + dig09 + dig10;
            int residuo = suma % 11;
            int resta = 11 - residuo;

            int digChk = 0;
            if (resta == 10)
            {
                digChk = 0;
            }
            else if (resta == 11)
            {
                digChk = 1;
            }
            else
            {
                digChk = resta;
            }

            if (dig11 == digChk)
            {
                return true;
            }
            else
            {
                msj = "NUMERO DE RUC INVALIDO!!!.";
                return false;
            }


        }

       

        //Construir la lista InvoiceInformationAdditional_Type que se enviara al ws_04003
        public List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> ExcelToListWs4003()
        {
           
            listaFacturas4003.Clear();
            listaCuotas4003.Clear();
            lista2Cuotas4003 = null;
            //listaCuotas4003Aux = null;
            InterfazCavali.InputService04003.Payment_Type[] listaCuotas40032;
            string monto = null;
            string number;//numer es el numero de la cuota
            try
            {

                //abrimos el xls y lo recorremos linea por linea
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {
                    byte[] fileBytes = new byte[file.Length];

                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));

                    using (var package = new ExcelPackage(file))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();


                        for (int rowIterator = 2; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                        {
                            //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura

                            if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                              && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                              && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                            {
                                break;
                            }
                            listaCuotas4003 = null;
                            listaCuotas4003.Clear();
                            lista2Cuotas4003 = null;
                            InterfazCavali.InputService04003.InvoiceCode_Type elementoListaFactura4003IdentificadorFactura = new InterfazCavali.InputService04003.InvoiceCode_Type();
                            InterfazCavali.InputService04003.InvoiceInformationAdditional_Type elementoListaFactura4003 = new InterfazCavali.InputService04003.InvoiceInformationAdditional_Type();

                            //verificar que los campos del excel obligatorios no sean null
                            elementoListaFactura4003IdentificadorFactura.providerRuc = workSheet.Cells[rowIterator, 2].Value.ToString();
                            elementoListaFactura4003IdentificadorFactura.invoiceType = workSheet.Cells[rowIterator, 1].Value.ToString();
                            //elementoListaFactura4003IdentificadorFactura.series = workSheet.Cells[rowIterator, 3].Value.ToString();
                            elementoListaFactura4003IdentificadorFactura.numeration = workSheet.Cells[rowIterator, 4].Value.ToString();
                            //elementoListaFactura4003IdentificadorFactura.authorizationNumber = workSheet.Cells[rowIterator, 4].Value.ToString();

                            if (workSheet.Cells[rowIterator, 3].Value.ToString().Length < 4)
                            {
                                int largo = workSheet.Cells[rowIterator, 3].Value.ToString().Length;
                                string series2 = workSheet.Cells[rowIterator, 3].Value.ToString();
                                string ceros = null;
                                for (int contCeros = 1; contCeros <= 4 - largo; contCeros++)
                                {
                                    ceros = "0" + ceros;
                                }
                                series2 = ceros + series2;
                                elementoListaFactura4003IdentificadorFactura.series = series2;
                            }
                            else
                            {
                                elementoListaFactura4003IdentificadorFactura.series = workSheet.Cells[rowIterator, 3].Value.ToString();
                            }

                            elementoListaFactura4003.invoiceCode = elementoListaFactura4003IdentificadorFactura;

                            elementoListaFactura4003.expirationDate = workSheet.Cells[rowIterator, 5].Value.ToString();
                            elementoListaFactura4003.department = workSheet.Cells[rowIterator, 6].Value.ToString();
                            elementoListaFactura4003.province = workSheet.Cells[rowIterator, 7].Value.ToString();
                            elementoListaFactura4003.district = workSheet.Cells[rowIterator, 8].Value.ToString();
                            elementoListaFactura4003.addressSupplier = workSheet.Cells[rowIterator, 9].Value.ToString();
                            elementoListaFactura4003.acqDepartment = workSheet.Cells[rowIterator, 10].Value.ToString();
                            elementoListaFactura4003.acqProvince = workSheet.Cells[rowIterator, 11].Value.ToString();
                            elementoListaFactura4003.acqDistrict = workSheet.Cells[rowIterator, 12].Value.ToString();
                            elementoListaFactura4003.addressAcquirer = workSheet.Cells[rowIterator, 13].Value.ToString();
                            elementoListaFactura4003.typePayment = workSheet.Cells[rowIterator, 14].Value.ToString();

                            //elementoListaFactura4003.numberQuota = workSheet.Cells[rowIterator, 15].Value.ToString();
                            //elementoListaFactura4003.deliverDateAcq = workSheet.Cells[rowIterator, 16].Value.ToString();
                            //elementoListaFactura4003.aceptedDate = workSheet.Cells[rowIterator, 17].Value.ToString();
                            //elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, 18].Value.ToString();
                            //elementoListaFactura4003.netAmount = Convert.ToDecimal(workSheet.Cells[rowIterator, 19].Value.ToString());
                            //elementoListaFactura4003.other1 = workSheet.Cells[rowIterator, 20].Value.ToString();
                            //elementoListaFactura4003.other2 = workSheet.Cells[rowIterator, ].Value.ToString();
                            //elementoListaFactura4003.additionalField1 = workSheet.Cells[rowIterator, 21].Value.ToString();
                            //elementoListaFactura4003.additionalField2 = workSheet.Cells[rowIterator, ].Value.ToString();


                            if (workSheet.Cells[rowIterator, 14].Value.ToString() == "1")
                            {
                                elementoListaFactura4003.numberQuota = workSheet.Cells[rowIterator, 15].Value.ToString();
                                elementoListaFactura4003.paymentDate = "01/01/1900";
                                elementoListaFactura4003.netAmount = Convert.ToDecimal("0");
                            }
                            else
                            {
                                elementoListaFactura4003.numberQuota = "0";
                                elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, 18].Value.ToString();
                                elementoListaFactura4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, 19].Value.ToString());
                            }

                            elementoListaFactura4003.deliverDateAcq = workSheet.Cells[rowIterator, 16].Value.ToString();
                            if (workSheet.Cells[rowIterator, 17].Value != null && workSheet.Cells[rowIterator, 17].Value.ToString() != "")
                            {
                                elementoListaFactura4003.aceptedDate = workSheet.Cells[rowIterator, 17].Value.ToString();
                            }
                            else
                            {
                                elementoListaFactura4003.aceptedDate = "01/01/1900";
                            }
                            if (workSheet.Cells[rowIterator, 20].Value != null && workSheet.Cells[rowIterator, 20].Value.ToString() != "")
                            {

                                elementoListaFactura4003.other1 = workSheet.Cells[rowIterator, 20].Value.ToString();
                            }
                            if (workSheet.Cells[rowIterator, 21].Value != null && workSheet.Cells[rowIterator, 21].Value.ToString() != "")
                            {
                                elementoListaFactura4003.additionalField1 = workSheet.Cells[rowIterator, 21].Value.ToString();
                            }

                            //si el pago es unico la coutas es unica y es la de la factura (solo se guyarda como respaldo)


                            if (Convert.ToInt32(workSheet.Cells[rowIterator, 14].Value.ToString()) == 0)
                            {

                             
                                int filaNetAmountCuota = 19;
                                int filapaymentCoutaCuota = 18;

                                monto = workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString();
                                if (monto.Contains(".") == true)
                                {
                                    monto=monto.Replace('.', ',');
                                }
                                elementoListaFactura4003.numberQuota = "0";
                                //elementoListaFactura4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                elementoListaFactura4003.netAmount = Decimal.Parse(monto);
                                elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString();
                                
                            }


                            //si el pago es en  coutas, se  gurdan las coutas que indica el numberquota

                            if (workSheet.Cells[rowIterator, 14].Value.ToString() == "1")
                            {
                                List<InterfazCavali.InputService04003.Payment_Type> listaCuotas4003Aux = new List<InterfazCavali.InputService04003.Payment_Type>();

                                //cuotas
                                number = workSheet.Cells[rowIterator, 15].Value.ToString();

                              
                                for (int contadorCuotas = 1; contadorCuotas <= Convert.ToInt32(number); contadorCuotas++)
                                {
                                    InterfazCavali.InputService04003.Payment_Type elementoListaCuota4003 = new InterfazCavali.InputService04003.Payment_Type();

                                    int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                    int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));


                                    elementoListaCuota4003.number = contadorCuotas.ToString();
                                    monto = workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString();
                                    if (monto.Contains(".")==true) {
                                        monto=monto.Replace('.', ',');
                                      }

                                    //elementoListaCuota4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                    elementoListaCuota4003.netAmount = Decimal.Parse(monto);
                                    elementoListaCuota4003.paymentDate = workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString();

                                    listaCuotas4003Aux.Add(elementoListaCuota4003);
                                    //listaCuotas4003.Add(elementoListaCuota4003);
                                }

                                listaCuotas40032 = listaCuotas4003Aux.ToArray();
                                //lista2Cuotas4003 = listaCuotas4003.ToArray();

                                //elementoListaFactura4003.paymentDetail = listaCuotas40032;
                                elementoListaFactura4003.paymentDetail = listaCuotas40032;

                            }


                            listaFacturas4003.Add(elementoListaFactura4003);


                        }

                    }
                }
                file.Close();
            }


            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener los datos de las facturas y coutas desde la BD para crear la lista a enviar al ws final: " + ex.ToString());
                return listaFacturas4003;
            }

            //Eliminar el archivo y limpiar archivo 
            if (archivo.name != null && archivo.ruta != null)
            {
                //Elimino archivos del servidor
                if (System.IO.File.Exists(archivo.ruta + archivo.name))
                {
                    System.IO.File.Delete(archivo.ruta + archivo.name);
                }
            }
            archivo.name = null;
            archivo.ruta = null;
            archivo.nameOriginal = null;

            return listaFacturas4003;
        }



        //Construir la lista InvoiceInformationAdditional_Type que se enviara al ws_04003
        public List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> ExcelToListWs4003version2()
        {

            
               List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> listaFacturas4003version2 = new List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type>();
         List<InterfazCavali.InputService04003.Payment_Type> listaCuotas4003version2 = new List<InterfazCavali.InputService04003.Payment_Type>();
         InterfazCavali.InputService04003.InvoiceInformationAdditional_Type[] lista2Facturas4003version2;
        InterfazCavali.InputService04003.Payment_Type[] lista2Cuotas4003version2;
        //listaCuotas4003Aux = null;
        InterfazCavali.InputService04003.Payment_Type[] listaCuotas40032;
            string monto = null;
            string number;//numer es el numero de la cuota
            try
            {

                //abrimos el xls y lo recorremos linea por linea
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {
                    byte[] fileBytes = new byte[file.Length];

                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));

                    using (var package = new ExcelPackage(file))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();


                        for (int rowIterator = 2; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                        {
                            //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura

                            if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                              && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                              && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                            {
                                break;
                            }
                            listaCuotas4003version2 = null;
                            lista2Cuotas4003version2 = null;
                            InterfazCavali.InputService04003.InvoiceCode_Type elementoListaFactura4003IdentificadorFactura = new InterfazCavali.InputService04003.InvoiceCode_Type();
                            InterfazCavali.InputService04003.InvoiceInformationAdditional_Type elementoListaFactura4003 = new InterfazCavali.InputService04003.InvoiceInformationAdditional_Type();

                            //verificar que los campos del excel obligatorios no sean null
                            elementoListaFactura4003IdentificadorFactura.providerRuc = workSheet.Cells[rowIterator, 2].Value.ToString();
                            elementoListaFactura4003IdentificadorFactura.invoiceType = workSheet.Cells[rowIterator, 1].Value.ToString();
                            //elementoListaFactura4003IdentificadorFactura.series = workSheet.Cells[rowIterator, 3].Value.ToString();
                            elementoListaFactura4003IdentificadorFactura.numeration = workSheet.Cells[rowIterator, 4].Value.ToString();
                            //elementoListaFactura4003IdentificadorFactura.authorizationNumber = workSheet.Cells[rowIterator, 4].Value.ToString();

                            if (workSheet.Cells[rowIterator, 3].Value.ToString().Length < 4)
                            {
                                int largo = workSheet.Cells[rowIterator, 3].Value.ToString().Length;
                                string series2 = workSheet.Cells[rowIterator, 3].Value.ToString();
                                string ceros = null;
                                for (int contCeros = 1; contCeros <= 4 - largo; contCeros++)
                                {
                                    ceros = "0" + ceros;
                                }
                                series2 = ceros + series2;
                                elementoListaFactura4003IdentificadorFactura.series = series2;
                            }
                            else
                            {
                                elementoListaFactura4003IdentificadorFactura.series = workSheet.Cells[rowIterator, 3].Value.ToString();
                            }

                            elementoListaFactura4003.invoiceCode = elementoListaFactura4003IdentificadorFactura;

                            elementoListaFactura4003.expirationDate = workSheet.Cells[rowIterator, 5].Value.ToString();
                            elementoListaFactura4003.department = workSheet.Cells[rowIterator, 6].Value.ToString();
                            elementoListaFactura4003.province = workSheet.Cells[rowIterator, 7].Value.ToString();
                            elementoListaFactura4003.district = workSheet.Cells[rowIterator, 8].Value.ToString();
                            elementoListaFactura4003.addressSupplier = workSheet.Cells[rowIterator, 9].Value.ToString();
                            elementoListaFactura4003.acqDepartment = workSheet.Cells[rowIterator, 10].Value.ToString();
                            elementoListaFactura4003.acqProvince = workSheet.Cells[rowIterator, 11].Value.ToString();
                            elementoListaFactura4003.acqDistrict = workSheet.Cells[rowIterator, 12].Value.ToString();
                            elementoListaFactura4003.addressAcquirer = workSheet.Cells[rowIterator, 13].Value.ToString();
                            elementoListaFactura4003.typePayment = workSheet.Cells[rowIterator, 14].Value.ToString();

                            //elementoListaFactura4003.numberQuota = workSheet.Cells[rowIterator, 15].Value.ToString();
                            //elementoListaFactura4003.deliverDateAcq = workSheet.Cells[rowIterator, 16].Value.ToString();
                            //elementoListaFactura4003.aceptedDate = workSheet.Cells[rowIterator, 17].Value.ToString();
                            //elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, 18].Value.ToString();
                            //elementoListaFactura4003.netAmount = Convert.ToDecimal(workSheet.Cells[rowIterator, 19].Value.ToString());
                            //elementoListaFactura4003.other1 = workSheet.Cells[rowIterator, 20].Value.ToString();
                            //elementoListaFactura4003.other2 = workSheet.Cells[rowIterator, ].Value.ToString();
                            //elementoListaFactura4003.additionalField1 = workSheet.Cells[rowIterator, 21].Value.ToString();
                            //elementoListaFactura4003.additionalField2 = workSheet.Cells[rowIterator, ].Value.ToString();


                            if (workSheet.Cells[rowIterator, 14].Value.ToString() == "1")
                            {
                                elementoListaFactura4003.numberQuota = workSheet.Cells[rowIterator, 15].Value.ToString();
                                elementoListaFactura4003.paymentDate = "01/01/1900";
                                elementoListaFactura4003.netAmount = Convert.ToDecimal("0");
                            }
                            else
                            {
                                elementoListaFactura4003.numberQuota = "0";
                                elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, 18].Value.ToString();
                                elementoListaFactura4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, 19].Value.ToString());
                            }

                            elementoListaFactura4003.deliverDateAcq = workSheet.Cells[rowIterator, 16].Value.ToString();
                            if (workSheet.Cells[rowIterator, 17].Value != null && workSheet.Cells[rowIterator, 17].Value.ToString() != "")
                            {
                                elementoListaFactura4003.aceptedDate = workSheet.Cells[rowIterator, 17].Value.ToString();
                            }
                            else
                            {
                                elementoListaFactura4003.aceptedDate = "01/01/1900";
                            }
                            if (workSheet.Cells[rowIterator, 20].Value != null && workSheet.Cells[rowIterator, 20].Value.ToString() != "")
                            {

                                elementoListaFactura4003.other1 = workSheet.Cells[rowIterator, 20].Value.ToString();
                            }
                            if (workSheet.Cells[rowIterator, 21].Value != null && workSheet.Cells[rowIterator, 21].Value.ToString() != "")
                            {
                                elementoListaFactura4003.additionalField1 = workSheet.Cells[rowIterator, 21].Value.ToString();
                            }

                            //si el pago es unico la coutas es unica y es la de la factura (solo se guyarda como respaldo)


                            if (Convert.ToInt32(workSheet.Cells[rowIterator, 14].Value.ToString()) == 0)
                            {


                                int filaNetAmountCuota = 19;
                                int filapaymentCoutaCuota = 18;

                                monto = workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString();
                                if (monto.Contains(".") == true)
                                {
                                    monto = monto.Replace('.', ',');
                                }
                                elementoListaFactura4003.numberQuota = "0";
                                //elementoListaFactura4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                elementoListaFactura4003.netAmount = Decimal.Parse(monto);
                                elementoListaFactura4003.paymentDate = workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString();

                            }


                            //si el pago es en  coutas, se  gurdan las coutas que indica el numberquota

                            if (workSheet.Cells[rowIterator, 14].Value.ToString() == "1")
                            {
                                List<InterfazCavali.InputService04003.Payment_Type> listaCuotas4003Aux = new List<InterfazCavali.InputService04003.Payment_Type>();

                                //cuotas
                                number = workSheet.Cells[rowIterator, 15].Value.ToString();


                                for (int contadorCuotas = 1; contadorCuotas <= Convert.ToInt32(number); contadorCuotas++)
                                {
                                    InterfazCavali.InputService04003.Payment_Type elementoListaCuota4003 = new InterfazCavali.InputService04003.Payment_Type();

                                    int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                    int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));


                                    elementoListaCuota4003.number = contadorCuotas.ToString();
                                    monto = workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString();
                                    if (monto.Contains(".") == true)
                                    {
                                        monto = monto.Replace('.', ',');
                                    }

                                    //elementoListaCuota4003.netAmount = Decimal.Parse(workSheet.Cells[rowIterator, filaNetAmountCuota].Value.ToString());
                                    elementoListaCuota4003.netAmount = Decimal.Parse(monto);
                                    elementoListaCuota4003.paymentDate = workSheet.Cells[rowIterator, filapaymentCoutaCuota].Value.ToString();

                                    listaCuotas4003Aux.Add(elementoListaCuota4003);
                                    //listaCuotas4003.Add(elementoListaCuota4003);
                                }

                                listaCuotas40032 = listaCuotas4003Aux.ToArray();
                                //lista2Cuotas4003 = listaCuotas4003.ToArray();

                                //elementoListaFactura4003.paymentDetail = listaCuotas40032;
                                elementoListaFactura4003.paymentDetail = listaCuotas40032;

                            }


                            listaFacturas4003version2.Add(elementoListaFactura4003);


                        }

                    }
                }
                file.Close();
            }


            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener los datos de las facturas y coutas desde la BD para crear la lista a enviar al ws final: " + ex.ToString());
                return listaFacturas4003;
            }

            //Eliminar el archivo y limpiar archivo 
            if (archivo.name != null && archivo.ruta != null)
            {
                //Elimino archivos del servidor
                if (System.IO.File.Exists(archivo.ruta + archivo.name))
                {
                    System.IO.File.Delete(archivo.ruta + archivo.name);
                }
            }
            archivo.name = null;
            archivo.ruta = null;
            archivo.nameOriginal = null;

            return listaFacturas4003version2;
        }

       


        //Construir la lista InvoiceInformationAdditional_Type que se enviara al ws_04003
        public List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> Excel2003ToListWs4003()
        {
            listaFacturas4003.Clear();
            listaCuotas4003.Clear();
            lista2Cuotas4003 = null;
            //listaCuotas4003Aux = null;
            InterfazCavali.InputService04003.Payment_Type[] listaCuotas40032;
            string monto = null;
            string number;//numer es el numero de la cuota
            try
            {
                if (archivo.name != "" || archivo.ruta != "" && (archivo.extension == "XLS" || archivo.extension == "xls"))
                {
                    //FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);



                    Workbook wb = new Workbook(archivo.ruta + archivo.name);

                    //Get the first worksheet.
                    Worksheet worksheet = wb.Worksheets[0];

                    //Get cells
                    Cells cells = worksheet.Cells;

                    // Get row and column count
                    int rowCount = cells.MaxDataRow;
                    int columnCount = cells.MaxDataColumn;

                    // Current cell value
                    string strCell = "";


                    for (int rowIterator = 1; rowIterator <= rowCount; rowIterator++) // Numeration starts from 0 to MaxDataRow

                    {
                        //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura

                        if (cells[rowIterator, 1 - 1].Value == null && cells[rowIterator, 2 - 1].Value == null && cells[rowIterator, 3 - 1].Value == null && cells[rowIterator, 4 - 1].Value == null && cells[rowIterator, 5 - 1].Value == null && cells[rowIterator, 6 - 1].Value == null && cells[rowIterator, 7 - 1].Value == null && cells[rowIterator, 8 - 1].Value == null
                      && cells[rowIterator, 9 - 1].Value == null && cells[rowIterator, 10 - 1].Value == null && cells[rowIterator, 11 - 1].Value == null && cells[rowIterator, 12 - 1].Value == null && cells[rowIterator, 13 - 1].Value == null && cells[rowIterator, 14 - 1].Value == null && cells[rowIterator, 15 - 1].Value == null && cells[rowIterator, 16 - 1].Value == null
                      && cells[rowIterator, 17 - 1].Value == null && cells[rowIterator, 18 - 1].Value == null && cells[rowIterator, 19 - 1].Value == null && cells[rowIterator, 20 - 1].Value == null && cells[rowIterator, 21 - 1].Value == null && cells[rowIterator, 22 - 1].Value == null && cells[rowIterator, 23 - 1].Value == null)
                        {
                            break;
                        }

                        listaCuotas4003.Clear();
                        lista2Cuotas4003 = null;
                        InterfazCavali.InputService04003.InvoiceCode_Type elementoListaFactura4003IdentificadorFactura = new InterfazCavali.InputService04003.InvoiceCode_Type();
                        InterfazCavali.InputService04003.InvoiceInformationAdditional_Type elementoListaFactura4003 = new InterfazCavali.InputService04003.InvoiceInformationAdditional_Type();

                        //verificar que los campos del excel obligatorios no sean null
                        elementoListaFactura4003IdentificadorFactura.providerRuc = cells[rowIterator, 2 - 1].Value.ToString();
                        elementoListaFactura4003IdentificadorFactura.invoiceType = cells[rowIterator, 1 - 1].Value.ToString();
                        //elementoListaFactura4003IdentificadorFactura.series = cells[rowIterator, 3-1 ].Value.ToString();
                        elementoListaFactura4003IdentificadorFactura.numeration = cells[rowIterator, 4 - 1].Value.ToString();
                        //elementoListaFactura4003IdentificadorFactura.authorizationNumber = cells[rowIterator, 4-1 ].Value.ToString();

                        if (cells[rowIterator, 3 - 1].Value.ToString().Length < 4)
                        {
                            int largo = cells[rowIterator, 3 - 1].Value.ToString().Length;
                            string series2 = cells[rowIterator, 3 - 1].Value.ToString();
                            string ceros = null;
                            for (int contCeros = 1; contCeros <= 4 - largo; contCeros++)
                            {
                                ceros = "0" + ceros;
                            }
                            series2 = ceros + series2;
                            elementoListaFactura4003IdentificadorFactura.series = series2;
                        }
                        else
                        {
                            elementoListaFactura4003IdentificadorFactura.series = cells[rowIterator, 3 - 1].Value.ToString();
                        }

                        elementoListaFactura4003.invoiceCode = elementoListaFactura4003IdentificadorFactura;

                        elementoListaFactura4003.expirationDate = cells[rowIterator, 5 - 1].Value.ToString();
                        elementoListaFactura4003.department = cells[rowIterator, 6 - 1].Value.ToString();
                        elementoListaFactura4003.province = cells[rowIterator, 7 - 1].Value.ToString();
                        elementoListaFactura4003.district = cells[rowIterator, 8 - 1].Value.ToString();
                        elementoListaFactura4003.addressSupplier = cells[rowIterator, 9 - 1].Value.ToString();
                        elementoListaFactura4003.acqDepartment = cells[rowIterator, 10 - 1].Value.ToString();
                        elementoListaFactura4003.acqProvince = cells[rowIterator, 11 - 1].Value.ToString();
                        elementoListaFactura4003.acqDistrict = cells[rowIterator, 12 - 1].Value.ToString();
                        elementoListaFactura4003.addressAcquirer = cells[rowIterator, 13 - 1].Value.ToString();
                        elementoListaFactura4003.typePayment = cells[rowIterator, 14 - 1].Value.ToString();

                        //elementoListaFactura4003.numberQuota = cells[rowIterator, 15-1 ].Value.ToString();
                        //elementoListaFactura4003.deliverDateAcq = cells[rowIterator, 16-1 ].Value.ToString();
                        //elementoListaFactura4003.aceptedDate = cells[rowIterator, 17-1 ].Value.ToString();
                        //elementoListaFactura4003.paymentDate = cells[rowIterator, 18-1 ].Value.ToString();
                        //elementoListaFactura4003.netAmount = Convert.ToDecimal(cells[rowIterator, 19-1 ].Value.ToString());
                        //elementoListaFactura4003.other1 = cells[rowIterator, 20-1 ].Value.ToString();
                        //elementoListaFactura4003.other2 = cells[rowIterator, -1 ].Value.ToString();
                        //elementoListaFactura4003.additionalField1 = cells[rowIterator, 21-1 ].Value.ToString();
                        //elementoListaFactura4003.additionalField2 = cells[rowIterator, -1 ].Value.ToString();


                        if (cells[rowIterator, 14 - 1].Value.ToString() == "1")
                        {
                            elementoListaFactura4003.numberQuota = cells[rowIterator, 15 - 1].Value.ToString();
                            elementoListaFactura4003.paymentDate = "01/01/1900";
                            elementoListaFactura4003.netAmount = Convert.ToDecimal("0");
                        }
                        else
                        {
                            elementoListaFactura4003.numberQuota = "0";
                            elementoListaFactura4003.paymentDate = cells[rowIterator, 18 - 1].Value.ToString();
                            elementoListaFactura4003.netAmount = Decimal.Parse(cells[rowIterator, 19 - 1].Value.ToString());
                        }

                        elementoListaFactura4003.deliverDateAcq = cells[rowIterator, 16 - 1].Value.ToString();
                        if (cells[rowIterator, 17 - 1].Value != null && cells[rowIterator, 17 - 1].Value.ToString() != "")
                        {
                            elementoListaFactura4003.aceptedDate = cells[rowIterator, 17 - 1].Value.ToString();
                        }
                        else
                        {
                            elementoListaFactura4003.aceptedDate = "01/01/1900";
                        }
                        if (cells[rowIterator, 20 - 1].Value != null && cells[rowIterator, 20 - 1].Value.ToString() != "")
                        {

                            elementoListaFactura4003.other1 = cells[rowIterator, 20 - 1].Value.ToString();
                        }
                        if (cells[rowIterator, 21 - 1].Value != null && cells[rowIterator, 21 - 1].Value.ToString() != "")
                        {
                            elementoListaFactura4003.additionalField1 = cells[rowIterator, 21 - 1].Value.ToString();
                        }

                        //si el pago es unico la coutas es unica y es la de la factura (solo se guyarda como respaldo)


                        if (Convert.ToInt32(cells[rowIterator, 14 - 1].Value.ToString()) == 0)
                        {


                            int filaNetAmountCuota = 19;
                            int filapaymentCoutaCuota = 18;

                            monto = cells[rowIterator, filaNetAmountCuota - 1].Value.ToString();
                            if (monto.Contains(".") == true)
                            {
                                monto = monto.Replace('.', ',');
                            }
                            elementoListaFactura4003.numberQuota = "0";
                            //elementoListaFactura4003.netAmount = Decimal.Parse(cells[rowIterator, filaNetAmountCuota-1 ].Value.ToString());
                            elementoListaFactura4003.netAmount = Decimal.Parse(monto);
                            elementoListaFactura4003.paymentDate = cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString();

                        }


                        //si el pago es en  coutas, se  gurdan las coutas que indica el numberquota

                        if (cells[rowIterator, 14 - 1].Value.ToString() == "1")
                        {
                            List<InterfazCavali.InputService04003.Payment_Type> listaCuotas4003Aux = new List<InterfazCavali.InputService04003.Payment_Type>();

                            //cuotas
                            number = cells[rowIterator, 15 - 1].Value.ToString();


                            for (int contadorCuotas = 1; contadorCuotas <= Convert.ToInt32(number); contadorCuotas++)
                            {
                                InterfazCavali.InputService04003.Payment_Type elementoListaCuota4003 = new InterfazCavali.InputService04003.Payment_Type();

                                int filaNetAmountCuota = 22 + (2 * (contadorCuotas - 1));
                                int filapaymentCoutaCuota = 23 + (2 * (contadorCuotas - 1));


                                elementoListaCuota4003.number = contadorCuotas.ToString();
                                monto = cells[rowIterator, filaNetAmountCuota - 1].Value.ToString();
                                if (monto.Contains(".") == true)
                                {
                                    monto = monto.Replace('.', ',');
                                }

                                //elementoListaCuota4003.netAmount = Decimal.Parse(cells[rowIterator, filaNetAmountCuota-1 ].Value.ToString());
                                elementoListaCuota4003.netAmount = Decimal.Parse(monto);
                                elementoListaCuota4003.paymentDate = cells[rowIterator, filapaymentCoutaCuota - 1].Value.ToString();

                                listaCuotas4003Aux.Add(elementoListaCuota4003);
                                //listaCuotas4003.Add(elementoListaCuota4003);
                            }

                            listaCuotas40032 = listaCuotas4003Aux.ToArray();
                            //lista2Cuotas4003 = listaCuotas4003.ToArray();

                            //elementoListaFactura4003.paymentDetail = listaCuotas40032;
                            elementoListaFactura4003.paymentDetail = listaCuotas40032;

                        }


                        listaFacturas4003.Add(elementoListaFactura4003);


                    }

                }
            }



            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener los datos de las facturas y coutas desde la BD para crear la lista a enviar al ws final: " + ex.ToString());
                return listaFacturas4003;
            }

            //Eliminar el archivo y limpiar archivo 
            if (archivo.name != null && archivo.ruta != null)
            {
                //Elimino archivos del servidor
                if (System.IO.File.Exists(archivo.ruta + archivo.name))
                {
                    System.IO.File.Delete(archivo.ruta + archivo.name);
                }
            }
            archivo.name = null;
            archivo.ruta = null;
            archivo.nameOriginal = null;

            return listaFacturas4003;
        }




        //Enviar lista InvoiceInformationAdditional_Type al WS_04003, recibir mensaje respuesta y mostrarlo en pantalla
        public JsonResult enviarXml(FormCollection formCollection)
        {
            List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type> list_fact = new List<InterfazCavali.InputService04003.InvoiceInformationAdditional_Type>();
            InterfazCavali.InputService04003.InvoiceInformationAdditional_Type[] list_fact2;
            try
            {
                Stopwatch sw3 = new Stopwatch(); // Creación del Stopwatch.
                sw3.Start(); // Iniciar la medición.
                //Datos consumidor recibidos del formulario
                string participantCode = formCollection["participantCode"];
                string type = formCollection["type"];
                string ruc = formCollection["ruc"];


                //Usuario
                string usuario = "USUARIO_WEB";
                int cantidadArchivos = 1;
                string mensajeCliente = null;
                string mensajeClienteCode = null;

                //Datos cabecera
                cabecera.COD_SERVICIO = "04003";
                cabecera.APP_CONSUMIDORA = "WEB_CLI";

                //Datos consumidor
                consumidor.type = type;
                consumidor.participantCode = participantCode;
                consumidor.ruc = ruc;

                //Inicializar el web service WS_Dim_04003_Registrar_inf_adicional
                InterfazCavali.InputService04003.WS_Dim_04003_Registrar_inf_adicional wss_04003 = new InterfazCavali.InputService04003.WS_Dim_04003_Registrar_inf_adicional();
                wss_04003.Timeout = -1;
                //Iniciarlizar el mensaje de respuesta
                InterfazCavali.InputService04003.MENSAJERES_Type mensaje = new InterfazCavali.InputService04003.MENSAJERES_Type();


                //Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                //sw.Start(); // Iniciar la medición.

                //armamos la lista a enviar al WS_4003
                list_fact.Clear();
                list_fact2 = null;
                if (archivo.extension == "XLS" || archivo.extension == "xls")
                {
                    list_fact = Excel2003ToListWs4003();
                }
                else
                {
                    list_fact = ExcelToListWs4003version2();
                }
                //list_fact = ExcelToListWs4003();
                list_fact2 = list_fact.ToArray();
                //sw.Stop(); // Detener la medición.
                //Console.WriteLine("Time elapsed: {0}", sw.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000


                //Stopwatch sw3 = new Stopwatch(); // Creación del Stopwatch.
                //sw3.Start(); // Iniciar la medición.


                //Consumir el web service WS_Dim_04003_Registrar_inf_adicional y guardamos la respuesta en mensaje 
                mensaje = wss_04003.RegistrarInfoAdicional(cabecera, consumidor, list_fact2, usuario);
                sw3.Stop(); // Detener la medición.
                Console.WriteLine("Time elapsed: {0}", sw3.Elapsed.ToString("hh\\:mm\\:ss\\.fff")); // Mostrar el tiempo transcurriodo con un formato hh:mm:ss.000

                //Mensajes de respuesta al cliente
                //Si resultCode es 0, se muestra mensaje de exito y el Id de la transaccion

                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.resultCode == null && mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.message !=null)

                {
                    mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo!";
                    mensajeCliente += "\n  " + mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.message;
                    mensajeClienteCode = "1";

                }

                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.resultCode == "0")

                {
                    mensajeCliente = "Felicidades su Transacción se ha realizado correctamente!";
                    mensajeCliente += "\n Su codigo de Transacción es: " + mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.idProceso;
                    mensajeClienteCode = "0";

                }

                //Si resultCode es 1, se muestra mensaje de error  
                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.resultCode == "1")
                {
                    mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo!";
                    mensajeClienteCode = "1";

                }

                //Si resultCode es 99, se muestra mensaje de error  
                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceInformationResponse.addInvoiceInformationResponseDetail.resultCode == "99")
                {
                    mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo!";
                    mensajeClienteCode = "99";
                }
                //Se pasan los mensajes a la vista y la cantidad de archivos que tiene la lista
                return Json(new { mensajeClienteCode, mensajeCliente, cantidadArchivos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int cantidadArchivos = 0;
                string mensajeCliente = null;
                mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo! " + ex.Message;
                string mensajeClienteCode = null;
                rg.eLog("Error al enviar datos adiccionales de facturas al ws_04003: " + ex.ToString());
                return Json(new { mensajeClienteCode, mensajeCliente, cantidadArchivos }, JsonRequestBehavior.AllowGet);

            }
        }


        public static List<string> obtenerHoja(string excelFilePath)
        {
            List<string> sheets = new List<string>();
            using (OleDbConnection connection =
                    new OleDbConnection((excelFilePath.TrimEnd().ToLower().EndsWith("x"))
                    ? "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + excelFilePath + "';" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'"
                    : "provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + excelFilePath + "';Extended Properties=Excel 8.0;"))
            {
                connection.Open();
                DataTable dt = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow drSheet in dt.Rows)
                    if (drSheet["TABLE_NAME"].ToString().Contains("$"))
                    {
                        string s = drSheet["TABLE_NAME"].ToString();
                        sheets.Add(s.StartsWith("'") ? s.Substring(1, s.Length - 3) : s.Substring(0, s.Length - 1));
                    }
                connection.Close();
            }
            return sheets;
        }
        //Generar el archivo de errores
        public void GenerarArchivoErroresBD(DataSet ds_errores)
        {

            try
            {
                //recorremos los errores de validaciones recibidos desde la bd
                if (ds_errores != null)
                {
                    if (ds_errores.Tables[0].Rows.Count > 0)
                    {
                        //abremos el archivo errores y guardamos los errores recibbidos
                        string Directorio = Properties.Settings.Default.rutaErrores;
                        string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                        // Valida y crea la carpeta definida en el config
                        if (!(Directory.Exists(ruta)))
                        {
                            Directory.CreateDirectory(ruta);
                        }
                        string path = ruta;
                        string filePath = path + "/Errores.csv";
                        using (var textWriter = new StreamWriter(@filePath,  false, System.Text.Encoding.UTF8))
                        {
                            var csvWriter = new CsvWriter(textWriter);
                            csvWriter.Configuration.Delimiter = ";";
                            csvWriter.WriteField("Fila del Error");
                            csvWriter.WriteField("Observación");
                            csvWriter.NextRecord();

                            for (int cont = 0; cont < ds_errores.Tables[0].Rows.Count; cont++)
                            {

                                InterfazCavali.InputService04003.InvoiceCode_Type elementoListaFactura4003IdentificadorFactura = new InterfazCavali.InputService04003.InvoiceCode_Type();

                                csvWriter.WriteField("El registro que se encuentra en la línea " + ds_errores.Tables[0].Rows[cont]["idVALIDACIONES_EXCEL_4003_TRASPASO"].ToString() + " contiene el siguiente error.");
                                //csvWriter.WriteField(ds_errores.Tables[0].Rows[cont]["idVALIDACIONES_EXCEL_4003_TRASPASO"].ToString());
                                csvWriter.WriteField(ds_errores.Tables[0].Rows[cont]["MENSAJE"].ToString());

                                csvWriter.NextRecord();
                            }
                            textWriter.Close();

                        }
                    }
                }
            }


            catch (Exception ex)
            {
                listaFacturas4003 = null;
                rg.eLog("Error al obtener los errores de validacion desde la BD y crear el archivo errores.csv: " + ex.ToString());

            }


        }

        public bool EnviarMail(String[] A, String De, String Asunto, String Cuerpo, String[] files, String Servidor)
        {
            //     A[0] = "bambasten9@gmail.com";
            //     De = "isobarzo@dim.cl";
            //     Asunto = "test";
            //     Cuerpo = "dasdfdsfdfa";
            //     files[0] = null;

            //    // De="soporte.Confirming@dimension.cl";

            //Servidor = "192.168.0.37";
            int codigo = 0;

                     MailMessage msg = new MailMessage();
            String file = "";

            for (int i = 0; i <= A.Length - 1; i++)
            {
                if (A[i] == null)
                    break;

                msg.To.Add(new MailAddress(A[i]));
            }

            msg.From = new MailAddress(De);
            msg.Subject = Asunto;
            msg.Body = Cuerpo;
            msg.IsBodyHtml = true;

            if (files != null)
            {
                for (int i = 0; i <= files.Length - 1; i++)
                {
                    if (files[i] == null)
                        break;

                    file = files[i];
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(file);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
                    // Add the file attachment to this e-mail message.
                    msg.Attachments.Add(data);

                }
            }

            SmtpClient clienteSmtp = new SmtpClient(Servidor);

            try
            {
                clienteSmtp.Send(msg);
                codigo = 1;
                return true;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadLine();
                codigo = 99;
                //detalle = ex.Message.ToString();
                return false;
            }

        }


        public bool ValidarExcelVacio()
        {
            int contadorFaltanCamposFacturasExcelTotal = 0;
            int dimensionreal = 0;

            try
            {
                FileStream file = System.IO.File.OpenRead(archivo.ruta + archivo.name);
                if ((file != null))
                {

                    Stopwatch sw = new Stopwatch(); // Creación del Stopwatch.
                    sw.Start(); // Iniciar la medición.
                    byte[] fileBytes = new byte[file.Length];

                    //abrir el archivo xls y crear el archivo errores
                    var data = file.Read(fileBytes, 0, Convert.ToInt32(file.Length));
                    string Directorio = Properties.Settings.Default.rutaErrores;
                    string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                    // Valida y crea la carpeta definida en el config
                    if (!(Directory.Exists(ruta)))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    string path = ruta;
                    string filePath = path + "/Errores.csv";
                    //creo el archivo Errores.csv
                    using (var textWriter = new StreamWriter(@filePath, false, System.Text.Encoding.UTF8))

                    {
                        //cabecera del archivo errores
                        var csvWriter = new CsvWriter(textWriter);
                        csvWriter.Configuration.Delimiter = ";";
                        csvWriter.WriteField("Fila del Error");
                        csvWriter.WriteField("Observación");
                        csvWriter.NextRecord();
                        using (var package = new ExcelPackage(file))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            //recorres fila por fila del xls

                            dimensionreal = workSheet.Dimension.End.Row;

                            if (workSheet.Dimension.End.Row > 25000)
                            {
                                for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                                {
                                    dimensionreal = rowIterator;
                                    //si una fila completa no contiene datos, se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                    if (workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                            && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                            && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                    {
                                        dimensionreal = rowIterator - 1;
                                        break;
                                    }

                                }
                            }

                        


                            //for (int rowIterator = 1; rowIterator <= workSheet.Dimension.End.Row; rowIterator++)
                            for (int rowIterator = 1; rowIterator <= dimensionreal; rowIterator++)
                            {

                                //si viene sin cabecera o sin datos crear nuevo csv
                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en blanco con basura
                                if (rowIterator == 1 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                                //si una fila no contiene datos se considera que el excel termino, esto es para eliminar espacios en balnco con basura
                                if (rowIterator == 2 && workSheet.Cells[rowIterator, 1].Value == null && workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null && workSheet.Cells[rowIterator, 5].Value == null && workSheet.Cells[rowIterator, 6].Value == null && workSheet.Cells[rowIterator, 7].Value == null && workSheet.Cells[rowIterator, 8].Value == null
                                    && workSheet.Cells[rowIterator, 9].Value == null && workSheet.Cells[rowIterator, 10].Value == null && workSheet.Cells[rowIterator, 11].Value == null && workSheet.Cells[rowIterator, 12].Value == null && workSheet.Cells[rowIterator, 13].Value == null && workSheet.Cells[rowIterator, 14].Value == null && workSheet.Cells[rowIterator, 15].Value == null && workSheet.Cells[rowIterator, 16].Value == null
                                    && workSheet.Cells[rowIterator, 17].Value == null && workSheet.Cells[rowIterator, 18].Value == null && workSheet.Cells[rowIterator, 19].Value == null && workSheet.Cells[rowIterator, 20].Value == null && workSheet.Cells[rowIterator, 21].Value == null && workSheet.Cells[rowIterator, 22].Value == null && workSheet.Cells[rowIterator, 23].Value == null)
                                {
                                    file.Close();
                                    textWriter.Close();
                                    GenerarArchivoErroresVacio(rowIterator);
                                    contadorFaltanCamposFacturasExcelTotal = 1;
                                    return false;
                                }

                            }
                            if (contadorFaltanCamposFacturasExcelTotal == 0)
                            {
                                return true;
                            }
                        }
                    }
                }

                if(contadorFaltanCamposFacturasExcelTotal==0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                rg.eLog("Error a validar factura vacia: " + ex.ToString());
                return false;
            }
        }
                                private List<string> Contains(List<string> list1, List<string> list2)
        {
            List<string> result = new List<string>();

            result.AddRange(list1.Except(list2, StringComparer.OrdinalIgnoreCase));
            result.AddRange(list2.Except(list1, StringComparer.OrdinalIgnoreCase));

            return result;
        }


  

    }
}
