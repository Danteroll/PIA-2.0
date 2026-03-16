using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    public class InvitadosControl : UserControl
    {
        // ── Controles ────────────────────────────────────────────────────────
        private ComboBox       cmbEventos;
        private TextBox        txtNombre, txtTelefono, txtAlergias, txtGrupo, txtBuscar;
        private CheckBox       chkConfirmado;
        private NumericUpDown  nudAcompanantes;
        private ListView       lstInvitados;
        private Button         btnAgregar, btnEditar, btnEliminar;
        private System.Windows.Forms.Timer _timerSync;

        private string EventoActual =>
            cmbEventos.SelectedItem?.ToString();
        private int _idSeleccionado = -1;

        public InvitadosControl()
        {
            BuildUI();
            IniciarTimer();
        }

        // ─── UI ────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            BackColor = Color.FromArgb(235, 240, 250);
            Dock      = DockStyle.Fill;

            // ── Header ───────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = Color.White,
                Padding   = new Padding(20, 0, 20, 0)
            };
            pnlHeader.Paint += (s, e) =>
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(215, 225, 245)),
                    0, pnlHeader.Height - 1, pnlHeader.Width, 1);

            var headerRow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 18, 0, 0)
            };

            var lblTitulo = new Label
            {
                Text      = "👥   Invitados",
                Font      = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(0, 0, 18, 0)
            };

            var lblEvLbl = new Label
            {
                Text      = "Evento:",
                AutoSize  = true,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 75, 110),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(12, 5, 8, 0)
            };

            cmbEventos = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width         = 280,
                Font          = new Font("Segoe UI", 10.5f),
                Margin        = new Padding(0, 2, 0, 0),
                DropDownWidth = 420
            };
            cmbEventos.SelectedIndexChanged += CmbEventos_Changed;

            headerRow.Controls.Add(lblTitulo);
            headerRow.Controls.Add(lblEvLbl);
            headerRow.Controls.Add(cmbEventos);
            pnlHeader.Controls.Add(headerRow);

            // ── Panel de campos ──────────────────────────────────────────────
            var pnlCampos = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 145,
                BackColor = Color.White,
                Padding   = new Padding(20, 14, 20, 10)
            };
            pnlCampos.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(218, 226, 240)))
                    e.Graphics.DrawRectangle(pen, 0, 0,
                        ((Panel)s).Width - 1, ((Panel)s).Height - 1);
            };

            var tlp = new TableLayoutPanel
            {
                ColumnCount     = 4,
                RowCount        = 2,
                Dock            = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            txtNombre   = MakeTxt();
            txtTelefono = MakeTxt();
            txtAlergias = MakeTxt();
            txtGrupo    = MakeTxt();

            tlp.Controls.Add(MakeLbl("Nombre:"),   0, 0); tlp.Controls.Add(txtNombre,   1, 0);
            tlp.Controls.Add(MakeLbl("Teléfono:"), 2, 0); tlp.Controls.Add(txtTelefono, 3, 0);
            tlp.Controls.Add(MakeLbl("Alergias:"), 0, 1); tlp.Controls.Add(txtAlergias, 1, 1);
            tlp.Controls.Add(MakeLbl("Grupo:"),    2, 1); tlp.Controls.Add(txtGrupo,    3, 1);

            pnlCampos.Controls.Add(tlp);

            // ── Panel controles (checkboxes, botones) ─────────────────────
            var pnlCtrl = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = Color.White,
                Padding   = new Padding(20, 0, 20, 0)
            };

            chkConfirmado = new CheckBox
            {
                Text      = "CONFIRMADO",
                ForeColor = Color.FromArgb(22, 148, 75),
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize  = true,
                Cursor    = Cursors.Hand
            };

            var lblAcomp = new Label
            {
                Text      = "Acompañantes:",
                AutoSize  = true,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(55, 65, 90)
            };

            nudAcompanantes = new NumericUpDown
            {
                Minimum  = 0,
                Maximum  = 30,
                Width    = 72,
                Font     = new Font("Segoe UI", 10.5f)
            };

            btnAgregar  = MakeBtn("Agregar",  Color.FromArgb(18, 118, 55));
            btnEditar   = MakeBtn("Editar",   Color.FromArgb(28, 95, 180));
            btnEliminar = MakeBtn("Eliminar", Color.FromArgb(190, 40, 40));

            btnAgregar.Click  += BtnAgregar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;

            var flpCtrl = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 14, 0, 0)
            };
            chkConfirmado.Margin     = new Padding(0, 4, 18, 0);
            lblAcomp.Margin          = new Padding(0, 7, 8, 0);
            nudAcompanantes.Margin   = new Padding(0, 4, 18, 0);
            btnAgregar.Margin        = new Padding(0, 2, 10, 0);
            btnEditar.Margin         = new Padding(0, 2, 10, 0);
            btnEliminar.Margin       = new Padding(0, 2, 0, 0);

            flpCtrl.Controls.AddRange(new Control[]
            {
                chkConfirmado, lblAcomp, nudAcompanantes,
                btnAgregar, btnEditar, btnEliminar
            });
            pnlCtrl.Controls.Add(flpCtrl);

            // ── Buscador + lista ─────────────────────────────────────────────
            var pnlLista = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 20)
            };

            var pnlBuscar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.Transparent
            };

            var lblBuscar = new Label
            {
                Text      = "Buscar:",
                AutoSize  = true,
                Location  = new Point(0, 8),
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(60, 75, 110)
            };

            txtBuscar = new TextBox
            {
                Location    = new Point(68, 5),
                Font        = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (_, __) => FiltrarLista();

            pnlBuscar.Controls.Add(lblBuscar);
            pnlBuscar.Controls.Add(txtBuscar);

            var hdr = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 26,
                ColumnCount = 4,
                BackColor   = Color.FromArgb(245, 248, 255),
                Padding     = new Padding(8, 2, 8, 2)
            };
            hdr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            hdr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            hdr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            hdr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            hdr.Controls.Add(MakeHdr("Nombre"),    0, 0);
            hdr.Controls.Add(MakeHdr("Teléfono"),  1, 0);
            hdr.Controls.Add(MakeHdr("Alergias"),  2, 0);
            hdr.Controls.Add(MakeHdr("Grupo"),     3, 0);

            lstInvitados = new ListView
            {
                Dock       = DockStyle.Fill,
                Font       = new Font("Segoe UI", 10.5f),
                BorderStyle= BorderStyle.FixedSingle,
                View       = View.Details,
                FullRowSelect = true,
                HideSelection = false
            };
            lstInvitados.Columns.Add("Nombre",   260);
            lstInvitados.Columns.Add("Teléfono", 150);
            lstInvitados.Columns.Add("Alergias", 90);
            lstInvitados.Columns.Add("Grupo",    160);
            lstInvitados.SelectedIndexChanged += LstInvitados_Changed;

            pnlLista.Controls.Add(lstInvitados);
            pnlLista.Controls.Add(hdr);
            pnlLista.Controls.Add(pnlBuscar);
            pnlLista.Resize += (_, __) => AjustarLista();

            Controls.Add(pnlLista);
            Controls.Add(pnlCtrl);
            Controls.Add(pnlCampos);
            Controls.Add(pnlHeader);
        }

        // ─── Helpers UI ────────────────────────────────────────────────────────
        private static TextBox MakeTxt() =>
            new TextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 10.5f),
                Margin      = new Padding(3),
                BorderStyle = BorderStyle.FixedSingle
            };

        private static Label MakeLbl(string txt) =>
            new Label
            {
                Text      = txt,
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 90)
            };

        private static Button MakeBtn(string txt, Color col)
        {
            var b = new Button
            {
                Text      = txt,
                Size      = new Size(105, 38),
                BackColor = col,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Label MakeHdr(string txt) =>
            new Label
            {
                Text      = txt,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 75, 110)
            };

        // ─── Timer ─────────────────────────────────────────────────────────────
        private void IniciarTimer()
        {
            _timerSync = new System.Windows.Forms.Timer { Interval = 5_000 };
            _timerSync.Tick += (_, __) => CargarEventos(silencioso: true);
            _timerSync.Start();
        }

        // ─── Lógica ────────────────────────────────────────────────────────────
        public void CargarEventos(bool silencioso = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => CargarEventos(silencioso)));
                return;
            }

            string prev = cmbEventos.SelectedItem?.ToString();
            cmbEventos.SelectedIndexChanged -= CmbEventos_Changed;
            cmbEventos.Items.Clear();

            var nombres = DatabaseManager.GetEventos()
                .Where(e => !e.Pasado)
                .Select(e => e.Nombre)
                .ToArray();
            cmbEventos.Items.AddRange(nombres);

            if (prev != null && cmbEventos.Items.Contains(prev))
                cmbEventos.SelectedItem = prev;
            else if (cmbEventos.Items.Count > 0)
                cmbEventos.SelectedIndex = 0;

            cmbEventos.SelectedIndexChanged += CmbEventos_Changed;

            if (!silencioso) CargarLista();
        }

        private void CmbEventos_Changed(object sender, EventArgs e)
        {
            LimpiarFormulario();
            CargarLista();
        }

        private void CargarLista()
        {
            lstInvitados.Items.Clear();
            if (EventoActual == null) return;

            string filtro = txtBuscar.Text.Trim().ToLowerInvariant();
            var todos = DatabaseManager.GetInvitados(EventoActual);

            foreach (var inv in todos.Where(i =>
                string.IsNullOrEmpty(filtro) ||
                i.Nombre.ToLowerInvariant().Contains(filtro) ||
                i.Grupo.ToLowerInvariant().Contains(filtro)))
            {
                var alergias = string.IsNullOrWhiteSpace(inv.Alergias) ||
                               string.Equals(inv.Alergias.Trim(), "ninguna", StringComparison.OrdinalIgnoreCase)
                    ? "No"
                    : "Sí";
                var item = new ListViewItem(inv.Nombre + (inv.Confirmado ? "  ✓" : ""))
                {
                    Tag = inv
                };
                item.SubItems.Add(inv.Telefono);
                item.SubItems.Add(alergias);
                item.SubItems.Add(inv.Grupo);
                lstInvitados.Items.Add(item);
            }
            AjustarLista();
        }

        private void FiltrarLista() => CargarLista();

        private void LstInvitados_Changed(object sender, EventArgs e)
        {
            if (lstInvitados.SelectedItems.Count == 1 &&
                lstInvitados.SelectedItems[0].Tag is Invitado inv)
            {
                _idSeleccionado         = inv.Id;
                txtNombre.Text          = inv.Nombre;
                txtTelefono.Text        = inv.Telefono;
                txtAlergias.Text        = inv.Alergias;
                txtGrupo.Text           = inv.Grupo;
                chkConfirmado.Checked   = inv.Confirmado;
                nudAcompanantes.Value   = inv.Acompanantes;
            }
        }

        private void LimpiarFormulario()
        {
            _idSeleccionado       = -1;
            txtNombre.Text        =
            txtTelefono.Text      =
            txtAlergias.Text      =
            txtGrupo.Text         = "";
            chkConfirmado.Checked = false;
            nudAcompanantes.Value = 0;
            lstInvitados.SelectedItems.Clear();
        }

        private Invitado LeerFormulario() => new Invitado
        {
            Nombre       = txtNombre.Text.Trim(),
            Telefono     = txtTelefono.Text.Trim(),
            Alergias     = txtAlergias.Text.Trim(),
            Grupo        = txtGrupo.Text.Trim(),
            Confirmado   = chkConfirmado.Checked,
            Acompanantes = (int)nudAcompanantes.Value
        };

        // ─── Botones CRUD ──────────────────────────────────────────────────────
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del invitado es requerido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseManager.AgregarInvitado(EventoActual, LeerFormulario());
            LimpiarFormulario();
            CargarLista();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado < 0)
            {
                MessageBox.Show("Selecciona un invitado de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es requerido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var inv = LeerFormulario();
            inv.Id = _idSeleccionado;
            DatabaseManager.EditarInvitado(EventoActual, inv);
            CargarLista();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado < 0)
            {
                MessageBox.Show("Selecciona un invitado de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Eliminar este invitado?", "Confirmar eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DatabaseManager.EliminarInvitado(EventoActual, _idSeleccionado);
                LimpiarFormulario();
                CargarLista();
            }
        }

        private void AjustarLista()
        {
            if (txtBuscar == null || lstInvitados == null) return;
            int w = Math.Max(0, (Parent?.ClientSize.Width ?? Width) - 420);
            txtBuscar.Width = Math.Max(220, w);

            int total = lstInvitados.ClientSize.Width;
            if (total <= 0 || lstInvitados.Columns.Count != 4) return;
            lstInvitados.Columns[0].Width = (int)(total * 0.38);
            lstInvitados.Columns[1].Width = (int)(total * 0.22);
            lstInvitados.Columns[2].Width = (int)(total * 0.18);
            lstInvitados.Columns[3].Width = (int)(total * 0.22);
        }
    }
}
