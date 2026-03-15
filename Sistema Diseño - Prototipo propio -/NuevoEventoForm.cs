using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class NuevoEventoForm : Form
    {
        public Evento EventoCreado { get; private set; }

        private TextBox       txtNombre;
        private ComboBox      cmbTipo;
        private DateTimePicker dtpFecha;
        private Button         btnGuardar, btnCancelar;

        public NuevoEventoForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text            = "Nuevo Evento";
            Size            = new Size(430, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            // Header azul
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 55,
                BackColor = Color.FromArgb(18, 30, 58)
            };
            var lblTitulo = new Label
            {
                Text      = "🎉   Crear Nuevo Evento",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            };
            pnlHeader.Controls.Add(lblTitulo);

            // Campos
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount    = 3,
                Location    = new Point(24, 70),
                Size        = new Size(374, 162),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++)
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            txtNombre = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5f),
                Margin = new Padding(3)
            };

            cmbTipo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f),
                Margin = new Padding(3)
            };
            cmbTipo.Items.AddRange(new object[]
            {
                "Boda", "Cumpleaños", "Bautizo", "Quinceañera",
                "Graduación", "Aniversario", "Corporativo", "Otro"
            });
            cmbTipo.SelectedIndex = 0;

            dtpFecha = new DateTimePicker
            {
                Dock   = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font   = new Font("Segoe UI", 10.5f),
                Value  = DateTime.Today.AddDays(30),
                Margin = new Padding(3)
            };

            // Añadir filas
            AddRow(tlp, 0, "Nombre:",  txtNombre);
            AddRow(tlp, 1, "Tipo:",    cmbTipo);
            AddRow(tlp, 2, "Fecha:",   dtpFecha);

            // Botones
            btnGuardar = new Button
            {
                Text      = "✔  Guardar",
                Size      = new Size(130, 40),
                Location  = new Point(158, 248),
                BackColor = Color.FromArgb(18, 30, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button
            {
                Text      = "✖  Cancelar",
                Size      = new Size(130, 40),
                Location  = new Point(296, 248),
                BackColor = Color.FromArgb(190, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            Controls.Add(pnlHeader);
            Controls.Add(tlp);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void AddRow(TableLayoutPanel tlp, int row, string labelText, Control ctrl)
        {
            tlp.Controls.Add(new Label
            {
                Text      = labelText,
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 80)
            }, 0, row);
            tlp.Controls.Add(ctrl, 1, row);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor ingresa el nombre del evento.",
                    "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            try
            {
                EventoCreado = new Evento
                {
                    Nombre = txtNombre.Text.Trim(),
                    Tipo   = cmbTipo.SelectedItem.ToString(),
                    Fecha  = dtpFecha.Value.Date
                };
                DatabaseManager.CrearEvento(EventoCreado);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el evento:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
