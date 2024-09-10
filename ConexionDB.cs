using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ListadeTareas
{
    internal class ConexionDB
    {
        private SqlConnection conexion = new SqlConnection("server = DESKTOP-TLDB5S0; database=ListaTareas; integrated security=true");

        
      
        public SqlConnection obtenerConexion()
        {
            return conexion;
        }

        public void Open()
        {
            try
            {
                conexion.Open();
               // MessageBox.Show("¡Conexión exitosa a la base de datos!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al intentar conectar a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close()
        {
            conexion.Close();
        }
    }
}
