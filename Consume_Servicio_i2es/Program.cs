using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using System.Net;



//probando a ver si sube

namespace Consume_Servicio_i2es
{
    class Program
    {
        public static SqlConnection con;
        public static SqlCommand cmd;
        static void Main(string[] args)
        {
            

            try
            {
               
                IniFile ini = new IniFile("./Web_Xml.ini"); // archivo ini 
                string ConString = ini.IniReadValue("Parametros", "Base");
                using (con = new SqlConnection(ConString))
                {
                    con.Open();
                    Llamows("Estado_Pedidos");
                    Llamows("Clientes");
                    Llamows("Cupones");
                    Llamows("Cotizacion");
                    Llamows("Actualizacion_Articulos");
                    Llamows("Baja_Articulos");
                    Llamows("Actualizacion_Stock_Precio");
                    Llamows("Actualizacion_Stock_Unidad");
                }

            }
            catch (Exception e)
            {
                GrabarError("Error en Main " + e.Message + (char)13 + (char)10);
            }
        }

      
        static void Llamows(string Codigo_Tabla)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            SqlDataReader reader = null;
            try
            {
            IniFile ini = new IniFile("./Web_Xml.ini"); // archivo ini 
            string userWS = ini.IniReadValue("Parametros", "userWS");
            string passWS = ini.IniReadValue("Parametros", "passWS");
            if (Codigo_Tabla == "Estado_Pedidos")
            {// Vista Estado_Pedidos  ****vista_pedido_estado_articulos_web_xml***
                
                string tabla = ini.IniReadValue("Parametros", "Tabla_Estado_pedidos");
                string CmdString_EstadoPedidos = "Select ISNULL(xmlcolumn, '') FROM " + tabla;
                cmd = new SqlCommand(CmdString_EstadoPedidos, con);
                reader = cmd.ExecuteReader();
                string resultado_ws_EstadoPedidos = "";
                reader.Read();
                string xml_EstadoPedidos = reader.GetString(0);
                reader.Close();
                if (xml_EstadoPedidos.Trim() != "")
                {
                    Pedidos_Estado.ws_estados_pedidos wsConsumo_Estado = new Pedidos_Estado.ws_estados_pedidos();
                    var salida_EstdoPedidos = wsConsumo_Estado.Callws_estados_pedidos(userWS, passWS, xml_EstadoPedidos);
                    resultado_ws_EstadoPedidos = salida_EstdoPedidos.Error;
                    //GraboLog
                    GrabaLog(Codigo_Tabla, "wsConsumo_Estado", xml_EstadoPedidos, resultado_ws_EstadoPedidos);
                    if (resultado_ws_EstadoPedidos != "")
                    {
                        GrabarError("Error en llamows_Estados " + resultado_ws_EstadoPedidos + (char)13 + (char)10);
                    }
                    else
                        Actualiza_fecha_envio(xml_EstadoPedidos, "nro_pedido", "Pedido_Estado");

                }
            }
            if (Codigo_Tabla == "Clientes")
            { // Vista Cliente  ****vista_clientes_xml***
                string tabla = ini.IniReadValue("Parametros", "Tabla_Clientes");
                string CmdString_Clientes = "Select * FROM " + tabla;
                cmd = new SqlCommand(CmdString_Clientes, con);
                reader = cmd.ExecuteReader();
                reader.Read();
                string xml_Cliente = reader.GetString(0);
                reader.Close();
                if (xml_Cliente.Trim() != "")
                {
                    Clientes.ws_actualizacion_clientes wsConsumo_Cliente = new Clientes.ws_actualizacion_clientes();
                    var salida_Cliente = wsConsumo_Cliente.Callws_actualizacion_clientes(userWS, passWS, xml_Cliente);
                    string resultado_ws_Cliente = salida_Cliente.Error;
                    //GraboLog
                    GrabaLog(Codigo_Tabla, "wsConsumo_Cliente", xml_Cliente, resultado_ws_Cliente);
                    if (resultado_ws_Cliente != "")
                    {

                        GrabarError("Error en llamows_Clientes " + resultado_ws_Cliente + (char)13 + (char)10);
                    }
                    else
                        Actualiza_fecha_envio(xml_Cliente, "Cod_Cliente", "clientes_alu_acuerdos_entrega");
                }

            }

                if (Codigo_Tabla == "Cupones")
                { // Vista Cliente  ****vista_clientes_xml***
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Cupones");
                    string CmdString_Cupones = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString_Cupones, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Cupones = reader.GetString(0);
                    reader.Close();
                    if (xml_Cupones.Trim() != "")
                    {
                        Cupones.ws_actualizacion_cuponessorteo wsConsumo_Cupones = new Cupones.ws_actualizacion_cuponessorteo();
                        var salida_Cupones = wsConsumo_Cupones.Callws_actualizacion_cuponessorteo(userWS, passWS, xml_Cupones);
                        string resultado_ws_Cupones = salida_Cupones.Error;
                        //GraboLog
                        GrabaLog(Codigo_Tabla, "wsConsumo_Cupones", xml_Cupones, resultado_ws_Cupones);
                        if (resultado_ws_Cupones != "")
                        {

                            GrabarError("Error en llamows_Cupones " + resultado_ws_Cupones + (char)13 + (char)10);
                        }
                        else
                            Actualiza_fecha_envio(xml_Cupones, "cupones");
                    }

                }

               if (Codigo_Tabla == "Cotizacion")
                {//Vista Cotizacion ****vista_cotizacion_xml****
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Cotizacion");
                    string CmdString_Cotizacion = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString_Cotizacion, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Cotizacion = reader.GetString(0);
                    reader.Close();
                    if (xml_Cotizacion.Trim() != "" && xml_Cotizacion != "<Cotizaciones/>")
                    {
                        Cotizacion.ws_actualizacion_cotizacion ws_Actualizacion_Cotizacion = new Cotizacion.ws_actualizacion_cotizacion();
                        var salida_Cotizacion = ws_Actualizacion_Cotizacion.Callws_actualizacion_cotizacion(userWS, passWS, xml_Cotizacion);
                        string resultado_ws_Cotizacion = salida_Cotizacion.Error;
                        //GraboLog
                        GrabaLog(Codigo_Tabla, "ws_Actualizacion_Cotizacion", xml_Cotizacion, resultado_ws_Cotizacion);
                        if (resultado_ws_Cotizacion != "")
                        {
                            GrabarError("Error en llamows_Cotizacion " + resultado_ws_Cotizacion + (char)13 + (char)10);
                        }
                        else
                            Actualiza_fecha_envio(xml_Cotizacion, "cotizacion");
                    }
                }

                if (Codigo_Tabla == "Actualizacion_Articulos")
                { // Vista Actualizacion_Articulos  ****Actualizacion_Articulos_xml***
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Actualizacion_Articulos");
                    string CmdString_Actualizacion_Articulos = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString_Actualizacion_Articulos, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Actualizacion_Articulos = reader.GetString(0);
                    reader.Close();
                        if (xml_Actualizacion_Articulos != null && xml_Actualizacion_Articulos.Trim() != "")
                        {
                            Actualizacion_Articulos.ws_actualizacion_articulos ws_Actualizacion_Articulos = new Actualizacion_Articulos.ws_actualizacion_articulos();
                            var salida_Actualizacion_Articulos = ws_Actualizacion_Articulos.Callws_actualizacion_articulos(userWS, passWS, xml_Actualizacion_Articulos);
                            string resultado_ws_Actualizacion_Articulos = salida_Actualizacion_Articulos.Error;
                            //GraboLog
                            GrabaLog(Codigo_Tabla, "ws_Actualizacion_Articulos", xml_Actualizacion_Articulos, resultado_ws_Actualizacion_Articulos);
                            if (resultado_ws_Actualizacion_Articulos != "")
                            {

                                GrabarError("Error en llamows_Actualizacion_Articulos " + resultado_ws_Actualizacion_Articulos + (char)13 + (char)10);
                            }
                            else
                                Actualiza_fecha_envio(xml_Actualizacion_Articulos, "Codigo", "articulos");
                        }
                    }


                    

                    if (Codigo_Tabla == "Baja_Articulos")
                { // Vista Baja_Articulos  ****Baja_Articulos_xml***
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Baja_Articulos");
                    string CmdString_Baja_Articulos = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString_Baja_Articulos, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Baja_Articulos = reader.GetString(0);
                    reader.Close();
                    if (xml_Baja_Articulos.Trim() != "" && xml_Baja_Articulos != "<Articulos/>")
                    {
                        Baja_Articulos.ws_actualizacion_articulosbaja ws_Actualizacion_Articulosbaja = new Baja_Articulos.ws_actualizacion_articulosbaja();
                        var salida_Baja_Articulos = ws_Actualizacion_Articulosbaja.Callws_actualizacion_articulosbaja(userWS, passWS, xml_Baja_Articulos);
                        string resultado_ws_Baja_Articulos = salida_Baja_Articulos.Error;
                        //GraboLog
                        GrabaLog(Codigo_Tabla, "ws_Actualizacion_Articulosbaja", xml_Baja_Articulos, resultado_ws_Baja_Articulos);
                        if (resultado_ws_Baja_Articulos != "")
                        {

                            GrabarError("Error en llamows_Baja_Articulos " + resultado_ws_Baja_Articulos + (char)13 + (char)10);
                        }
                        else
                            Actualiza_fecha_envio(xml_Baja_Articulos, "baja_articulos");
                    }

                }

                if (Codigo_Tabla == "Actualizacion_Stock_Precio")
                { // Vista Actualizacion_Stock_Precio  ****Actualizacion_Stock_Precio***
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Actualizacion_Stock_Precio");
                    string CmdString__Actualizacion_Stock_Precio = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString__Actualizacion_Stock_Precio, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Actualizacion_Stock_Precio = reader.GetString(0);
                    reader.Close();
                    if (xml_Actualizacion_Stock_Precio != null && xml_Actualizacion_Stock_Precio.Trim() != "")
                    {
                        Actualizacion_Stock_Precio.ws_actualizacion_stockprecio ws_Actualizacion_Stockprecio = new Actualizacion_Stock_Precio.ws_actualizacion_stockprecio();
                        var salida_Actualizacion_Stock_Precio = ws_Actualizacion_Stockprecio.Callws_actualizacion_stockprecio(userWS, passWS, xml_Actualizacion_Stock_Precio);
                        string resultado_ws_Actualizacion_Stock_Precio = salida_Actualizacion_Stock_Precio.Error;
                        //GraboLog
                        GrabaLog(Codigo_Tabla, "ws_Actualizacion_Stockprecio", xml_Actualizacion_Stock_Precio, resultado_ws_Actualizacion_Stock_Precio);
                        if (resultado_ws_Actualizacion_Stock_Precio != "")
                        {

                            GrabarError("Error en llamows_Actualizacion_Stock_Precio " + resultado_ws_Actualizacion_Stock_Precio + (char)13 + (char)10);
                        }
                        else
                            Actualiza_fecha_envio(xml_Actualizacion_Stock_Precio, "Codigo", "articulo_stock_precio");
                    }

                }

                if (Codigo_Tabla == "Actualizacion_Stock_Unidad")
                { // Vista Actualizacion_Stock_Unidad  ****vista_stock_distrib_xml***
                    string tabla = ini.IniReadValue("Parametros", "Tabla_Actualizacion_Stock_Unidad");
                    string CmdString__Actualizacion_Stock_Unidad = "Select * FROM " + tabla;
                    cmd = new SqlCommand(CmdString__Actualizacion_Stock_Unidad, con);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    string xml_Actualizacion_Stock_Unidad = reader.GetString(0);
                    reader.Close();
                    if (xml_Actualizacion_Stock_Unidad.Trim() != "")
                    {
                        Actualizacion_stock_unidad.ws_actualizacion_stockunidad ws_Actualizacion_Stockunidad = new Actualizacion_stock_unidad.ws_actualizacion_stockunidad();
                        var salida_Actualizacion_Stock_Unidad = ws_Actualizacion_Stockunidad.Callws_actualizacion_stockunidad(userWS, passWS, xml_Actualizacion_Stock_Unidad);
                        string resultado_ws_Actualizacion_Stock_Unidad = salida_Actualizacion_Stock_Unidad.Error;
                        //GraboLog
                        GrabaLog(Codigo_Tabla, "ws_Actualizacion_Stockunidad", xml_Actualizacion_Stock_Unidad, resultado_ws_Actualizacion_Stock_Unidad);
                        if (resultado_ws_Actualizacion_Stock_Unidad != "")
                        {

                            GrabarError("Error en llamows_Actualizacion_Stock_Unidad " + resultado_ws_Actualizacion_Stock_Unidad + (char)13 + (char)10);
                        }
                        else
                            Actualiza_fecha_envio(xml_Actualizacion_Stock_Unidad, "Codigo", "stock_distrib");
                    }

                }

            }
            

