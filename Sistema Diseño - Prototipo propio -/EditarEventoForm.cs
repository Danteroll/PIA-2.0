using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class EditarEventoForm : Form
    {
        private readonly Evento _evento;

        private TextBox        txtNombre;
        private ComboBox       cmbTipo;
        private DateTimePicker dtpFecha;
        private Button         btnGuardar, btnCancelar;

        public EditarEventoForm(Evento ev)
        {
            _evento = ev;
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            Text            = "Editar Evento";
            Size            = new Size(430, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            // Header
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 55,
                BackColor = Color.FromArgb(18, 30, 58)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "✏️   Editar Evento",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            });

            // Campos
            var tlp = new TableLayoutPanel
            {
                ColumnCount     = 2,
                RowCount        = 3,
                Location        = new Point(24, 70),
                Size            = new Size(374, 162),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            for (int i = 0; i < 3; i++)
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            txtNombre = new TextBox
            {
                Dock   = DockStyle.Fill,
                Font   = new Font("Segoe UI", 10.5f),
                Margin = new Padding(3)
            };

            cmbTipo = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10.5f),
                Margin        = new Padding(3)
            };
            cmbTipo.Items.AddRange(new object[]
            {
                "Boda","Cumpleaños","Bautizo","Quinceañera",
                "Graduación","Aniversario","Corporativo","Otro"
            });

            dtpFecha = new DateTimePicker
            {
                Dock   = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font   = new Font("Segoe UI", 10.5f),
                Margin = new Padding(3)
            };

            AddRow(tlp, 0, "Nombre:", txtNombre);
            AddRow(tlp, 1, "Tipo:",   cmbTipo);
            AddRow(tlp, 2, "Fecha:",  dtpFecha);

            // Botones
            btnGuardar = MakeBtn("✔  Guardar",   Color.FromArgb(18, 30, 58),  new Point(158, 258));
            btnCancelar= MakeBtn("✖  Cancelar",  Color.FromArgb(190, 45, 45), new Point(296, 258));

            btnGuardar.Click  += BtnGuardar_Click;
            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            Controls.Add(pnlHeader);
            Controls.Add(tlp);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private static void AddRow(TableLayoutPanel tlp, int row,
            string labelText, Control ctrl)
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

        private static Button MakeBtn(string txt, Color col, Point loc)
        {
            var b = new Button
            {
                Text      = txt,
                Size      = new Size(130, 40),
                Location  = loc,
                BackColor = col,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void CargarDatos()
        {
            txtNombre.Text = _evento.Nombre;
            int idx = cmbTipo.Items.IndexOf(_evento.Tipo);
            cmbTipo.SelectedIndex = idx >= 0 ? idx : 0;
            dtpFecha.Value = _evento.Fecha;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del evento es requerido.",
                    "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            try
            {
                DatabaseManager.EditarEvento(
                    _evento.Id,
                    _evento.Nombre,
                    txtNombre.Text.Trim(),
                    cmbTipo.SelectedItem.ToString(),
                    dtpFecha.Value.Date);

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
