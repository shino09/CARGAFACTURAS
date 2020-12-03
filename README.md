Web cliente mvc c# , aplicativo encargado de subir facturas de tipo xml y xls a la base de datos, mediante el uso de web services.

Valida datos del usuario con js y ajax.
Consume web service para insertar facturas xml.
Da formato a archivo xls.
Convierte archivo xls a xlsx.
Inserta facturas xlsx, en bd utilizando OPENROWSET(Microsoft.ACE.OLEDB.12.0).
Valida  datos factuas xlsx con procedimientos almacenados, (nulos,tipo datos, largo, fechas,rut,etc), utilizando Web services.
Errores de validacion son mostrados en un archivo csv.
Elimina archivos al finalizar y tabla temporales.
Envia email al usuario si la carga se realiza correctamente o tiene errores.


Autoria: Ivan Sobarzo

PD: se elimino webconfig porque tenia informacion sensibles de acceso a base de datos.
