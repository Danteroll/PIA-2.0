using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class NuevoPlatilloForm : Form
    {
        private string _eventoNombre;
        private string _tipoPlatillo;
        private TextBox txtPlatillo, txtIngrediente;
        private ListBox lstIngredientes;
        private Button btnAgregarIngrediente, btnGuardarTodo;

       
        public NuevoPlatilloForm(string eventoNombre, string tipoPlatillo)
        {
            _eventoNombre = eventoNombre;
            _tipoPlatillo = tipoPlatillo;
            
            Text = $"Nuevo Catálogo: {tipoPlatillo}";
            Size = new Size(350, 450);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var lbl1 = new Label { Text = $"1. Nombre de {tipoPlatillo}:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtPlatillo = new TextBox { Location = new Point(20, 45), Width = 290 };

            var lbl2 = new Label { Text = "2. Agregar Ingredientes (Opcional):", Location = new Point(20, 80), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtIngrediente = new TextBox { Location = new Point(20, 105), Width = 200 };
            
            btnAgregarIngrediente = new Button { Text = "Añadir a lista", Location = new Point(230, 104), Width = 80, BackColor = Color.LightGray };
            btnAgregarIngrediente.Click += (s, e) => {
                if (!string.IsNullOrWhiteSpace(txtIngrediente.Text)) {
                    lstIngredientes.Items.Add(txtIngrediente.Text.Trim());
                    txtIngrediente.Clear();
                }
            };

            lstIngredientes = new ListBox { Location = new Point(20, 140), Size = new Size(290, 180) };

            btnGuardarTodo = new Button { Text = "💾 Guardar Platillo", Location = new Point(20, 340), Size = new Size(290, 40), BackColor = Color.FromArgb(18, 118, 55), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnGuardarTodo.Click += BtnGuardarTodo_Click;

            Controls.AddRange(new Control[] { lbl1, txtPlatillo, lbl2, txtIngrediente, btnAgregarIngrediente, lstIngredientes, btnGuardarTodo });
        }

        private void BtnGuardarTodo_Click(object sender, EventArgs e)
        {
            string platillo = txtPlatillo.Text.Trim();
            if (string.IsNullOrEmpty(platillo)) {
                MessageBox.Show("Escribe el nombre del platillo.");
                return;
            }


            DatabaseManager.CrearPlatillo(_eventoNombre, platillo, _tipoPlatillo);


            foreach (string ingNombre in lstIngredientes.Items)
            {
                int idIngrediente = DatabaseManager.AgregarIngrediente(_eventoNombre, ingNombre);
                DatabaseManager.AsignarIngredienteAPlatillo(_eventoNombre, platillo, idIngrediente, _tipoPlatillo);
            }

            MessageBox.Show("¡Platillo guardado con éxito!");
            this.Close();
        }
    }
}