using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GestionEventos
{
    public class MenuControl : UserControl
    {
        private ComboBox cmbEventos = null!;
        private ComboBox cmbEntrada = null!;
        private ComboBox cmbPlatilloFuerte = null!;
        private ComboBox cmbPostre = null!;
        private TextBox txtNuevaBebida = null!;
        private ComboBox cmbTipoBebida = null!;
        private ListBox lstBebidas = null!;
        private Button btnGuardarMenu = null!;
        private Button btnAgregarBebida = null!;
        private Button btnEliminarBebida = null!;
        private Button btnValidarAlergias = null!;
        private Label lblResumen = null!;
        private Label lblEstado = null!;
        private System.Windows.Forms.Timer _timerSync = null!;

        private string? EventoActual => cmbEventos.SelectedItem?.ToString();
        private Menu _menuActual = new Menu();

        private static readonly string[] TiposBebida =
        {
            "Alcohólica", "Sin alcohol", "Vino", "Cerveza", "Cóctel", "Refresco", "Otra"
        };

        private static readonly Color BgApp = Color.FromArgb(235, 240, 250);
        private static readonly Color CardBg = Color.White;
        private static readonly Color Ink = Color.FromArgb(22, 38, 70);
        private static readonly Color InkSoft = Color.FromArgb(90, 105, 130);
        private static readonly Color Border = Color.FromArgb(215, 225, 245);
        private static readonly Color Primary = Color.FromArgb(18, 30, 58);
        private static readonly Color Green = Color.FromArgb(18, 118, 55);
        private static readonly Color Red = Color.FromArgb(185, 40, 40);
        private static readonly Color AccentEntrada = Color.FromArgb(56, 195, 164);
        private static readonly Color AccentFuerte = Color.FromArgb(72, 149, 239);
        private static readonly Color AccentPostre = Color.FromArgb(235, 87, 155);

        public MenuControl()
        {
            BuildUI();
            IniciarTimer();
        }

        private void BuildUI()
        {
            BackColor = BgApp;
            Dock = DockStyle.Fill;
            Padding = new Padding(20, 16, 20, 16);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var pnlHeader = CreateCard();
            pnlHeader.Padding = new Padding(18, 14, 18, 12);

            var headerGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                BackColor = Color.Transparent
            };
            headerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            headerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));

            var lblTitulo = new Label
            {
                Text = "Gestión del Menú",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Ink,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSub = new Label
            {
                Text = "Configura platillos, bebidas y valida alergias de invitados",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = InkSoft,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            var pnlHeaderLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            pnlHeaderLeft.Controls.Add(lblTitulo);
            pnlHeaderLeft.Controls.Add(lblSub);

            var lblEv = new Label
            {
                Text = "Evento:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Ink,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            cmbEventos = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f),
                Margin = new Padding(0, 18, 0, 18),
                DropDownWidth = 420
            };
            cmbEventos.SelectedIndexChanged += CmbEventos_Changed;

            headerGrid.Controls.Add(pnlHeaderLeft, 0, 0);
            headerGrid.Controls.Add(lblEv, 1, 0);
            headerGrid.Controls.Add(cmbEventos, 2, 0);
            pnlHeader.Controls.Add(headerGrid);

            var contentGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 14, 0, 0)
            };
            contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

            var leftCol = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            leftCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            leftCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));

            var pnlPlatillos = CreateCard();
            pnlPlatillos.Padding = new Padding(18, 16, 18, 18);

            var lblPlatTit = new Label
            {
                Text = "Selección de platillos",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Ink,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblPlatSub = new Label
            {
                Text = "Selecciona entrada, platillo fuerte y postre. También puedes administrar el catálogo.",
                Font = new Font("Segoe UI", 9.25f),
                ForeColor = InkSoft,
                AutoSize = true,
                Location = new Point(2, 28)
            };

            var cardsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 430,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            cardsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            cardsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            cardsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34f));

            string[] vacio = { "— Selecciona un platillo —" };
            cardsPanel.Controls.Add(MakePlatilloCard("Entrada", vacio, AccentEntrada, out cmbEntrada), 0, 0);
            cardsPanel.Controls.Add(MakePlatilloCard("Platillo Fuerte", vacio, AccentFuerte, out cmbPlatilloFuerte), 0, 1);
            cardsPanel.Controls.Add(MakePlatilloCard("Postre", vacio, AccentPostre, out cmbPostre), 0, 2);

            pnlPlatillos.Controls.Add(cardsPanel);
            pnlPlatillos.Controls.Add(lblPlatSub);
            pnlPlatillos.Controls.Add(lblPlatTit);

            var pnlResumen = CreateCard();
            pnlResumen.Padding = new Padding(18, 14, 18, 12);

            var lblResTit = new Label
            {
                Text = "Resumen del menú",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Ink,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblResumen = new Label
            {
                Text = "Selecciona un evento para ver el resumen.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = InkSoft,
                AutoSize = false,
                Location = new Point(2, 28),
                Size = new Size(900, 36)
            };

            pnlResumen.Controls.Add(lblResTit);
            pnlResumen.Controls.Add(lblResumen);

            var pnlAcciones = CreateCard();
            pnlAcciones.Padding = new Padding(14, 10, 14, 10);

            btnGuardarMenu = CreateActionButton("💾 Guardar menú", Primary, 170);
            btnGuardarMenu.Location = new Point(14, 12);
            btnGuardarMenu.Click += BtnGuardarMenu_Click;

            btnValidarAlergias = CreateActionButton("🧠 Validar alergias", Red, 190);
            btnValidarAlergias.Location = new Point(194, 12);
            btnValidarAlergias.Click += BtnValidarAlergias_Click;

            lblEstado = new Label
            {
                Text = "Listo para editar.",
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold),
                ForeColor = InkSoft,
                AutoSize = true,
                Location = new Point(402, 19)
            };

            pnlAcciones.Controls.Add(btnGuardarMenu);
            pnlAcciones.Controls.Add(btnValidarAlergias);
            pnlAcciones.Controls.Add(lblEstado);

            leftCol.Controls.Add(pnlPlatillos, 0, 0);
            leftCol.Controls.Add(pnlResumen, 0, 1);
            leftCol.Controls.Add(pnlAcciones, 0, 2);

            var pnlBebidas = CreateCard();
            pnlBebidas.Padding = new Padding(16, 16, 16, 16);

            var lblBebTit = new Label
            {
                Text = "Bebidas",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Ink,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblBebSub = new Label
            {
                Text = "Agrega y administra las bebidas del evento.",
                Font = new Font("Segoe UI", 9.25f),
                ForeColor = InkSoft,
                AutoSize = true,
                Location = new Point(2, 28)
            };

            var pnlBebWrap = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 520,
                BackColor = Color.Transparent
            };

            var pnlAddBeb = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 76,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            pnlAddBeb.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlAddBeb.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pnlAddBeb.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));

            txtNuevaBebida = new TextBox
            {
                PlaceholderText = "Nombre de la bebida...",
                Font = new Font("Segoe UI", 9.5f),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 8, 14),
                BorderStyle = BorderStyle.FixedSingle
            };

            cmbTipoBebida = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 8, 14),
                DropDownWidth = 220
            };
            cmbTipoBebida.Items.AddRange(TiposBebida);
            cmbTipoBebida.SelectedIndex = 0;

            btnAgregarBebida = new Button
            {
                Text = "+",
                Dock = DockStyle.Fill,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(0, 14, 0, 14),
                Cursor = Cursors.Hand
            };
            btnAgregarBebida.FlatAppearance.BorderSize = 0;
            btnAgregarBebida.Click += BtnAgregarBebida_Click;

            pnlAddBeb.Controls.Add(txtNuevaBebida, 0, 0);
            pnlAddBeb.Controls.Add(cmbTipoBebida, 1, 0);
            pnlAddBeb.Controls.Add(btnAgregarBebida, 2, 0);

            lstBebidas = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 360,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false
            };

            btnEliminarBebida = new Button
            {
                Text = "🗑 Eliminar bebida seleccionada",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminarBebida.FlatAppearance.BorderSize = 0;
            btnEliminarBebida.Click += BtnEliminarBebida_Click;

            pnlBebWrap.Controls.Add(btnEliminarBebida);
            pnlBebWrap.Controls.Add(CreateSpacer(10));
            pnlBebWrap.Controls.Add(lstBebidas);
            pnlBebWrap.Controls.Add(CreateSpacer(12));
            pnlBebWrap.Controls.Add(pnlAddBeb);

            pnlBebidas.Controls.Add(pnlBebWrap);
            pnlBebidas.Controls.Add(lblBebSub);
            pnlBebidas.Controls.Add(lblBebTit);

            contentGrid.Controls.Add(leftCol, 0, 0);
            contentGrid.Controls.Add(pnlBebidas, 1, 0);

            root.Controls.Add(pnlHeader, 0, 0);
            root.Controls.Add(contentGrid, 0, 1);

            Controls.Add(root);
        }

        private Panel MakePlatilloCard(string titulo, string[] opciones, Color accent, out ComboBox cmb)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = CardBg,
                Padding = new Padding(16, 14, 16, 14)
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Border, 1f);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using var brush = new SolidBrush(accent);
                e.Graphics.FillRectangle(brush, 0, 0, 6, card.Height);
            };

            string emoji = titulo == "Entrada" ? "🥗" : titulo == "Platillo Fuerte" ? "🍖" : "🍰";
            string tipoLimpio = titulo == "Platillo Fuerte" ? "Fuerte" : titulo;

            var lblTit = new Label
            {
                Text = $"{emoji}  {titulo}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(18, 10)
            };

            var lblSub = new Label
            {
                Text = $"{Math.Max(0, opciones.Length - 1)} opciones disponibles",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = InkSoft,
                AutoSize = true,
                Location = new Point(20, 32)
            };

            var btnAgregar = new Button
            {
                Text = "+ Agregar",
                Size = new Size(94, 28),
                Location = new Point(card.Width - 150, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(232, 237, 247),
                ForeColor = Ink,
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderColor = Border;
            btnAgregar.Click += (s, e) =>
            {
                if (EventoActual == null)
                {
                    MessageBox.Show("Selecciona un evento primero.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var form = new NuevoPlatilloForm(EventoActual, tipoLimpio);
                form.ShowDialog();
                CargarMenu();
            };

            var btnEliminar = new Button
            {
                Text = "Eliminar",
                Size = new Size(82, 28),
                Location = new Point(card.Width - 242, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(250, 235, 235),
                ForeColor = Red,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderColor = Color.FromArgb(240, 210, 210);
            btnEliminar.Click += (s, e) =>
            {
                if (EventoActual == null)
                {
                    MessageBox.Show("Selecciona un evento primero.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var form = new EliminarPlatilloForm(EventoActual, tipoLimpio);
                form.ShowDialog();
                CargarMenu();
            };

            var comboLocal = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(18, 62),
                Width = 620,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.FromArgb(245, 248, 255),
                FlatStyle = FlatStyle.Flat
            };

            comboLocal.Items.AddRange(opciones);
            comboLocal.SelectedIndex = 0;

            card.Resize += (_, __) =>
            {
                comboLocal.Width = Math.Max(220, card.Width - 36);
            };

            cmb = comboLocal;

            card.Controls.Add(lblTit);
            card.Controls.Add(lblSub);
            card.Controls.Add(btnEliminar);
            card.Controls.Add(btnAgregar);
            card.Controls.Add(cmb);
            return card;
        }

        private Panel CreateCard()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0)
            };
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(Border, 1f);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        private Button CreateActionButton(string text, Color color, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 40),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Panel CreateSpacer(int height)
        {
            return new Panel
            {
                Dock = DockStyle.Top,
                Height = height,
                BackColor = Color.Transparent
            };
        }

        private void IniciarTimer()
        {
            _timerSync = new System.Windows.Forms.Timer { Interval = 5000 };
            _timerSync.Tick += (_, __) => CargarEventos(true);
            _timerSync.Start();
        }

        public void CargarEventos(bool silencioso = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => CargarEventos(silencioso)));
                return;
            }

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

            if (!silencioso)
                CargarMenu();
        }

        private void CmbEventos_Changed(object? sender, EventArgs e) => CargarMenu();

        private void CargarMenu()
        {
            if (EventoActual == null) return;

            _menuActual = DatabaseManager.GetMenu(EventoActual);

            string[] catEntradas = DatabaseManager.ObtenerCatalogoPlatillos(EventoActual, "Entrada").ToArray();
            string[] catFuertes = DatabaseManager.ObtenerCatalogoPlatillos(EventoActual, "Fuerte").ToArray();
            string[] catPostres = DatabaseManager.ObtenerCatalogoPlatillos(EventoActual, "Postre").ToArray();

            cmbEntrada.Items.Clear();
            cmbEntrada.Items.Add("— Selecciona un platillo —");
            cmbEntrada.Items.AddRange(catEntradas);

            cmbPlatilloFuerte.Items.Clear();
            cmbPlatilloFuerte.Items.Add("— Selecciona un platillo —");
            cmbPlatilloFuerte.Items.AddRange(catFuertes);

            cmbPostre.Items.Clear();
            cmbPostre.Items.Add("— Selecciona un platillo —");
            cmbPostre.Items.AddRange(catPostres);

            ActualizarContador(cmbEntrada);
            ActualizarContador(cmbPlatilloFuerte);
            ActualizarContador(cmbPostre);

            SetCombo(cmbEntrada, _menuActual.Entrada?.Nombre ?? "", cmbEntrada.Items.Cast<string>().ToArray());
            SetCombo(cmbPlatilloFuerte, _menuActual.PlatilloFuerte?.Nombre ?? "", cmbPlatilloFuerte.Items.Cast<string>().ToArray());
            SetCombo(cmbPostre, _menuActual.Postre?.Nombre ?? "", cmbPostre.Items.Cast<string>().ToArray());

            lstBebidas.Items.Clear();
            foreach (var b in _menuActual.Bebidas)
                lstBebidas.Items.Add(b);

            lblEstado.Text = $"Editando menú de: {EventoActual}";
            lblEstado.ForeColor = InkSoft;
            ActualizarResumen();
        }

        private void ActualizarContador(ComboBox cmb)
        {
            if (cmb.Parent is null) return;

            foreach (Control c in cmb.Parent.Controls)
            {
                if (c is Label lbl && lbl.Text.Contains("opciones"))
                {
                    lbl.Text = $"{Math.Max(0, cmb.Items.Count - 1)} opciones disponibles";
                    break;
                }
            }
        }

        private static void SetCombo(ComboBox cmb, string valor, string[] lista)
        {
            if (string.IsNullOrEmpty(valor))
            {
                cmb.SelectedIndex = 0;
                return;
            }

            int idx = Array.FindIndex(lista, x => x == valor || x.StartsWith(valor + " ("));
            cmb.SelectedIndex = idx >= 0 ? idx : 0;
        }
        private void CargarCatalogoPlatillos()
        {
            lstPlatillosCatalogo.Items.Clear();
        }

        private void ActualizarResumen()
        {
            string entrada = TextoVisible(cmbEntrada);
            string fuerte = TextoVisible(cmbPlatilloFuerte);
            string postre = TextoVisible(cmbPostre);

            lblResumen.Text = $"Entrada: {entrada}   •   Platillo fuerte: {fuerte}   •   Postre: {postre}";
        }

        private static string TextoVisible(ComboBox cmb)
        {
            string txt = cmb.SelectedItem?.ToString() ?? "Sin selección";
            return txt.StartsWith("—") ? "Sin selección" : txt;
        }

        private void BtnGuardarMenu_Click(object? sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Func<ComboBox, string> limpiarNombre = cmb =>
            {
                string txt = cmb.SelectedItem?.ToString() ?? "";
                if (txt.StartsWith("—")) return "";
                int idx = txt.IndexOf(" (");
                return idx > 0 ? txt.Substring(0, idx) : txt;
            };

            _menuActual.Entrada = new Platillo { Nombre = limpiarNombre(cmbEntrada) };
            _menuActual.PlatilloFuerte = new Platillo { Nombre = limpiarNombre(cmbPlatilloFuerte) };
            _menuActual.Postre = new Platillo { Nombre = limpiarNombre(cmbPostre) };

            try
            {
                DatabaseManager.GuardarMenu(EventoActual, _menuActual);
                lblEstado.Text = "✅ Menú guardado correctamente.";
                lblEstado.ForeColor = Green;

                MessageBox.Show("✅ Menú guardado correctamente.", "Guardado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error al guardar el menú.";
                lblEstado.ForeColor = Red;

                MessageBox.Show("Error al guardar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregarBebida_Click(object? sender, EventArgs e)
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
                Tipo = cmbTipoBebida.SelectedItem?.ToString() ?? "Otra"
            };

            DatabaseManager.AgregarBebida(EventoActual, beb);
            txtNuevaBebida.Clear();
            cmbTipoBebida.SelectedIndex = 0;
            CargarMenu();

            lblEstado.Text = "✅ Bebida agregada correctamente.";
            lblEstado.ForeColor = Green;
        }

        private void BtnEliminarBebida_Click(object? sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!(lstBebidas.SelectedItem is Bebida beb))
            {
                MessageBox.Show("Selecciona una bebida de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar '{beb.Nombre}' de la lista?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DatabaseManager.EliminarBebida(EventoActual, beb.Id);
                CargarMenu();

                lblEstado.Text = "✅ Bebida eliminada.";
                lblEstado.ForeColor = Green;
            }
        }

        private void BtnValidarAlergias_Click(object? sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Func<ComboBox, string> limpiar = cmb =>
            {
                string txt = cmb.SelectedItem?.ToString() ?? "";
                if (txt.StartsWith("—")) return "";
                int idx = txt.IndexOf(" (");
                return idx > 0 ? txt.Substring(0, idx) : txt;
            };

            string entrada = limpiar(cmbEntrada);
            string fuerte = limpiar(cmbPlatilloFuerte);
            string postre = limpiar(cmbPostre);

            List<string> ingredientesMenu = new List<string>();

            Action<string> agregarIngredientes = nombrePlatillo =>
            {
                if (!string.IsNullOrEmpty(nombrePlatillo))
                {
                    var lista = DatabaseManager.ObtenerIngredientesDePlatillo(EventoActual, nombrePlatillo);
                    foreach (var ing in lista)
                        ingredientesMenu.Add(ing.Nombre.ToLower());
                }
            };

            agregarIngredientes(entrada);
            agregarIngredientes(fuerte);
            agregarIngredientes(postre);

            List<string> alergiasInvitados = DatabaseManager.ObtenerAlergiasDelEvento(EventoActual);

            List<string> peligrosDetectados = new List<string>();
            foreach (var alergia in alergiasInvitados)
            {
                foreach (var ingrediente in ingredientesMenu)
                {
                    if (ingrediente.Contains(alergia) || alergia.Contains(ingrediente))
                    {
                        if (!peligrosDetectados.Contains(alergia))
                            peligrosDetectados.Add(alergia);
                    }
                }
            }

            if (peligrosDetectados.Count > 0)
            {
                string lista = string.Join(", ", peligrosDetectados).ToUpper();
                lblEstado.Text = "⚠ Se detectaron conflictos de alergias.";
                lblEstado.ForeColor = Red;

                MessageBox.Show(
                    $"¡⚠️ ALERTA DE SALUD!\n\nEl menú seleccionado contiene ingredientes peligrosos que chocan con las alergias de tus invitados:\n\n❌ {lista}\n\nPor favor, cambia los platillos para evitar accidentes.",
                    "Validación Fallida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                lblEstado.Text = "✅ Menú validado sin conflictos.";
                lblEstado.ForeColor = Green;

                MessageBox.Show(
                    "✅ ¡MENÚ 100% SEGURO!\n\nNingún ingrediente del menú choca con las alergias registradas de tus invitados.",
                    "Validación Exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void BtnAgregarPlatillo_Click(object sender, EventArgs e)
        {
            if (EventoActual == null)
            {
                MessageBox.Show("Selecciona un evento primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombre = txtNuevoPlatillo.Text.Trim();
            string tipo = cmbTipoPlatilloNuevo.SelectedItem?.ToString() ?? "Otro";
            string ingredientesTexto = txtIngredientesPlatillo.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Escribe el nombre del platillo.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNuevoPlatillo.Focus();
                return;
            }

            try
            {
                int platilloId = DatabaseManager.AgregarPlatillo(EventoActual, new Platillo
                {
                    Nombre = nombre,
                    Tipo = tipo
                });

                if (!string.IsNullOrWhiteSpace(ingredientesTexto))
                {
                    var ingredientes = ingredientesTexto
                        .Split(',')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var ing in ingredientes)
                    {
                        int ingredienteId = DatabaseManager.AgregarIngrediente(EventoActual, ing);
                        DatabaseManager.AsignarIngredienteAPlatillo(EventoActual, platilloId, ingredienteId);
                    }
                }

                txtNuevoPlatillo.Clear();
                txtIngredientesPlatillo.Clear();
                cmbTipoPlatilloNuevo.SelectedIndex = 0;

                CargarCatalogoPlatillos();

                MessageBox.Show("✅ Platillo agregado correctamente.", "Listo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar platillo:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

