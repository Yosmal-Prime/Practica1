using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.MonthCalendar;


namespace ListadeTareas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

       

        ConexionDB db = new ConexionDB();



        private void Form1_Load(object sender, EventArgs e)
        {
            Mostrar();
        }

        private void Mostrar()
        {


            db.Open();
            SqlConnection conexion = db.obtenerConexion();

            string consulta = "SELECT * FROM Tarea";

            SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            foreach (DataRow row in dataTable.Rows)
            {
                if (row["Estado"].ToString() == "Completado")
                {
                    row["Estado"] = "Completado"; 
                }
                else
                {
                    row["Estado"] = "Pendiente"; 
                }
            }

            datagri.DataSource = dataTable;

            db.Close();

        }
        private void datagri_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == datagri.Columns["Estado"].Index && e.RowIndex >= 0)
            {
                string estado = e.Value.ToString();
                if (estado == "Completado")
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                }
                else if (estado == "Pendiente")
                {
                    e.CellStyle.BackColor = Color.Orange;
                }
            }
        }




        private void btnAgregar_Click(object sender, EventArgs e)
        {

            if (!CamposValidos())
            {
                return; 
            }

            db.Open();
            SqlConnection conexion = db.obtenerConexion();

            // Consulta SQL para insertar la tarea
            string consulta = "INSERT INTO Tarea (Asignatura, Titulo, Descripcion, FechaEntrega, Estado) VALUES (@Asignatura, @Titulo, @Descripcion, @FechaEntrega, @Estado)";

                SqlCommand command = new SqlCommand(consulta, conexion);
                command.Parameters.AddWithValue("@Asignatura", txtAsignatura.Text); 
                command.Parameters.AddWithValue("@Titulo", txtTarea.Text);
                command.Parameters.AddWithValue("@Descripcion", txtDescripcion.Text);
                command.Parameters.AddWithValue("@FechaEntrega", Convert.ToDateTime(date.Text));

         
            
            string estado = radioPendiente.Checked ? "Pendiente" : "Completado";
                command.Parameters.AddWithValue("@Estado", estado);

                command.ExecuteNonQuery();

                MessageBox.Show("Se agrego la tarea correctamente ", "Guardado Correcto", MessageBoxButtons.OK);
            db.Close();
            Mostrar();

        }

        private bool CamposValidos()
        {
            // Verifica si los campos están vacíos
            if (string.IsNullOrEmpty(txtAsignatura.Text) ||
                string.IsNullOrEmpty(txtTarea.Text) ||             
                string.IsNullOrEmpty(date.Text) ||
                !radioPendiente.Checked &&
                !radioRealizada.Checked) 
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; 
            }

            return true; 
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // Primero vemos si hay selepcion
            if (datagri.SelectedRows.Count > 0)
            {
                
                DialogResult resultado = MessageBox.Show("¿Estás seguro de que quieres eliminar las tareas seleccionadas?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultado == DialogResult.Yes)
                {

                    db.Open();
                    SqlConnection conexion = db.obtenerConexion();
                    
                      
                        foreach (DataGridViewRow fila in datagri.SelectedRows)
                        {
                            int idTarea = Convert.ToInt32(fila.Cells["ID"].Value);

                            string consulta = "DELETE FROM Tarea WHERE ID = @ID";
                            SqlCommand command = new SqlCommand(consulta, conexion);
                            command.Parameters.AddWithValue("@ID", idTarea);
                            command.ExecuteNonQuery();

                            datagri.Rows.Remove(fila);
                        }

                       // MessageBox.Show("Se eliminaron las tareas seleccionadas correctamente.", "Eliminación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    db.Close();

                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione al menos una tarea para eliminar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        bool cargandoDatos = false; 


        private void btnModificar_Click(object sender, EventArgs e)
        {


            if (!cargandoDatos) 
            {
                if (datagri.SelectedRows.Count == 1) 
                {
                    cargandoDatos = true; // Cambiar el estado a cargando datos

                    DataGridViewRow filaSeleccionada = datagri.SelectedRows[0];

                    string asignatura = filaSeleccionada.Cells["Asignatura"].Value.ToString();
                    string titulo = filaSeleccionada.Cells["Titulo"].Value.ToString();
                    string descripcion = filaSeleccionada.Cells["Descripcion"].Value.ToString();
                    DateTime fechaEntrega = Convert.ToDateTime(filaSeleccionada.Cells["FechaEntrega"].Value);
                    string estado = filaSeleccionada.Cells["Estado"].Value.ToString();

                    
                    txtAsignatura.Text = asignatura;
                    txtTarea.Text = titulo;
                    txtDescripcion.Text = descripcion;
                    date.Value = fechaEntrega;
                    if (estado == "Pendiente")
                        radioPendiente.Checked = true;
                    else
                        radioRealizada.Checked = true;

                    btnModificar.Text = "Guardar"; 
                    btnModificar.BackColor = Color.Green;
                }
                else if (datagri.SelectedRows.Count > 1)
                {
                    MessageBox.Show("Por favor, seleccione solo una tarea para modificar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Por favor, seleccione una tarea para modificar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else 
            {
                
                db.Open();
                SqlConnection conexion = db.obtenerConexion();
                
             
                    string asignatura = txtAsignatura.Text;
                    string titulo = txtTarea.Text;
                    string descripcion = txtDescripcion.Text;
                    DateTime fechaEntrega = date.Value;
                    string estado = radioPendiente.Checked ? "Pendiente" : "Completado";

                    int idTarea = Convert.ToInt32(datagri.SelectedRows[0].Cells["ID"].Value);

                    // Consulta SQL para actualizar los datos en la base de datos
                    string consulta = "UPDATE Tarea SET Asignatura = @Asignatura, Titulo = @Titulo, Descripcion = @Descripcion, FechaEntrega = @FechaEntrega, Estado = @Estado WHERE ID = @ID";
                    SqlCommand command = new SqlCommand(consulta, conexion);
                    command.Parameters.AddWithValue("@Asignatura", asignatura);
                    command.Parameters.AddWithValue("@Titulo", titulo);
                    command.Parameters.AddWithValue("@Descripcion", descripcion);
                    command.Parameters.AddWithValue("@FechaEntrega", fechaEntrega);
                    command.Parameters.AddWithValue("@Estado", estado);
                    command.Parameters.AddWithValue("@ID", idTarea);
                    command.ExecuteNonQuery();

                    //MessageBox.Show("Los cambios se guardaron correctamente.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                db.Close();

                Mostrar();
                cargandoDatos = false; 
                btnModificar.Text = "Modificar"; 
                btnModificar.BackColor = Color.DodgerBlue;
            }
          
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            btnCancelar.BackColor = Color.Red;
            btnCancelar.Text = "Cancelar";
            Mostrar();
            txtAsignatura.Text = "";
            txtTarea.Text = "";
            txtDescripcion.Text = "";
            radioPendiente.Checked = false;
            radioRealizada.Checked = false;
         
        }

        private void btnPendientes_Click(object sender, EventArgs e)
        {
            db.Open();
            SqlConnection conexion = db.obtenerConexion();
            string consulta = "SELECT * FROM Tarea WHERE Estado = 'Pendiente'";

            btnCancelar.Text = "Volver";
            btnCancelar.BackColor = Color.Green;
            SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            datagri.DataSource = null;
            datagri.Rows.Clear();

            datagri.DataSource = tabla;
            db.Close();
            
        }

        private void btnRealizadas_Click(object sender, EventArgs e)
        {
            btnCancelar.Text = "Volver";
            btnCancelar.BackColor = Color.Green;
            db.Open();
            SqlConnection conexion = db.obtenerConexion();
            string consulta = "SELECT * FROM Tarea WHERE Estado = 'Completado'";


            SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            datagri.DataSource = null;
            datagri.Rows.Clear();

            datagri.DataSource = tabla;

            db.Close();
        }



        private void colorDeFondoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = colorDialog.Color;
            }
        }

        private void fuentesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                this.Font = fontDialog.Font;
            }
        }

        private void colorDeLetrasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                this.ForeColor = colorDialog.Color;
            }
        }

        private void datagri_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
