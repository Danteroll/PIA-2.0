using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class NuevoEventoForm : Form
    {
        // nullable: solo se asigna si el usuario guarda
        public Evento? EventoCreado { get; private set; }

        private TextBox        txtNombre  = null!;
        private ComboBox       cmbTipo    = null!;
        private DateTimePicker dtpFecha   = null!;
        private Button         btnGuardar = null!;
        private Button         btnCancelar= null!;

        public NuevoEventoForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text            = "Nuevo Evento";
            // FIX: tamaño mínimo garantizado para que nunca se recorten controles
            Size            = new Size(430, 340);
            MinimumSize     = new Size(380, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Header ────────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 55,
                BackColor = Color.FromArgb(18, 30, 58)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "🎉   Crear Nuevo Evento",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            });

            // ── Área de contenido: Fill para adaptarse al tamaño del form ────
            // FIX: reemplaza el tlp + botones de coordenadas absolutas por un
            // layout de dos niveles (DockStyle) que siempre queda visible.
            var pnlBody = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(28, 16, 28, 16)
            };

            // TableLayoutPanel de campos: Dock Top, alto fijo por fila
            var tlp = new TableLayoutPanel
            {
                ColumnCount     = 2,
                RowCount        = 3,
                Dock            = DockStyle.Top,
                Height          = 162,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
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
            cmbTipo.SelectedIndex = 0;

            dtpFecha = new DateTimePicker
            {
                Dock   = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font   = new Font("Segoe UI", 10.5f),
                Value  = DateTime.Today.AddDays(30),
                Margin = new Padding(3)
            };

            AddRow(tlp, 0, "Nombre:", txtNombre);
            AddRow(tlp, 1, "Tipo:",   cmbTipo);
            AddRow(tlp, 2, "Fecha:",  dtpFecha);

            // ── Botones en FlowLayoutPanel Dock Bottom ────────────────────────
            // FIX: los botones se anclan al fondo del pnlBody con Dock Bottom,
            // eliminando las coordenadas absolutas que causaban el recorte.
            var pnlBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 58,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 10, 0, 0)
            };

            btnCancelar = new Button
            {
                Text      = "✖  Cancelar",
                Size      = new Size(130, 40),
                BackColor = Color.FromArgb(190, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 0, 0)
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            btnGuardar = new Button
            {
                Text      = "✔  Guardar",
                Size      = new Size(130, 40),
                BackColor = Color.FromArgb(18, 30, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 12, 0)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            // FlowDirection.RightToLeft → primero Cancelar, luego Guardar
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Controls.Add(btnGuardar);

            pnlBody.Controls.Add(pnlBotones);
            pnlBody.Controls.Add(tlp);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);

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

        private void BtnGuardar_Click(object? sender, EventArgs e)
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
                    Tipo   = cmbTipo.SelectedItem?.ToString() ?? "Otro",
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