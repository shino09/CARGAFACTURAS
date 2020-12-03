using System;
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
//using ClaseRutinas;
using dim.rutinas;
namespace WEB_CLIENTE.Controllers
{
    
    public class CargarFacturasController : Controller
    {
        RutinasGenerales rg = new RutinasGenerales();

        //datos consumidor 
        public static string type = null;
        public static string participantCode = null;
        public static string ruc = null;
        public static string processNumber = "0";
        public static string usuario = "USUARIO_WEB";

        
        //cabecera, consumidor,proceso y lista de archivos del WS_04002
        public static InterfazCavali.InputService04002.CABECERA_Type cabecera = new InterfazCavali.InputService04002.CABECERA_Type();
        public static InterfazCavali.InputService04002.Consumer_Type consumidor = new InterfazCavali.InputService04002.Consumer_Type();
        public static InterfazCavali.InputService04002.ProcessDetail_Type proceso = new InterfazCavali.InputService04002.ProcessDetail_Type();
        public static List<InterfazCavali.InputService04002.InvoiceXML_Type> listaArchivos = new List<InterfazCavali.InputService04002.InvoiceXML_Type>();
        public static InterfazCavali.InputService04002.InvoiceXML_Type[] listaArchivos1;
        public ActionResult Index()

        {
            Session["listaArchivos"] = listaArchivos;
            Session["listaArchivos1"] = listaArchivos1;
  
            WS_GET_TYPE.WS_TEST ws_get_type = new WS_GET_TYPE.WS_TEST();
            WS_GET_TYPE.Parametro[] lista;
            lista = ws_get_type.GetTipoParticipante();

            return View();
        }

        [HttpPost]
        //Enviar Informacion adicional al WS_4002
        public JsonResult enviarXml(FormCollection formCollection)
        {
            //Mensajes de respuesta al cliente
            string mensajeCliente = null;
            string mensajeClienteCode = null;
            int cantidadArchivos = 0;

            /*mensaje de ingreso*/
            Console.Write("Ingresamos al enviarXml");
            try
            {
                //Datos consumidor recibidos deL FORMULARIO
                string participantCode = formCollection["participantCode"];
                string type = formCollection["type"];
                string ruc = formCollection["ruc"];

                //Procceso
                string processNumber = "0";
                proceso.processNumber = processNumber;

                //Usuario
                string usuario = "USUARIO_WEB";

                cantidadArchivos = listaArchivos.Count;

                //Datos cabecera
                cabecera.COD_SERVICIO = "04002";
                cabecera.APP_CONSUMIDORA = "WEB_CLI";

                //Datos consumidor
                consumidor.type = type;
                consumidor.participantCode = participantCode;
                consumidor.ruc = ruc;

                //Inicializar el web service WS_Dim_04002_Registrar_facturas_XML
                InterfazCavali.InputService04002.WS_Dim_04002_Registrar_facturas_XML wss_04002 = new InterfazCavali.InputService04002.WS_Dim_04002_Registrar_facturas_XML();

                //Iniciarlizar el mensaje de respuesta
                InterfazCavali.InputService04002.MENSAJERES_Type mensaje = new InterfazCavali.InputService04002.MENSAJERES_Type();
                mensaje = null;

                //Consumir el web service WS_Dim_04002_Registrar_facturas_XML y guardar la respuesta en mensaje 
                mensaje = wss_04002.RegistrarFacturasXml(cabecera, consumidor, proceso, listaArchivos1, usuario);


                //Si resultCode es 0, se muestra mensaje de exito y el Id de la transaccion
                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceXMLResponse.addInvoiceXMLResponseDetail.resultCode == "0")
                {
                    mensajeCliente = "Felicidades su Transacción se ha realizado correctamente!";
                    mensajeCliente += "\n Su codigo de Transacción es: " + mensaje.INTEGRES.DETALLE.DATOS.addInvoiceXMLResponse.addInvoiceXMLResponseDetail.idProceso;
                    mensajeClienteCode = "0";

                }

                //Si resultCode es 1, se muestra mensaje de error  
                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceXMLResponse.addInvoiceXMLResponseDetail.resultCode == "1")
                {
                    mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo!";
                    mensajeClienteCode = "1";

                }

                //Si resultCode es 99, se muestra mensaje de error  
                if (mensaje.INTEGRES.DETALLE.DATOS.addInvoiceXMLResponse.addInvoiceXMLResponseDetail.resultCode == "99")
                {
                    mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo!";
                    mensajeClienteCode = "99";
                }

