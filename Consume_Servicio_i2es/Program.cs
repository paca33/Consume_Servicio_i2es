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
                    string CmdString = "Select * FROM vista_pedido_estado_articulos_web_xml";//ini.IniReadValue("Parametros", "sql" + i.ToString().Trim()); //
                    cmd = new SqlCommand(CmdString, con);
                     SqlDataReader reader = cmd.ExecuteReader();

                    reader.Read();
                    string xml = reader.GetString(0);
                    Pedidos_Estado.ws_estados_pedidos wsConsumo_Estado = new Pedidos_Estado.ws_estados_pedidos();


               
              var pp=   wsConsumo_Estado.Callws_estados_pedidos("WAluminios", "ALURG12677", xml);



                }
            //}
            //catch
            //{

            //}
            
        }
    }
}

