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
                }

            }
            catch (Exception e)
            {
                GrabarError("Error en Main " + e.Message + (char)13 + (char)10);
            }
        }


        static void Llamows(string Codigo_Tabla)
        {
            try
            {
            IniFile ini = new IniFile("./Web_Xml.ini"); // archivo ini 
            string userWS = ini.IniReadValue("Parametros", "userWS");
            string passWS = ini.IniReadValue("Parametros", "passWS");
            if (Codigo_Tabla == "Estado_Pedidos")
            {// Vista Estado_Pedidos  ****vista_pedido_estado_articulos_web_xml***
                
                string tabla = ini.IniReadValue("Parametros", "Tabla_Estado_pedidos");
                string CmdString_EstadoPedidos = "Select * FROM " + tabla;
                cmd = new SqlCommand(CmdString_EstadoPedidos, con);
                SqlDataReader reader = cmd.ExecuteReader();
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
                SqlDataReader readerCliente = cmd.ExecuteReader();
                readerCliente.Read();
                string xml_Cliente = readerCliente.GetString(0);
                readerCliente.Close();
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
            }
            catch (Exception ex)
            {
                GrabarError("Error en llamoWS: " + ex.Message + (char)13 + (char)10);
                

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