            catch (Exception ex)
            {
                GrabarError("Error en llamoWS: " + ex.Message + (char)13 + (char)10);
                

            }
            finally
            {
                reader.Close();

            }
        }
        static void busco_articulos_nuevos()
        {
            try
            {
                string sql = "select codigo from [Nodum_Web].[dbo].articulos where fecha_mail is null";
                cmd = new SqlCommand(sql, con);
                SqlDataReader drSql = cmd.ExecuteReader();
                string sinimagen = "";
                while (drSql.Read())
                {
                    sinimagen = sinimagen + drSql["codigo"].ToString() + ";";
                }
                drSql.Close();
                if (sinimagen != "")
                {

                    // cargo los vacios
                    sql = "update [Nodum_Web].[dbo].articulos set fecha_mail=getdate()  where fecha_mail is null";
                    cmd = new SqlCommand(sql, con);

                    cmd.ExecuteNonQuery();
                    // mando mail

                    Mail m = new Mail();
                    m.envio("Falta levantar imagen a la Web", sinimagen);

                }

            }
            catch (Exception ex)
            {
                GrabarError("Error en busco_articulos_nuevos: " + ex.Message + (char)13 + (char)10);

            }
        }
        static void GrabaLog(string Codigo_Tabla, string nombrews, string xml, string resultado)
        {
            try
            {     //Envio TipoId C (Consumiendo WS)    
                string sql = " INSERT INTO Webservice_Log([WebService_LogTipo] ,[WebService_Log_CodId] ,[WebService_Log_NumId],[WebService_Logxml] ,[WebService_LogResultado],[WebService_Log_FechaRecibido]) values " +
                                       "('E','" + Codigo_Tabla + "','" + nombrews + "','" + xml + "','" + resultado + "',getdate())";
                cmd = new SqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            { GrabarError("Error en GrabaLog: " + ex.Message + (char)13 + (char)10);

            }
        }

        static void GrabarError(string TextoLog)
        {
            try
            {
                StreamWriter sw = new StreamWriter("./errorws.log", true);
                sw.Write((System.DateTime.Now).ToString() + ':' + TextoLog);
                sw.Flush();
                sw.Close();
                Mail m = new Mail();
                m.envio("Error en intercambio e-commerce ", TextoLog);

            }
            catch (Exception e)
            {
                string MensajeError;
                MensajeError = "El archivo  de log no pudo ser grabado [" + e.Message + "]";
            }

        }
        static void Actualiza_fecha_envio(string xml, string campo, string tabla)
        {
            try
            {
                XDocument xdc = XDocument.Parse(xml);
                var valores = xdc.Root.Descendants(campo).Select(e => e.Value).ToArray();
                var sql = string.Format("update " + tabla + " set fecha_envio=getdate() where " + campo + "  in ('{0}')", string.Join("', '", valores));
                cmd = new SqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                GrabarError("Error en Actualiza_fecha_envio: " + ex.Message + (char)13 + (char)10);

            }
        }

        static void Actualiza_fecha_envio(string xml, string tabla)
        {
            try
            {
                var sql = string.Format("update " + tabla + " set fecha_envio=getdate() where fecha_envio IS NULL");
                cmd = new SqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                GrabarError("Error en Actualiza_fecha_envio de cupones: " + ex.Message + (char)13 + (char)10);

            }
        }

    }


