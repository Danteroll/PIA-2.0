using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    public class MenuControl : UserControl
    {
        // ── Controles ────────────────────────────────────────────────────────
        private ComboBox  cmbEventos;
        private ComboBox  cmbEntrada, cmbPlatilloFuerte, cmbPostre;
        private TextBox   txtNuevaBebida;
        private ComboBox  cmbTipoBebida;
        private ListBox   lstBebidas;
        private Button    btnGuardarMenu, btnAgregarBebida, btnEliminarBebida;
        private System.Windows.Forms.Timer _timerSync;

        private string EventoActual => cmbEventos.SelectedItem?.ToString();
        private Menu   _menuActual  = new Menu();

        // ── Catálogo de platillos ─────────────────────────────────────────────
        private static readonly string[] Entradas =
        {
            "— Sin entrada —",
            "Sopa de lima",
            "Sopa de tortilla",
            "Crema de elote",
            "Consomé de pollo",
            "Ensalada César",
            "Ensalada caprese",
            "Carpaccio de res",
            "Bruschetta italiana",
            "Ceviche de camarón",
            "Aguachile verde",
            "Coctel de camarón",
            "Tarta de queso brie",
            "Charcutería y quesos",
            "Dip de espinaca y alcachofa",
        };

        private static readonly string[] PlatillosFuertes =
        {
            "— Sin platillo fuerte —",
            "Filete de res al vino tinto",
            "Arrachera a la parrilla",
            "Costillas BBQ",
            "Pollo a la mostaza",
            "Pollo en salsa de champiñones",
            "Pechuga rellena de espinaca",
            "Salmón al limón con alcaparras",
            "Tilapia empapelada",
            "Camarones al ajillo",
            "Brochetas de mariscos",
            "Pasta Alfredo con pollo",
            "Pasta a la boloñesa",
            "Risotto de champiñones",
            "Lasaña de carne",
            "Enchiladas verdes",
            "Mole negro con pollo",
            "Birria de res",
            "Cordero al horno",
            "Pavo relleno",
            "Lomo de cerdo a las finas hierbas",
            "Cochinita pibil",
            "Opción vegetariana: Curry de garbanzos",
            "Opción vegetariana: Portobello relleno",
        };

        private static readonly string[] Postres =
        {
            "— Sin postre —",
            "Pastel de bodas (tradicional)",
            "Pastel de chocolate",
            "Pastel tres leches",
            "Cheesecake de fresa",
            "Cheesecake de mango",
            "Tiramisú",
            "Crème brûlée",
            "Panna cotta de vainilla",
            "Mousse de chocolate",
            "Flan napolitano",
            "Arroz con leche",
            "Profiteroles con chocolate",
            "Macarons surtidos",
            "Cupcakes decorados",
            "Fondue de chocolate con fresas",
            "Helado artesanal surtido",
            "Tarta de limón",
            "Brownies con nuez",
            "Mesa de dulces",
        };

        private static readonly string[] TiposBebida =
        {
            "Alcohólica", "Sin alcohol", "Vino", "Cerveza", "Cóctel", "Refresco", "Agua", "Otra"
        };

        public MenuControl()
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

            var lblTitulo = new Label
            {
                Text      = "🍴   Menú del Evento",
                Font      = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                Dock      = DockStyle.Left,
                Width     = 300,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblEvLbl = new Label
            {
                Text      = "Evento:",
                Dock      = DockStyle.Left,
                Width     = 68,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 75, 110),
                TextAlign = ContentAlignment.MiddleRight
            };

            cmbEventos = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width         = 260,
                Font          = new Font("Segoe UI", 10.5f),
                Dock          = DockStyle.Left
            };
            cmbEventos.SelectedIndexChanged += CmbEventos_Changed;

            pnlHeader.Controls.Add(cmbEventos);
            pnlHeader.Controls.Add(lblEvLbl);
            pnlHeader.Controls.Add(lblTitulo);

            // ── Cuerpo ───────────────────────────────────────────────────────
            // Dividido en dos columnas: platillos (izq) | bebidas (der)
            var pnlBody = new Panel { Dock = DockStyle.Fill };

            // ─ Panel derecho: bebidas ─────────────────────────────────────
            var pnlBebidas = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 320,
                BackColor = Color.White,
                Padding   = new Padding(14, 16, 14, 14)
            };
            pnlBebidas.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 225, 245)))
                    e.Graphics.DrawLine(pen, 0, 0, 0, ((Panel)s).Height);
            };

            var lblBebTit = new Label
            {
                Text      = "🥤  Bebidas",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                Dock      = DockStyle.Top,
                Height    = 34,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var sepB1 = new Panel { Dock = DockStyle.Top, Height = 6,  BackColor = Color.Transparent };

            // Fila: nombre + tipo + botón agregar
            var pnlAddBeb = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 38,
                BackColor = Color.Transparent
            };

            txtNuevaBebida = new TextBox
            {
                PlaceholderText = "Nombre de la bebida...",
                Font            = new Font("Segoe UI", 9.5f),
                Location        = new Point(0, 5),
                Width           = 150,
                BorderStyle     = BorderStyle.FixedSingle
            };

            cmbTipoBebida = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f),
                Location      = new Point(155, 5),
                Width         = 95
            };
            cmbTipoBebida.Items.AddRange(TiposBebida);
            cmbTipoBebida.SelectedIndex = 0;

            btnAgregarBebida = new Button
            {
                Text      = "+",
                Location  = new Point(255, 4),
                Size      = new Size(36, 28),
                BackColor = Color.FromArgb(18, 118, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnAgregarBebida.FlatAppearance.BorderSize = 0;
            btnAgregarBebida.Click += BtnAgregarBebida_Click;

            pnlAddBeb.Controls.AddRange(
                new Control[] { txtNuevaBebida, cmbTipoBebida, btnAgregarBebida });

            var sepB2 = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };

            lstBebidas = new ListBox
            {
                Dock                = DockStyle.Fill,
                Font                = new Font("Segoe UI", 10f),
                ScrollAlwaysVisible = true,
                BorderStyle         = BorderStyle.FixedSingle
            };

            btnEliminarBebida = new Button
            {
                Text      = "🗑  Eliminar bebida seleccionada",
                Dock      = DockStyle.Bottom,
                Height    = 36,
                BackColor = Color.FromArgb(185, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnEliminarBebida.FlatAppearance.BorderSize = 0;
            btnEliminarBebida.Click += BtnEliminarBebida_Click;

            // Apilar en pnlBebidas (DockStyle.Top = orden visual)
            pnlBebidas.Controls.Add(lstBebidas);          // Fill
            pnlBebidas.Controls.Add(btnEliminarBebida);   // Bottom
            pnlBebidas.Controls.Add(sepB2);
            pnlBebidas.Controls.Add(pnlAddBeb);
            pnlBebidas.Controls.Add(sepB1);
            pnlBebidas.Controls.Add(lblBebTit);

            // ─ Panel izquierdo: platillos ──────────────────────────────────
            var pnlPlatillos = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 20)
            };

            // Título platillos
            var lblPlatTit = new Label
            {
                Text      = "🍽️  Selección de Platillos",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                Dock      = DockStyle.Top,
                Height    = 34,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Cards de selección
            var pnlCards = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                Height        = 360,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                BackColor     = Color.Transparent
            };

            pnlCards.Controls.Add(MakePlatilloCard(
                "🥗  Entrada", Entradas, Color.FromArgb(56, 195, 164),
                out cmbEntrada));
            pnlCards.Controls.Add(MakePlatilloCard(
                "🍖  Platillo Fuerte", PlatillosFuertes, Color.FromArgb(72, 149, 239),
                out cmbPlatilloFuerte));
            pnlCards.Controls.Add(MakePlatilloCard(
                "🍰  Postre", Postres, Color.FromArgb(235, 87, 155),
                out cmbPostre));

            // Botón guardar menú
            btnGuardarMenu = new Button
            {
                Text      = "💾  Guardar Menú",
                Dock      = DockStyle.Top,
                Height    = 46,
                BackColor = Color.FromArgb(18, 30, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 14, 0, 0)
            };
            btnGuardarMenu.FlatAppearance.BorderSize = 0;
            btnGuardarMenu.Click += BtnGuardarMenu_Click;

            var sepGuardar = new Panel { Dock = DockStyle.Top, Height = 14, BackColor = Color.Transparent };

            pnlPlatillos.Controls.Add(btnGuardarMenu);
            pnlPlatillos.Controls.Add(sepGuardar);
            pnlPlatillos.Controls.Add(pnlCards);
            pnlPlatillos.Controls.Add(lblPlatTit);

            pnlBody.Controls.Add(pnlPlatillos);
            pnlBody.Controls.Add(pnlBebidas);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
        }

        // ─── Helper: card de un platillo ──────────────────────────────────────
        private Panel MakePlatilloCard(string titulo, string[] opciones,
            Color accent, out ComboBox cmb)
        {
            var card = new Panel
            {
                Width     = 500,
                Height    = 108,
                Margin    = new Padding(0, 0, 0, 12),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(accent, 3f))
                    e.Graphics.DrawRectangle(pen, 0, 0,
                        card.Width - 1, card.Height - 1);
            };

            // Franja de color izquierda
            var bar = new Panel
            {
                Width     = 6,
                Dock      = DockStyle.Left,
                BackColor = accent
            };

            var lblTit = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = accent,
                Location  = new Point(16, 10),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 11f),
                Location      = new Point(16, 42),
                Width         = 460,
                BackColor     = Color.FromArgb(245, 248, 255),
                FlatStyle     = FlatStyle.Flat
            };
            cmb.Items.AddRange(opciones);
            cmb.SelectedIndex = 0;

            var lblSub = new Label
            {
                Text      = $"{opciones.Length - 1} opciones disponibles",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(140, 155, 180),
                Location  = new Point(16, 78),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { bar, lblTit, cmb, lblSub });
            return card;
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

            if (!silencioso) CargarMenu();
        }

        private void CmbEventos_Changed(object sender, EventArgs e) => CargarMenu();

        private void CargarMenu()
        {
            if (EventoActual == null) return;

            _menuActual = DatabaseManager.GetMenu(EventoActual);

            // Platillos
            SetCombo(cmbEntrada,         _menuActual.Entrada,        Entradas);
            SetCombo(cmbPlatilloFuerte,  _menuActual.PlatilloFuerte, PlatillosFuertes);
            SetCombo(cmbPostre,          _menuActual.Postre,         Postres);

            // Bebidas
            lstBebidas.Items.Clear();
            foreach (var b in _menuActual.Bebidas)
                lstBebidas.Items.Add(b);
        }

        private static void SetCombo(ComboBox cmb, string valor, string[] lista)
        {
            int idx = Array.IndexOf(lista, valor);
            cmb.SelectedIndex = idx >= 0 ? idx : 0;
        }

        // ─── Botones ───────────────────────────────────────────────────────────
        private void BtnGuardarMenu_Click(object sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _menuActual.Entrada        = cmbEntrada.SelectedItem?.ToString()       ?? "";
            _menuActual.PlatilloFuerte = cmbPlatilloFuerte.SelectedItem?.ToString() ?? "";
            _menuActual.Postre         = cmbPostre.SelectedItem?.ToString()         ?? "";

            // Limpiar el "— Sin ... —" para no guardarlo como texto real
            if (_menuActual.Entrada.StartsWith("—"))        _menuActual.Entrada        = "";
            if (_menuActual.PlatilloFuerte.StartsWith("—")) _menuActual.PlatilloFuerte = "";
            if (_menuActual.Postre.StartsWith("—"))         _menuActual.Postre         = "";

            try
            {
                DatabaseManager.GuardarMenu(EventoActual, _menuActual);
                MessageBox.Show("✅  Menú guardado correctamente.", "Guardado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregarBebida_Click(object sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombre = txtNuevaBebida.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Escribe el nombre de la bebida.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNuevaBebida.Focus();
                return;
            }

            var beb = new Bebida
            {
                Nombre = nombre,
                Tipo   = cmbTipoBebida.SelectedItem?.ToString() ?? "Otra"
            };

            DatabaseManager.AgregarBebida(EventoActual, beb);
            txtNuevaBebida.Clear();
            cmbTipoBebida.SelectedIndex = 0;
            CargarMenu();
        }

        private void BtnEliminarBebida_Click(object sender, EventArgs e)
        {
            if (!(lstBebidas.SelectedItem is Bebida beb))
            {
                MessageBox.Show("Selecciona una bebida de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar '{beb.Nombre}' de la lista?",
                "Confirmar", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DatabaseManager.EliminarBebida(EventoActual, beb.Id);
                CargarMenu();
            }
        }
    }
}