                //Se pasan los mensajes a la vista y la cantidad de archivos que tiene la lista
                return Json(new { mensajeClienteCode, mensajeCliente, cantidadArchivos }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                /*int cantidadArchivos = 0;
                string mensajeCliente = null;
                string mensajeClienteCode = null;
                rg.eLog("Error al enviar datos adiccionales de facturas al ws_04002: " + ex.ToString());
                return Json(new { mensajeClienteCode, mensajeCliente, cantidadArchivos }, JsonRequestBehavior.AllowGet);*/
				
				mensajeCliente = "!Ha ocurrido un error al realizar su transacción, por favor intente de nuevo! " + ex.Message;
                mensajeClienteCode = "99";
                Console.Write(ex.Message);               

                return Json(new { mensajeClienteCode, mensajeCliente, cantidadArchivos }, JsonRequestBehavior.AllowGet);

				

            }
        }

        //Subir el archivo Xlsx al servidor
        public JsonResult UploadFilesAjax()
        {
            try
            {
                //Limpiar lista
                //listaArchivos.Clear();

                //agregamos una ruta para la creación guardar los archivos
                string Directorio = Properties.Settings.Default.rutaXml;
                string ruta = System.AppDomain.CurrentDomain.BaseDirectory + Directorio + "/";

                // Valida y crea la carpeta definida en el config
                if (!(Directory.Exists(ruta)))
                {
                    Directory.CreateDirectory(ruta);
                }

                //Subir los archivos
                string path = ruta;//Server.MapPath("~/UploadedFiles/");
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    //Recivir archivos uno por uno y guardarlos en la ruta con un nombre unico (ej:1709201991611_BDTABLAS.png)
                    HttpPostedFileBase file = files[i];
                    string nombre_archivo = DateTime.Now.ToString().Replace("/", "") + "_" + file.FileName;
                    nombre_archivo = nombre_archivo.Replace(":", "");
                    nombre_archivo = nombre_archivo.Replace(" ", "");
                    file.SaveAs(path + nombre_archivo);
                    //Anadir archivo a la lista como  byte
                    InterfazCavali.InputService04002.InvoiceXML_Type archivo = new InterfazCavali.InputService04002.InvoiceXML_Type();
                    FileStream inStream = System.IO.File.OpenRead(path + nombre_archivo);
                    byte[] xmlByte = new byte[inStream.Length];
                    inStream.Read(xmlByte, 0, Convert.ToInt32(inStream.Length.ToString()));
                    //archivo.fileXml = System.IO.File.ReadAllBytes(path + nombre_archivo);
                    //string fileBase64 = Convert.ToBase64String(archivo.fileXml);
                    archivo.fileXml = xmlByte;
                    archivo.name = nombre_archivo;
                    archivo.additionalField1 = "";
                    archivo.additionalField2 = "";

                    listaArchivos.Add(archivo);
                    listaArchivos1 = listaArchivos.ToArray();
                    inStream.Close();
                }

                string pathEliminar = ruta;

                //Eliminar los archivos de la carpeta del servidor, ya una vez convertidos a bytes y almacenados en la lista
                if (listaArchivos1 != null && listaArchivos1.Length > 0)
                {
                    for (int i = 0; i < listaArchivos1.Length; i++)
                    {

                        //Elimino archivos del servidor
                        string nombre_archivo_eliminar = listaArchivos1[i].name;
                        if (System.IO.File.Exists(pathEliminar + nombre_archivo_eliminar))
                        {
                            System.IO.File.Delete(pathEliminar + nombre_archivo_eliminar);
                        }
                    }
                }

                /*FileStream inStream = file.OpenRead(file.FileName);
                byte[] xmlByte = new byte[inStream.Length];
                inStream.Read(xmlByte, 0, Convert.ToInt32(inStream.Length.ToString()));*/

                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                rg.eLog("Error al subir el archivo: " + ex.ToString());
                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);
                //ws_carga_info_adic.VaciarTablas();
            }
        }

        //Eliminar archivo de la lista y el servidor 
        public JsonResult Delete(int index)
        {
            try
            {
                //Eliminar el archivo de la lista y devolver la nueva lista 
                listaArchivos.RemoveAt(index);
                listaArchivos1 = listaArchivos.ToArray();

                //elemento.Delete(file);
                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                rg.eLog("Error al eliminar el archivo: " + ex.ToString());
                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);

            }
        }

        //Obtener datos del tipo de participante para mostrar en select
        public ActionResult getType()
        {
            try {
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

        //funcion para limpiar la lista al refrescar la pgina
        public JsonResult LimpiarListaAjax()
        {
            try
            {
                //Limpiar lista
                listaArchivos.Clear();
                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                rg.eLog("Error al limpiar lista: " + ex.ToString());
                return Json(listaArchivos.ToList(), JsonRequestBehavior.AllowGet);
                //ws_carga_info_adic.VaciarTablas();
            }
        }
    }
   

}