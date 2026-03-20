using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace GestionEventos
{
    public class MainForm : Form
    {
        // null! resuelve CS8618 en proyectos con Nullable habilitado
        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;
        private Button btnInicio = null!;
        private Button btnInvitados = null!;
        private Button btnMapaMesas = null!;
        private Button btnMenu = null!;
        private Button? _activeBtn;          // nullable: empieza sin selección

        private InicioControl ctrlInicio = null!;
        private InvitadosControl ctrlInvitados = null!;
        private MapaMesasControl ctrlMapaMesas = null!;
        private MenuControl ctrlMenu = null!;

        public MainForm()
        {
            BuildUI();
            CargarControles();
            Navigate(ctrlInicio, btnInicio);
        }


        // ─── Construcción de la UI ─────────────────────────────────────────────
        private void BuildUI()
        {
            Text = "Gestión de Eventos";
            Size = new Size(1260, 780);
            MinimumSize = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.FromArgb(235, 240, 250);

            // Sidebar
            pnlSidebar = new Panel
            {
                Width = 255,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(18, 30, 58)
            };

            var pnlLogo = new Panel
            {
                Height = 115,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(10, 20, 44)
            };
            pnlLogo.Paint += (s, e) =>
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(52, 80, 140)),
                    0, pnlLogo.Height - 3, pnlLogo.Width, 3);
            pnlLogo.Controls.Add(new Label
            {
                Text = "🎊 Gestión\n    de Eventos",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            });

            var flowNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 18, 0, 0)
            };

            btnInicio = MakeSideButton("🏠   Inicio");
            btnInvitados = MakeSideButton("👥   Invitados");
            btnMapaMesas = MakeSideButton("🍽️   Mapa de Mesas");
            btnMenu = MakeSideButton("🍴   Menú");

            flowNav.Controls.AddRange(
                new Control[] { btnInicio, btnInvitados, btnMapaMesas, btnMenu });

            pnlSidebar.Controls.Add(flowNav);
            pnlSidebar.Controls.Add(pnlLogo);

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 240, 250)
            };

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
        }

        private static Button MakeSideButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 255,
                Height = 54,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(168, 196, 235),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 56, 100);
            return b;
        }

        // ─── Controles ─────────────────────────────────────────────────────────
        private void CargarControles()
        {
            ctrlInicio = new InicioControl();
            ctrlInvitados = new InvitadosControl();
            ctrlMapaMesas = new MapaMesasControl();
            ctrlMenu = new MenuControl();

            foreach (UserControl c in new UserControl[]
                { ctrlInicio, ctrlInvitados, ctrlMapaMesas, ctrlMenu })
            {
                c.Dock = DockStyle.Fill;
                c.Visible = false;
                pnlContent.Controls.Add(c);
            }

            btnInicio.Click += (_, __) => Navigate(ctrlInicio, btnInicio);
            btnInvitados.Click += (_, __) => Navigate(ctrlInvitados, btnInvitados);
            btnMapaMesas.Click += (_, __) => Navigate(ctrlMapaMesas, btnMapaMesas);
            btnMenu.Click += (_, __) => Navigate(ctrlMenu, btnMenu);
        }

        private void InitializeComponent()
        {

        }

        // ─── Navegación ────────────────────────────────────────────────────────
        private void Navigate(UserControl ctrl, Button btn)
        {
            foreach (Control c in pnlContent.Controls)
                c.Visible = false;
            ctrl.Visible = true;

            if (_activeBtn != null)
            {
                _activeBtn.BackColor = Color.Transparent;
                _activeBtn.ForeColor = Color.FromArgb(168, 196, 235);
                _activeBtn.Font = new Font("Segoe UI", 11f);
            }

            btn.BackColor = Color.FromArgb(45, 78, 140);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _activeBtn = btn;

            if (ctrl is InicioControl ic) ic.RefrescarEventos();
            if (ctrl is InvitadosControl ivc) ivc.CargarEventos();
            if (ctrl is MapaMesasControl mmc) mmc.CargarEventos();
            if (ctrl is MenuControl mc) mc.CargarEventos();
        }
    }
}
