using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;




namespace Consume_Servicio_i2es
{
    class Program
    {
        public static SqlConnection con;
        public static SqlCommand cmd;
        static void Main(string[] args)
        {
           // try
            //{
                string ConString = "Data Source=10.25.1.237;Initial Catalog=Nodum_Web;Trusted_Connection=True;Connection Timeout=30;";//ini.IniReadValue("Parametros", "Base");

                

                    using (con = new SqlConnection(ConString))
                {
                    con.Open();
                
                    //Vista pedidos Estado
                    string CmdString_EstadoPedidos = "Select * FROM vista_pedido_estado_articulos_web_xml";//ini.IniReadValue("Parametros", "sql" + i.ToString().Trim()); //
                    cmd = new SqlCommand(CmdString_EstadoPedidos, con);
                     SqlDataReader reader = cmd.ExecuteReader();
                string resultado_ws_EstadoPedidos = "";
                reader.Read();
                     string xml_EstadoPedidos = reader.GetString(0);
                    if (xml_EstadoPedidos.Trim() != "")
                    {
                        Pedidos_Estado.ws_estados_pedidos wsConsumo_Estado = new Pedidos_Estado.ws_estados_pedidos();
                        var salida_EstdoPedidos = wsConsumo_Estado.Callws_estados_pedidos("WAluminios", "ALURG12677", xml_EstadoPedidos);
                    resultado_ws_EstadoPedidos = salida_EstdoPedidos.Error;
                    
                     }
                reader.Close();
                // Vista Cliente  ****vista_clientes_xml***
                    string CmdString_Clientes = "Select * FROM vista_clientes_xml";//ini.IniReadValue("Parametros", "sql" + i.ToString().Trim()); //
                    cmd = new SqlCommand(CmdString_Clientes, con);
                     SqlDataReader readerCliente = cmd.ExecuteReader();
              

                    readerCliente.Read();
                    string xml_Cliente = readerCliente.GetString(0);
                    if (xml_Cliente.Trim() != "")
                    {
                    Clientes.ws_actualizacion_clientes wsConsumo_Cliente = new Clientes.ws_actualizacion_clientes();
                    var salida_Cliente = wsConsumo_Cliente.Callws_actualizacion_clientes("WAluminios", "ALURG12677", xml_Cliente);
                    string resultado_ws_Cliente = salida_Cliente.Error;
                    }
                readerCliente.Close();
            

        }
            //}
            //catch
            //{

            //}

        }
    }
}

