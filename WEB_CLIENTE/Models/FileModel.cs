using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace WEB_CLIENTE.Models
{
    public class FileModel
    {
        //public HttpPostedFileBase[] files { get; set; }


        /*[Display(Name = "fileXml")]
        [Required(ErrorMessage = "Debe seleccionar al menos 1 archivo Xml.")]
        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.xml)$", ErrorMessage = "Solo se aceptan archivos Xml.")]*/
        public byte[] fileXml { get; set; }

        public String fileXmlBase64 { get; set; }
        public string name { get; set; }
        public string ruta { get; set; }
        public string additionalField1 { get; set; }
        public string additionalField2 { get; set; }

        /*
        [Display(Name = "ruc")]
        [Required(ErrorMessage = "El campo Ruc es requerido.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El campo Ruc debe ser de tipo Numerico")]
        //[StringLength(11, ErrorMessage = "El campo Ruc debe tener una longitud máxima de 11 caracteres")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El campo Ruc debe tener una longitud exacta de 11 caracteres")]*/

        public string ruc { get; set; }

        /*[Display(Name = "participantCode")]
        [Required(ErrorMessage = "El campo codigo del participante es requerido.")]

        [RegularExpression("([0-9]+)", ErrorMessage = "El campo codigo del participante debe ser de tipo Numerico")]
        [StringLength(9, ErrorMessage = "El campo codigo del participante debe tener una longitud máxima de 9 caracteres")]*/
        public string participantCode { get; set; }

        /*[Display(Name = "type")]
        [Required(ErrorMessage = "El campo type es requerido.")]*/
        //[StringLength(1, ErrorMessage = "El campo type debe tener una longitud máxima de 1 caracter")]
        public string type { get; set; }

    



    }
}