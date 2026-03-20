using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class EliminarPlatilloForm : Form
    {
        private string _eventoNombre;
        private string _tipoPlatillo;
        private ListBox lstPlatillos;
        private Button btnEliminar;

        public EliminarPlatilloForm(string eventoNombre, string tipoPlatillo)
        {
            _eventoNombre = eventoNombre;
            _tipoPlatillo = tipoPlatillo;

            Text = $"Gestor de Eliminación: {_tipoPlatillo}";
            Size = new Size(350, 400);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var lbl = new Label { Text = $"Selecciona el platillo ({_tipoPlatillo}) a eliminar:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            lstPlatillos = new ListBox { Location = new Point(20, 50), Size = new Size(290, 220), Font = new Font("Segoe UI", 10) };
            CargarLista();

            btnEliminar = new Button { Text = "🗑️ Eliminar Platillo Seleccionado", Location = new Point(20, 290), Size = new Size(290, 40), BackColor = Color.FromArgb(185, 40, 40), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Click += BtnEliminar_Click;

            Controls.AddRange(new Control[] { lbl, lstPlatillos, btnEliminar });
        }

        private void CargarLista()
        {
            lstPlatillos.Items.Clear();
            var catalogo = DatabaseManager.ObtenerCatalogoPlatillos(_eventoNombre, _tipoPlatillo);
            foreach (var item in catalogo)
            {
              
                if (!item.StartsWith("—")) 
                    lstPlatillos.Items.Add(item);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (lstPlatillos.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un platillo de la lista primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string txt = lstPlatillos.SelectedItem.ToString();

            string nombreLimpio = txt.IndexOf(" (") > 0 ? txt.Substring(0, txt.IndexOf(" (")) : txt;

            if (MessageBox.Show($"¿Estás seguro de que deseas eliminar '{nombreLimpio}' definitivamente?\n\nEsto también lo quitará del menú del evento.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DatabaseManager.EliminarPlatillo(_eventoNombre, nombreLimpio);
                CargarLista(); 
                MessageBox.Show("Platillo eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}