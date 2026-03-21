using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    // ══════════════════════════════════════════════════════════════════════════
    //  MapaMesasControl
    // ══════════════════════════════════════════════════════════════════════════
    public class MapaMesasControl : UserControl
    {
        // = null! silencia CS8618 — todos se asignan en BuildUI()
        private ComboBox      cmbEventos      = null!;
        private Panel         pnlCanvas       = null!;
        private ListBox       lstInvitados    = null!;
        private NumericUpDown nudCapacidad    = null!;
        private Button        btnAgregarMesa  = null!;
        private Button        btnAsignar      = null!;
        private Button        btnQuitarAsig   = null!;
        private Button        btnEliminarMesa = null!;
        private System.Windows.Forms.Timer _timerSync = null!;
       

        // _mesaSel es nullable por diseño
        private string? EventoActual => cmbEventos.SelectedItem?.ToString();
        private MesaPanel?       _mesaSel;
        private List<MesaPanel>  _paneles = new List<MesaPanel>();

        public MapaMesasControl()
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
                Text      = "🍽️   Mapa de Mesas",
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

            // ── Área principal ────────────────────────────────────────────────
            var pnlMain = new Panel { Dock = DockStyle.Fill };

            // Panel derecho
            var pnlDerecho = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 228,
                BackColor = Color.White,
                Padding   = new Padding(10, 12, 10, 10)
            };
            pnlDerecho.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(215, 225, 245));
                e.Graphics.DrawLine(pen, 0, 0, 0, ((Panel)s!).Height);
            };

            var lblListaTit = new Label
            {
                Text      = "Lista Invitados",
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lstInvitados = new ListBox
            {
                Dock                = DockStyle.Top,
                Height              = 240,
                Font                = new Font("Segoe UI", 9.5f),
                ScrollAlwaysVisible = true,
                BorderStyle         = BorderStyle.FixedSingle
            };

            var sep1 = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };

            var lblCap = new Label
            {
                Text      = "Capacidad de la mesa:",
                Dock      = DockStyle.Top,
                Height    = 22,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 75, 110),
                TextAlign = ContentAlignment.MiddleLeft
            };

            nudCapacidad = new NumericUpDown
            {
                Dock    = DockStyle.Top,
                Minimum = 1,
                Maximum = 60,
                Value   = 8,
                Font    = new Font("Segoe UI", 10.5f)
            };

            btnAgregarMesa  = MakeBtn("＋ Agregar Mesa",     Color.FromArgb(18, 30, 58));
            btnAsignar      = MakeBtn("► Asignar a mesa",    Color.FromArgb(18, 120, 55));
            btnQuitarAsig   = MakeBtn("◄ Quitar asignación", Color.FromArgb(180, 100, 18));
            btnEliminarMesa = MakeBtn("🗑  Eliminar mesa",   Color.FromArgb(185, 40, 40));

            btnAgregarMesa.Click  += BtnAgregarMesa_Click;
            btnAsignar.Click      += BtnAsignar_Click;
            btnQuitarAsig.Click   += BtnQuitarAsig_Click;
            btnEliminarMesa.Click += BtnEliminarMesa_Click;

            // Botones Excel ────────────────────────────────────────────────────
            var btnImportarMesas  = MakeBtn("📥 Importar Excel", Color.FromArgb(34, 120, 34));
            var btnPlantillaMesas = MakeBtn("📄 Plantilla",      Color.FromArgb(70, 110, 170));
            btnImportarMesas.Click  += BtnImportarMesas_Click;
            btnPlantillaMesas.Click += BtnDescargarPlantillaMesas_Click;

            var sep2 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };
            var sep3 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };
            var sep4 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };
            var sep5 = new Panel { Dock = DockStyle.Top, Height = 8,  BackColor = Color.Transparent };
            var sep6 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };
            var sep7 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };

            pnlDerecho.Controls.Add(btnPlantillaMesas);
            pnlDerecho.Controls.Add(sep7);
            pnlDerecho.Controls.Add(btnImportarMesas);
            pnlDerecho.Controls.Add(sep6);
            pnlDerecho.Controls.Add(btnEliminarMesa);
            pnlDerecho.Controls.Add(sep4);
            pnlDerecho.Controls.Add(btnQuitarAsig);
            pnlDerecho.Controls.Add(sep3);
            pnlDerecho.Controls.Add(btnAsignar);
            pnlDerecho.Controls.Add(sep2);
            pnlDerecho.Controls.Add(btnAgregarMesa);
            pnlDerecho.Controls.Add(sep5);
            pnlDerecho.Controls.Add(nudCapacidad);
            pnlDerecho.Controls.Add(lblCap);
            pnlDerecho.Controls.Add(sep1);
            pnlDerecho.Controls.Add(lstInvitados);
            pnlDerecho.Controls.Add(lblListaTit);

            // Canvas
            pnlCanvas = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.FromArgb(225, 232, 248),
                AutoScroll = true
            };
            pnlCanvas.Paint += PnlCanvas_Paint;

            pnlMain.Controls.Add(pnlCanvas);
            pnlMain.Controls.Add(pnlDerecho);

            Controls.Add(pnlMain);
            Controls.Add(pnlHeader);
        }

        private static Button MakeBtn(string txt, Color col)
        {
            var b = new Button
            {
                Text      = txt,
                Dock      = DockStyle.Top,
                Height    = 38,
                BackColor = col,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void PnlCanvas_Paint(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(210, 218, 238));
            for (int x = 0; x < pnlCanvas.Width; x += 30)
                e.Graphics.DrawLine(pen, x, 0, x, pnlCanvas.Height);
            for (int y = 0; y < pnlCanvas.Height; y += 30)
                e.Graphics.DrawLine(pen, 0, y, pnlCanvas.Width, y);
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

            // ── FIX: siempre recargar canvas e invitados al navegar a esta pestaña
            if (!silencioso)
            {
                CargarCanvas();
                CargarInvitados();
            }
        }

        private void CmbEventos_Changed(object? sender, EventArgs e)
        {
            CargarCanvas();
            CargarInvitados();
        }

        private void CargarCanvas()
        {
            pnlCanvas.Controls.Clear();
            _paneles.Clear();
            _mesaSel = null;

            if (EventoActual == null) return;

            var mesas = DatabaseManager.GetMesas(EventoActual);
            foreach (var mesa in mesas)
            {
                var mp = CrearMesaPanel(mesa);
                pnlCanvas.Controls.Add(mp);
                _paneles.Add(mp);
            }
        }

        private MesaPanel CrearMesaPanel(Mesa mesa)
        {
            var mp = new MesaPanel(mesa);
            mp.Location       = new Point(mesa.PosX, mesa.PosY);
            mp.Seleccionado  += () => SeleccionarMesa(mp);
            mp.PosicionCambio += (id, x, y) =>
            {
                if (EventoActual != null)
                    DatabaseManager.ActualizarPosMesa(EventoActual, id, x, y);
            };
            return mp;
        }

        private void SeleccionarMesa(MesaPanel mp)
        {
            foreach (var p in _paneles) p.Activa = false;
            mp.Activa = true;
            _mesaSel  = mp;
        }

        // ── FIX sincronización: carga invitados desde BD cada vez ────────────
        private void CargarInvitados()
        {
            lstInvitados.Items.Clear();
            if (EventoActual == null) return;

            // Obtener todos los invitados del evento frescos desde la BD
            var todos     = DatabaseManager.GetInvitados(EventoActual);
            var mesas     = DatabaseManager.GetMesas(EventoActual);
            var asignados = new HashSet<int>(
                mesas.SelectMany(m => m.Invitados.Select(i => i.Id)));

            // Mostrar solo los NO asignados a ninguna mesa
            foreach (var inv in todos.Where(i => !asignados.Contains(i.Id)))
                lstInvitados.Items.Add(inv);
        }

        // ─── Botones CRUD ──────────────────────────────────────────────────────
        private void BtnAgregarMesa_Click(object? sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int col = _paneles.Count % 4;
            int row = _paneles.Count / 4;
            int px  = 30 + col * 185;
            int py  = 30 + row * 165;

            int id = DatabaseManager.AgregarMesa(
                EventoActual, (int)nudCapacidad.Value, px, py);

            var mesa = new Mesa
            {
                Id        = id,
                Numero    = _paneles.Count + 1,
                Capacidad = (int)nudCapacidad.Value,
                PosX      = px,
                PosY      = py
            };

            var mp = CrearMesaPanel(mesa);
            pnlCanvas.Controls.Add(mp);
            _paneles.Add(mp);
            SeleccionarMesa(mp);
        }

        private void BtnAsignar_Click(object? sender, EventArgs e)
        {
            if (_mesaSel == null)
            {
                MessageBox.Show("Selecciona una mesa en el canvas.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lstInvitados.SelectedItem is not Invitado inv)
            {
                MessageBox.Show("Selecciona un invitado de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mesa = _mesaSel.Mesa;
            if (mesa.Invitados.Count >= mesa.Capacidad)
            {
                MessageBox.Show(
                    $"La Mesa {mesa.Numero} está llena ({mesa.Capacidad} personas).",
                    "Mesa llena", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseManager.AsignarInvitadoMesa(EventoActual!, inv.Id, mesa.Id);
            RecargarYMantienerSel(mesa.Id);
        }

        private void BtnQuitarAsig_Click(object? sender, EventArgs e)
        {
            if (_mesaSel == null)
            {
                MessageBox.Show("Selecciona una mesa primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var invsMesa = _mesaSel.Mesa.Invitados;
            if (invsMesa.Count == 0)
            {
                MessageBox.Show("Esta mesa no tiene invitados asignados.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new Form
            {
                Text            = "Quitar invitado de mesa",
                Size            = new Size(300, 230),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false
            };

            var lb = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
            foreach (var i in invsMesa) lb.Items.Add(i);

            var btnOk = new Button
            {
                Text      = "Quitar",
                Dock      = DockStyle.Bottom,
                Height    = 38,
                BackColor = Color.FromArgb(180, 100, 18),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;

            int mesaId = _mesaSel.Mesa.Id;
            btnOk.Click += (_, __) =>
            {
                if (lb.SelectedItem is Invitado sel)
                {
                    DatabaseManager.QuitarInvitadoMesa(EventoActual!, sel.Id);
                    dlg.Close();
                    RecargarYMantienerSel(mesaId);
                }
            };

            dlg.Controls.Add(lb);
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }

        private void BtnEliminarMesa_Click(object? sender, EventArgs e)
        {
            if (_mesaSel == null)
            {
                MessageBox.Show("Selecciona una mesa para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                $"¿Eliminar {_mesaSel.Mesa}?\nSe desasignarán sus invitados.",
                "Confirmar", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DatabaseManager.EliminarMesa(EventoActual!, _mesaSel.Mesa.Id);
                CargarCanvas();
                CargarInvitados();
            }
        }

        private void RecargarYMantienerSel(int mesaId)
        {
            CargarCanvas();
            CargarInvitados();
            var mp = _paneles.FirstOrDefault(p => p.Mesa.Id == mesaId);
            if (mp != null) SeleccionarMesa(mp);
        }

        // ─── Botones Excel ─────────────────────────────────────────────────────

        /// <summary>
        /// Verifica que haya invitados en el evento actual.
        /// Retorna true si puede proceder; false (y muestra mensaje) si no.
        /// </summary>
        private bool VerificarInvitadosExisten()
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            var invitados = DatabaseManager.GetInvitados(EventoActual);
            if (invitados.Count == 0)
            {
                MessageBox.Show(
                    "No hay invitados registrados en este evento.\n\n" +
                    "Debes agregar invitados primero (sección 👥 Invitados) " +
                    "antes de poder usar la importación o plantilla de mesas.",
                    "Sin invitados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnImportarMesas_Click(object? sender, EventArgs e)
        {
            if (!VerificarInvitadosExisten()) return;

            using var dlg = new OpenFileDialog
            {
                Title  = "Seleccionar archivo Excel de Mesas",
                Filter = "Excel (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var (mesasLeidas, erroresLectura) = ExcelImporter.LeerMesasDeExcel(dlg.FileName);

            if (mesasLeidas.Count == 0)
            {
                string msg = "No se encontraron mesas en el archivo.";
                if (erroresLectura.Count > 0)
                    msg += "\n\n" + string.Join("\n", erroresLectura);
                MessageBox.Show(msg, "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int mesasCreadas = 0, invAsignados = 0;
            var erroresBD = new List<string>();

            var mesasExistentes = DatabaseManager.GetMesas(EventoActual!)
                .Select(m => m.Numero).ToHashSet();

            var invitadosPorNombre = DatabaseManager.GetInvitados(EventoActual!)
                .ToDictionary(i => i.Nombre.ToLowerInvariant(), i => i);

            foreach (var (mesa, nombres) in mesasLeidas)
            {
                try
                {
                    if (mesasExistentes.Contains(mesa.Numero))
                    { erroresBD.Add($"Mesa {mesa.Numero} ya existe → omitida."); continue; }

                    int mesaId = DatabaseManager.AgregarMesaConNumero(
                        EventoActual!, mesa.Numero, mesa.Capacidad, mesa.PosX, mesa.PosY);
                    mesasExistentes.Add(mesa.Numero);
                    mesasCreadas++;

                    int asignadosEnEsta = 0;
                    foreach (var nombre in nombres)
                    {
                        string key = nombre.ToLowerInvariant();
                        if (!invitadosPorNombre.TryGetValue(key, out Invitado? inv))
                        { erroresBD.Add($"Mesa {mesa.Numero}: '{nombre}' no encontrado."); continue; }
                        if (asignadosEnEsta >= mesa.Capacidad)
                        { erroresBD.Add($"Mesa {mesa.Numero}: capacidad llena, '{nombre}' no asignado."); continue; }

                        DatabaseManager.AsignarInvitadoMesa(EventoActual!, inv.Id, mesaId);
                        asignadosEnEsta++;
                        invAsignados++;
                    }
                }
                catch (Exception ex) { erroresBD.Add($"Mesa {mesa.Numero}: {ex.Message}"); }
            }

            // Refrescar inmediatamente
            CargarCanvas();
            CargarInvitados();

            var todos = new List<string>(erroresLectura);
            todos.AddRange(erroresBD);
            string resumen = $"Importación completada.\n\n" +
                             $"✔ Mesas creadas:       {mesasCreadas}\n" +
                             $"✔ Invitados asignados: {invAsignados}\n" +
                             $"⚠ Errores/omitidos:    {todos.Count}";
            if (todos.Count > 0) resumen += "\n\nDetalles:\n• " + string.Join("\n• ", todos);
            MessageBox.Show(resumen, "Importar Mesas", MessageBoxButtons.OK,
                todos.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void BtnDescargarPlantillaMesas_Click(object? sender, EventArgs e)
        {
            if (!VerificarInvitadosExisten()) return;

            using var dlg = new SaveFileDialog
            {
                Title    = "Guardar plantilla de Mesas",
                Filter   = "Excel (*.xlsx)|*.xlsx",
                FileName = "Plantilla_Mesas.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Pasa los invitados reales del evento para el desplegable
                var invitados = DatabaseManager.GetInvitados(EventoActual!);
                ExcelImporter.GenerarPlantillaMesas(dlg.FileName, invitados);

                MessageBox.Show(
                    "Plantilla guardada con 2 hojas:\n\n" +
                    "• Hoja 1 'Mesas':        NumeroMesa | Capacidad | PosX | PosY\n" +
                    "• Hoja 2 'Asignaciones': NumeroMesa | NombreInvitado\n\n" +
                    $"La columna 'NombreInvitado' incluye un desplegable\n" +
                    $"con los {invitados.Count} invitados registrados en el evento.",
                    "Plantilla lista", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


    // ══════════════════════════════════════════════════════════════════════════
    //  MesaPanel  —  tarjeta arrastrable en el canvas
    // ══════════════════════════════════════════════════════════════════════════
    public class MesaPanel : Panel
    {
        public Mesa Mesa { get; }

        // Eventos como nullable para silenciar CS8618
        public event Action?               Seleccionado;
        public event Action<int, int, int>? PosicionCambio;

        private readonly Label _lblTitulo;
        private readonly Label _lblInvitados;
        private readonly Label _lblCapacidad;
        private bool  _activa;
        private Point _dragScreenStart;
        private bool  _arrastrando;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Activa
        {
            get => _activa;
            set { _activa = value; ActualizarEstilo(); }
        }

        public MesaPanel(Mesa mesa)
        {
            Mesa      = mesa;
            Size      = new Size(170, 145);
            BackColor = Color.White;
            Cursor    = Cursors.SizeAll;

            _lblTitulo = new Label
            {
                Text      = $"Mesa {mesa.Numero}",
                Dock      = DockStyle.Top,
                Height    = 32,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(18, 30, 58),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblInvitados = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(50, 65, 90),
                Padding   = new Padding(6, 4, 6, 0),
                TextAlign = ContentAlignment.TopLeft
            };

            _lblCapacidad = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(90, 105, 135),
                BackColor = Color.FromArgb(232, 238, 252),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(_lblInvitados);
            Controls.Add(_lblCapacidad);
            Controls.Add(_lblTitulo);

            RefrescarContenido();
            ConfigurarArrastre();
        }

        public void RefrescarContenido()
        {
            _lblInvitados.Text =
                Mesa.Invitados.Count > 0
                    ? string.Join("\n", Mesa.Invitados.Select(i => "• " + i.Nombre))
                    : "(sin invitados)";
            _lblCapacidad.Text = $"{Mesa.Invitados.Count} / {Mesa.Capacidad}  personas";
        }

        private void ActualizarEstilo()
        {
            _lblTitulo.BackColor =
                _activa ? Color.FromArgb(45, 90, 178) : Color.FromArgb(18, 30, 58);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(
                _activa ? Color.FromArgb(45, 90, 178) : Color.FromArgb(195, 210, 235), 2f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void ConfigurarArrastre()
        {
            AttachTo(this);
            foreach (Control c in Controls)
            {
                AttachTo(c);
                foreach (Control cc in c.Controls) AttachTo(cc);
            }
        }

        private void AttachTo(Control c)
        {
            c.MouseDown += OnChildMouseDown;
            c.MouseMove += OnChildMouseMove;
            c.MouseUp   += OnChildMouseUp;
            c.Click     += OnChildClick;
        }

        private void OnChildClick(object? s, EventArgs e)      => Seleccionado?.Invoke();

        private void OnChildMouseDown(object? s, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _arrastrando     = true;
            _dragScreenStart = ((Control)s!).PointToScreen(e.Location);
        }

        private void OnChildMouseMove(object? s, MouseEventArgs e)
        {
            if (!_arrastrando) return;
            var screenNow = ((Control)s!).PointToScreen(e.Location);
            int dx = screenNow.X - _dragScreenStart.X;
            int dy = screenNow.Y - _dragScreenStart.Y;
            _dragScreenStart = screenNow;

            var parent = Parent;
            if (parent == null) return;

            int nx = Math.Max(0, Math.Min(Left + dx, parent.ClientSize.Width  - Width));
            int ny = Math.Max(0, Math.Min(Top  + dy, parent.ClientSize.Height - Height));
            Location = new Point(nx, ny);
        }

        private void OnChildMouseUp(object? s, MouseEventArgs e)
        {
            if (!_arrastrando) return;
            _arrastrando = false;
            PosicionCambio?.Invoke(Mesa.Id, Left, Top);
        }
    }
}