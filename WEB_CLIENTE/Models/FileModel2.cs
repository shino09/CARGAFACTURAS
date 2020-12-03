using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ComponentModel.DataAnnotations;
namespace WEB_CLIENTE.Models
{
    public class FileModel2
    {
       
            //public HttpPostedFileBase[] files { get; set; }

            //[Display(Name = "fileXml")]
            //[Required(ErrorMessage = "Debe seleccionar al menos 1 archivo Excel.")]
           // [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.xlsx|.xls|.XLSX|.XLS|.Xlsx|.Xls)$", ErrorMessage = "Solo se aceptan archivos Excel.")]
        //[RegularExpression(@"([a-zA-Z0-9\s_\$\\.\-:])+(.xlsx|.xls|.XLSX|.XLS|.Xlsx|.Xls)$", ErrorMessage = "Solo se aceptan archivos Excel.")]
     
        //[FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file")]

        public byte[] fileXml { get; set; }

            public String fileXmlBase64 { get; set; }
            public string name { get; set; }
            public string ruta { get; set; }

            //public string type { get; set; }

            //public string ruc { get; set; }

            //public string participantCode { get; set; }
         
            
            /*
            [Display(Name = "ruc")]
            [Required(ErrorMessage = "El campo Ruc es requerido.")]
            [RegularExpression("([0-9]+)", ErrorMessage = "El campo Ruc debe ser de tipo Numerico")]
            [StringLength(11, ErrorMessage = "El campo Ruc debe tener una longitud máxima de 11 caracteres")]*/

            public string ruc { get; set; }

        /*
            [Display(Name = "participantCode")]
            [Required(ErrorMessage = "El campo codigo del participante es requerido.")]

            [RegularExpression("([0-9]+)", ErrorMessage = "El campo codigo del participante debe ser de tipo Numerico")]
            [StringLength(9, ErrorMessage = "El campo codigo del participante debe tener una longitud máxima de 9 caracteres")]*/
            public string participantCode { get; set; }

        /*
            [Display(Name = "type")]
            [Required(ErrorMessage = "El campo type es requerido.")]*/
            public string type { get; set; }

            public int providerRuc { get; set; }
            public string series { get; set; }
            public int numeration { get; set; }
            public string authorizationNumber { get; set; }
            public string invoiceType { get; set; }
            public DateTime expirationDate { get; set; }
            public string department { get; set; }
            public string province { get; set; }
            public string district { get; set; }
            public string addressSupplier { get; set; }
            public string acqDepartment { get; set; }
            public string acqProvince { get; set; }
            public string acqDistrict { get; set; }
            public string addressAcquirer { get; set; }
            public int typePayment { get; set; }
            public int numberQuota { get; set; }
            public DateTime deliverDateAcq { get; set; }
            public DateTime aceptedDate { get; set; }
            public DateTime paymentDate { get; set; }
            public Decimal netAmount { get; set; }
            public string other1 { get; set; }
            public string other2 { get; set; }
            public string additionalField1 { get; set; }
            public string additionalField2 { get; set; }

    }
}