    public class IniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <param name="INIPath"></param>
        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section"></param>
        /// Section name
        /// <param name="Key"></param>
        /// Key Name
        /// <param name="Value"></param>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();

        }
    }
    public class Mail
    {
        public void envio(string textocabezal, string textobody)
        {
            // Falta levantar imagen a la Web
            System.Net.Mail.MailMessage correo = new System.Net.Mail.MailMessage();
            IniFile ini = new IniFile("./Web_Xml.ini"); // archivo ini               
            string correo1 = ini.IniReadValue("Parametros", "correo1");
            string correo2 = ini.IniReadValue("Parametros", "correo2");
            string correo3 = ini.IniReadValue("Parametros", "correo3");
            string correoimagen1 = ini.IniReadValue("Parametros", "correoimagen1");
            string correoimagen2 = ini.IniReadValue("Parametros", "correoimagen2");
            string correoimagen3 = ini.IniReadValue("Parametros", "correoimagen3");
            string correoimagen4 = ini.IniReadValue("Parametros", "correoimagen4");

            correo.From = new System.Net.Mail.MailAddress("ventasweb@aluminios.com");
            if (textocabezal == "Falta levantar imagen a la Web")
            {
                if (correoimagen1 != "")
                    correo.To.Add(correoimagen1);
                if (correoimagen2 != "")
                    correo.To.Add(correoimagen2);
                if (correoimagen3 != "")
                    correo.To.Add(correoimagen3);
                if (correoimagen4 != "")
                    correo.To.Add(correoimagen4);
            }
            else
            {

                if (correo1 != "")
                {
                    //    correo.To.Add(correo1);
                    foreach (string address in correo1.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        correo.To.Add(address);
                    }
                }

                if (correo2 != "")
                {
                    //   correo.To.Add(correo2);
                    foreach (string address in correo2.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        correo.To.Add(address);
                    }
                }
                if (correo3 != "")
                {
                    //correo.To.Add(correo3);
                    foreach (string address in correo3.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        correo.To.Add(address);
                    }
                }

            }
            correo.Subject = textocabezal;
            correo.Body = textobody;
            correo.IsBodyHtml = false;
            correo.Priority = System.Net.Mail.MailPriority.Normal;

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
            //smtp.Host = "10.25.1.216";
            // smtp.Host = "10.25.1.216";
            smtp.Host = ini.IniReadValue("Parametros", "host");
            smtp.Port = Convert.ToInt16(ini.IniReadValue("Parametros", "puerto"));
            smtp.EnableSsl = true;
            //Solo si necesita usuario y clave
            //smtp.Credentials = new System.Net.NetworkCredential("ScadaPrensa", "Prensa2012");
            try
            {
                smtp.Send(correo);
            }
            catch (Exception ex)
            {
                //Controlar el error
            }


        }
    }
}

