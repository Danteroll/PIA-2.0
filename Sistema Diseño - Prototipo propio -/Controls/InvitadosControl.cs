using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    public class InvitadosControl : UserControl
    {
        // ── Controles (= null! silencia CS8618 — se asignan en BuildUI) ──────
        private ComboBox      cmbEventos      = null!;
        private TextBox       txtNombre       = null!;
        private TextBox       txtTelefono     = null!;
        private TextBox       txtAlergias     = null!;
        private TextBox       txtBuscar       = null!;
        private NumericUpDown nudAcompanantes = null!;
        private ListView      lstInvitados    = null!;
        private Button        btnAgregar      = null!;
        private Button        btnEditar       = null!;
        private Button        btnEliminar     = null!;
        private System.Windows.Forms.Timer _timerSync = null!;

        private string? EventoActual => cmbEventos.SelectedItem?.ToString();
        private int _idSeleccionado = -1;
        private bool _confirmadoSeleccionado;
        private bool _updatingChecks;

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
            // FIX: AutoSize + MinimumSize en vez de Height fijo para que no se
            // recorten las labels en ventanas pequeñas.
            var pnlCampos = new Panel
            {
                Dock        = DockStyle.Top,
                MinimumSize = new Size(0, 112),   // garantiza alto mínimo visible
                AutoSize    = true,
                AutoSizeMode= AutoSizeMode.GrowAndShrink,
                BackColor   = Color.White,
                Padding     = new Padding(20, 14, 20, 10)
            };
            pnlCampos.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(218, 226, 240));
                e.Graphics.DrawRectangle(pen, 0, 0,
                    ((Panel)s!).Width - 1, ((Panel)s!).Height - 1);
            };

            // FIX: el TableLayoutPanel de campos usa Anchor en vez de Dock
            // para que el alto no dependa del panel padre y las labels
            // tengan espacio suficiente siempre.
            var tlp = new TableLayoutPanel
            {
                ColumnCount     = 4,
                RowCount        = 2,
                Dock            = DockStyle.Top,
                Height          = 100,              // alto fijo suficiente para 2 filas
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            txtNombre   = MakeTxt();
            txtTelefono = MakeTxt();
            txtAlergias = MakeTxt();

            tlp.Controls.Add(MakeLbl("Nombre:"),   0, 0); tlp.Controls.Add(txtNombre,   1, 0);
            tlp.Controls.Add(MakeLbl("Teléfono:"), 2, 0); tlp.Controls.Add(txtTelefono, 3, 0);
            tlp.Controls.Add(MakeLbl("Alergias:"), 0, 1); tlp.Controls.Add(txtAlergias, 1, 1);
            tlp.SetColumnSpan(txtAlergias, 3);

            pnlCampos.Controls.Add(tlp);

            // ── Panel controles ──────────────────────────────────────────────
            // FIX: WrapContents = true para que los botones bajen de línea
            // en vez de salirse del panel en ventanas estrechas.
            var pnlCtrl = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = Color.White,
                Padding   = new Padding(20, 0, 20, 0)
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
                Minimum = 0,
                Maximum = 30,
                Width   = 72,
                Font    = new Font("Segoe UI", 10.5f)
            };

            btnAgregar  = MakeBtn("Agregar",  Color.FromArgb(18, 118, 55));
            btnEditar   = MakeBtn("Editar",   Color.FromArgb(28, 95, 180));
            btnEliminar = MakeBtn("Eliminar", Color.FromArgb(190, 40, 40));

            btnAgregar.Click  += BtnAgregar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;

            var btnImportar  = MakeBtn("📥 Importar Excel", Color.FromArgb(34, 120, 34));
            var btnPlantilla = MakeBtn("📄 Plantilla",      Color.FromArgb(70, 110, 170));
            btnImportar.Click  += BtnImportarExcel_Click;
            btnPlantilla.Click += BtnDescargarPlantilla_Click;

            var flpCtrl = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,               // FIX: permite wrap en pantallas pequeñas
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 14, 0, 0)
            };
            lblAcomp.Margin        = new Padding(0, 7, 8, 0);
            nudAcompanantes.Margin = new Padding(0, 4, 18, 0);
            btnAgregar.Margin      = new Padding(0, 2, 10, 0);
            btnEditar.Margin       = new Padding(0, 2, 10, 0);
            btnEliminar.Margin     = new Padding(0, 2, 0, 0);
            btnImportar.Margin     = new Padding(14, 2, 10, 0);
            btnPlantilla.Margin    = new Padding(0, 2, 0, 0);

            flpCtrl.Controls.AddRange(new Control[]
            {
                lblAcomp, nudAcompanantes,
                btnAgregar, btnEditar, btnEliminar,
                btnImportar, btnPlantilla
            });
            pnlCtrl.Controls.Add(flpCtrl);

            // ── Buscador + lista ─────────────────────────────────────────────
            var pnlLista = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 20)
            };

            // FIX: pnlBuscar usa layout relativo con un FlowLayoutPanel
            // para que lblBuscar y txtBuscar se distribuyan correctamente
            // sin depender de coordenadas absolutas.
            var pnlBuscar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 38,
                BackColor = Color.Transparent
            };

            var flpBuscar = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 4, 0, 0)
            };

            var lblBuscar = new Label
            {
                Text      = "Buscar:",
                AutoSize  = false,
                Width     = 62,
                Height    = 28,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(60, 75, 110),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(0, 0, 6, 0)
            };

            txtBuscar = new TextBox
            {
                // FIX: Anchor + height en vez de Location absoluta
                Width       = 280,
                Height      = 26,
                Font        = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.FixedSingle,
                Margin      = new Padding(0, 0, 0, 0)
            };
            txtBuscar.TextChanged += (_, __) => FiltrarLista();

            // Ajustar ancho de txtBuscar al redimensionar
            pnlBuscar.Resize += (_, __) =>
            {
                int w = pnlBuscar.Width - lblBuscar.Width - 20;
                txtBuscar.Width = Math.Max(120, w);
            };

            flpBuscar.Controls.Add(lblBuscar);
            flpBuscar.Controls.Add(txtBuscar);
            pnlBuscar.Controls.Add(flpBuscar);

            lstInvitados = new ListView
            {
                Dock          = DockStyle.Fill,
                Font          = new Font("Segoe UI", 10.5f),
                BorderStyle   = BorderStyle.FixedSingle,
                View          = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                CheckBoxes    = true
            };
            lstInvitados.Columns.Add("Nombre",   260);
            lstInvitados.Columns.Add("Teléfono", 150);
            lstInvitados.Columns.Add("Alergias", 90);
            lstInvitados.SelectedIndexChanged += LstInvitados_Changed;
            lstInvitados.ItemChecked += LstInvitados_ItemChecked;

            pnlLista.Controls.Add(lstInvitados);
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
                Margin      = new Padding(3, 8, 3, 3),   // FIX: margen superior para centrar
                BorderStyle = BorderStyle.FixedSingle
            };

        private static Label MakeLbl(string txt) =>
            new Label
            {
                Text      = txt,
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 90),
                // FIX: AutoSize false + Dock Fill garantiza que el texto
                // se vea completo con el ":" en la misma línea.
                AutoSize  = false
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
            if (InvokeRequired) { Invoke(new Action(() => CargarEventos(silencioso))); return; }

            string? prev = cmbEventos.SelectedItem?.ToString();
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

        private void CmbEventos_Changed(object? sender, EventArgs e)
        {
            LimpiarFormulario();
            CargarLista();
        }

        private void CargarLista()
        {
            _updatingChecks = true;
            lstInvitados.Items.Clear();
            if (EventoActual == null)
            {
                _updatingChecks = false;
                return;
            }

            string filtro = txtBuscar.Text.Trim().ToLowerInvariant();
            var todos = DatabaseManager.GetInvitados(EventoActual);

            foreach (var inv in todos.Where(i =>
                string.IsNullOrEmpty(filtro) ||
                i.Nombre.ToLowerInvariant().Contains(filtro) ||
                i.Telefono.ToLowerInvariant().Contains(filtro)))
            {
                // Evita mostrar una fila importada accidentalmente como encabezado.
                if (string.Equals(inv.Nombre.Trim(), "nombre", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(inv.Telefono.Trim(), "teléfono", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(inv.Telefono.Trim(), "telefono", StringComparison.OrdinalIgnoreCase)))
                    continue;

                string alergias =
                    string.IsNullOrWhiteSpace(inv.AlergiasText) ||
                    string.Equals(inv.AlergiasText?.Trim(), "ninguna", StringComparison.OrdinalIgnoreCase)
                        ? "No" : "Sí";

                var item = new ListViewItem(inv.Nombre)
                    { Tag = inv };
                item.SubItems.Add(inv.Telefono);
                item.SubItems.Add(alergias);
                item.Checked = inv.Confirmado;
                lstInvitados.Items.Add(item);
            }
            _updatingChecks = false;
            AjustarLista();
        }

        private void FiltrarLista() => CargarLista();

        private void LstInvitados_Changed(object? sender, EventArgs e)
        {
            if (lstInvitados.SelectedItems.Count == 1 &&
                lstInvitados.SelectedItems[0].Tag is Invitado inv)
            {
                _idSeleccionado       = inv.Id;
                txtNombre.Text        = inv.Nombre;
                txtTelefono.Text      = inv.Telefono;
                txtAlergias.Text      = inv.AlergiasText;
                _confirmadoSeleccionado = inv.Confirmado;
                nudAcompanantes.Value = inv.Acompanantes;
            }
        }

        private void LstInvitados_ItemChecked(object? sender, ItemCheckedEventArgs e)
        {
            if (_updatingChecks || EventoActual == null) return;
            if (e.Item.Tag is not Invitado inv) return;

            inv.Confirmado = e.Item.Checked;
            _confirmadoSeleccionado = inv.Confirmado;
            DatabaseManager.EditarInvitado(EventoActual, inv);
        }

        private void LimpiarFormulario()
        {
            _idSeleccionado       = -1;
            txtNombre.Text        =
            txtTelefono.Text      =
            txtAlergias.Text      =
            "";
            _confirmadoSeleccionado = false;
            nudAcompanantes.Value = 0;
            lstInvitados.SelectedItems.Clear();
        }

        private Invitado LeerFormulario()
        {
            string textoAlergias = txtAlergias.Text.Trim();

            var listaAlergiasObj = new List<Alergia>();
            if (!string.IsNullOrWhiteSpace(textoAlergias) && !string.Equals(textoAlergias, "no", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in textoAlergias.Split(','))
                {
                    string alergia = item.Trim();
                    if (!string.IsNullOrEmpty(alergia))
                    {
                        listaAlergiasObj.Add(new Alergia { NombreIngrediente = alergia });
                    }
                }
            }

            return new Invitado
            {
                Nombre       = txtNombre.Text.Trim(),
                Telefono     = txtTelefono.Text.Trim(),
                Grupo        = "",
                Confirmado   = _confirmadoSeleccionado,
                Acompanantes = (int)nudAcompanantes.Value,
                AlergiasText = textoAlergias,
                Alergias     = listaAlergiasObj
            };
        }

        // ─── Botones CRUD ──────────────────────────────────────────────────────
        private void BtnAgregar_Click(object? sender, EventArgs e)
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

        private void BtnEditar_Click(object? sender, EventArgs e)
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
            DatabaseManager.EditarInvitado(EventoActual!, inv);
            CargarLista();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
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
                DatabaseManager.EliminarInvitado(EventoActual!, _idSeleccionado);
                LimpiarFormulario();
                CargarLista();
            }
        }

        // ─── Botones Excel ─────────────────────────────────────────────────────
        private void BtnImportarExcel_Click(object? sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title  = "Seleccionar archivo Excel de Invitados",
                Filter = "Excel (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var (invitados, erroresLectura) = ExcelImporter.LeerInvitadosDeExcel(dlg.FileName);

            if (invitados.Count == 0)
            {
                string msg = "No se encontraron invitados en el archivo.";
                if (erroresLectura.Count > 0)
                    msg += "\n\n" + string.Join("\n", erroresLectura);
                MessageBox.Show(msg, "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int guardados = 0, omitidos = 0;
            var erroresBD = new List<string>();

            var existentes = DatabaseManager.GetInvitados(EventoActual)
                .Select(i => i.Nombre.ToLowerInvariant())
                .ToHashSet();

            foreach (var inv in invitados)
            {
                try
                {
                    if (existentes.Contains(inv.Nombre.ToLowerInvariant()))
                    { omitidos++; erroresBD.Add($"'{inv.Nombre}' ya existe → omitido."); continue; }

                    DatabaseManager.AgregarInvitado(EventoActual, inv);
                    existentes.Add(inv.Nombre.ToLowerInvariant());
                    guardados++;
                }
                catch (Exception ex)
                { omitidos++; erroresBD.Add($"'{inv.Nombre}': {ex.Message}"); }
            }

            CargarLista();

            var todos = new List<string>(erroresLectura);
            todos.AddRange(erroresBD);
            string resumen = $"Importación completada.\n\n✔ Guardados: {guardados}\n⚠ Omitidos: {omitidos}";
            if (todos.Count > 0) resumen += "\n\nDetalles:\n• " + string.Join("\n• ", todos);
            MessageBox.Show(resumen, "Importar Invitados", MessageBoxButtons.OK,
                todos.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void BtnDescargarPlantilla_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title    = "Guardar plantilla de Invitados",
                Filter   = "Excel (*.xlsx)|*.xlsx",
                FileName = "Plantilla_Invitados.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                ExcelImporter.GenerarPlantillaInvitados(dlg.FileName);
                MessageBox.Show(
                    "Plantilla guardada.\n\n" +
                    "Columnas: Nombre | Teléfono | Grupo | Alergias | Confirmado\n\n" +
                    "• El campo Grupo se ignora en la app actual\n" +
                    "• 'Confirmado' acepta: Sí / No / 1 / 0\n" +
                    "• Solo 'Nombre' es obligatorio",
                    "Plantilla lista", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar la plantilla:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── AjustarLista ──────────────────────────────────────────────────────
        private void AjustarLista()
        {
            if (lstInvitados == null) return;

            int total = lstInvitados.ClientSize.Width;
            if (total <= 0 || lstInvitados.Columns.Count != 3) return;
            lstInvitados.Columns[0].Width = (int)(total * 0.46);
            lstInvitados.Columns[1].Width = (int)(total * 0.30);
            lstInvitados.Columns[2].Width = (int)(total * 0.24);
        }
    }
}