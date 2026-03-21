using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace GestionEventos
{
    public class MainForm : Form
    {
        // null! resuelve CS8618 en proyectos con Nullable habilitado
        private Panel           pnlSidebar         = null!;
        private Panel           pnlContent         = null!;
        private Panel           pnlTopbar          = null!;
        private Panel           pnlLogo            = null!;
        private FlowLayoutPanel flowNav            = null!;
        private Panel           pnlActiveIndicator = null!;
        private Label           lblClock           = null!;
        private Label           lblTitle           = null!;
        private Label           lblSection         = null!;
        private Button          btnInicio          = null!;
        private Button          btnInvitados       = null!;
        private Button          btnMapaMesas       = null!;
        private Button          btnMenu            = null!;
        private Button? _activeBtn; // nullable: empieza sin selección
        private UserControl? _currentCtrl;

        private System.Windows.Forms.Timer _clockTimer = null!;
        private System.Windows.Forms.Timer _indicatorAnimTimer = null!;
        private int _indicatorTargetTop;
        private System.Windows.Forms.Timer _transitionTimer = null!;
        private UserControl? _transitionCtrl;

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
            Text          = "Gestión de Eventos";
            Size          = new Size(1340, 820);
            MinimumSize   = new Size(1040, 680);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState   = FormWindowState.Maximized;
            Font          = new Font("Segoe UI", 9.5f);
            BackColor     = Color.FromArgb(232, 237, 247);

            // Sidebar
            pnlSidebar = new Panel
            {
                Width     = 272,
                Dock      = DockStyle.Left,
                BackColor = Color.FromArgb(9, 28, 65)
            };

            pnlLogo = new Panel
            {
                Height    = 136,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(5, 21, 53)
            };
            pnlLogo.Paint += (s, e) =>
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(73, 136, 238)),
                    0, pnlLogo.Height - 4, pnlLogo.Width, 4);
            pnlLogo.Controls.Add(new Label
            {
                Text      = "Gestión\nde Eventos",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI Semibold", 21f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            });

            pnlActiveIndicator = new Panel
            {
                Width     = 4,
                Height    = 54,
                BackColor = Color.FromArgb(95, 164, 255),
                Location  = new Point(0, pnlLogo.Bottom),
                Visible   = false
            };

            flowNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                Padding       = new Padding(0, 18, 0, 0),
                BackColor     = Color.Transparent
            };

            btnInicio    = MakeSideButton("Inicio");
            btnInvitados = MakeSideButton("Invitados");
            btnMapaMesas = MakeSideButton("Mapa de Mesas");
            btnMenu      = MakeSideButton("Menú");

            flowNav.Controls.AddRange(
                new Control[] { btnInicio, btnInvitados, btnMapaMesas, btnMenu });

            pnlTopbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 84,
                BackColor = Color.White,
                Padding   = new Padding(24, 0, 24, 0)
            };
            pnlTopbar.Paint += (s, e) =>
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(219, 227, 242)),
                    0, pnlTopbar.Height - 1, pnlTopbar.Width, 1);

            lblTitle = new Label
            {
                Text      = "Panel de Control",
                ForeColor = Color.FromArgb(17, 48, 99),
                Font      = new Font("Segoe UI Semibold", 15.5f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(24, 20)
            };

            lblSection = new Label
            {
                Text      = "Resumen general",
                ForeColor = Color.FromArgb(94, 110, 140),
                Font      = new Font("Segoe UI", 10.5f),
                AutoSize  = true,
                Location  = new Point(28, 52)
            };

            lblClock = new Label
            {
                ForeColor = Color.FromArgb(82, 99, 133),
                Font      = new Font("Segoe UI", 10f),
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlTopbar.Resize += (_, __) => PositionTopbarItems();

            pnlTopbar.Controls.Add(lblTitle);
            pnlTopbar.Controls.Add(lblSection);
            pnlTopbar.Controls.Add(lblClock);

            pnlSidebar.Controls.Add(flowNav);
            pnlSidebar.Controls.Add(pnlActiveIndicator);
            pnlSidebar.Controls.Add(pnlLogo);

            pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(232, 237, 247)
            };

            var pnlMain = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(232, 237, 247)
            };
            pnlMain.Controls.Add(pnlContent);
            pnlMain.Controls.Add(pnlTopbar);

            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);

            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (_, __) => ActualizarReloj();
            _clockTimer.Start();

            _indicatorAnimTimer = new System.Windows.Forms.Timer { Interval = 12 };
            _indicatorAnimTimer.Tick += (_, __) => AnimarIndicador();

            _transitionTimer = new System.Windows.Forms.Timer { Interval = 14 };
            _transitionTimer.Tick += (_, __) => AnimarTransicion();

            ActualizarReloj();
        }

        private static Button MakeSideButton(string text)
        {
            var b = new Button
            {
                Text      = text,
                Width     = 272,
                Height    = 54,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(184, 208, 246),
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 12f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(28, 0, 0, 0),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize        = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, 57, 113);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(26, 66, 132);
            return b;
        }

        private void ActualizarReloj()
        {
            lblClock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy  •  HH:mm:ss");
            PositionTopbarItems();
        }

        private void PositionTopbarItems()
        {
            lblClock.Location = new Point(
                pnlTopbar.Width - lblClock.Width - 24,
                (pnlTopbar.Height - lblClock.Height) / 2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clockTimer?.Stop();
                _clockTimer?.Dispose();
                _indicatorAnimTimer?.Stop();
                _indicatorAnimTimer?.Dispose();
                _transitionTimer?.Stop();
                _transitionTimer?.Dispose();
            }
            base.Dispose(disposing);
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
            if (_currentCtrl != ctrl)
                MostrarConTransicion(ctrl);

            if (_activeBtn != null)
            {
                _activeBtn.BackColor = Color.Transparent;
                _activeBtn.ForeColor = Color.FromArgb(184, 208, 246);
                _activeBtn.Font      = new Font("Segoe UI", 12f);
            }

            btn.BackColor = Color.FromArgb(40, 83, 157);
            btn.ForeColor = Color.White;
            btn.Font      = new Font("Segoe UI Semibold", 12f, FontStyle.Bold);
            _activeBtn    = btn;

            pnlActiveIndicator.Visible = true;
            pnlActiveIndicator.Height  = btn.Height;
            _indicatorTargetTop = btn.Top + flowNav.Top;
            if (!_indicatorAnimTimer.Enabled)
                _indicatorAnimTimer.Start();

            lblSection.Text = btn.Text;

            if (ctrl is InicioControl    ic)  ic.RefrescarEventos();
            if (ctrl is InvitadosControl ivc) ivc.CargarEventos();
            if (ctrl is MapaMesasControl mmc) mmc.CargarEventos();
            if (ctrl is MenuControl mc) mc.CargarEventos();
        }

        private void MostrarConTransicion(UserControl ctrl)
        {
            foreach (Control c in pnlContent.Controls)
                c.Visible = false;

            _transitionCtrl = ctrl;
            _currentCtrl = ctrl;

            ctrl.Dock = DockStyle.None;
            ctrl.Bounds = new Rectangle(
                Math.Max(24, pnlContent.Width / 8),
                0,
                pnlContent.Width,
                pnlContent.Height);
            ctrl.Visible = true;
            ctrl.BringToFront();

            if (!_transitionTimer.Enabled)
                _transitionTimer.Start();
        }

        private void AnimarTransicion()
        {
            if (_transitionCtrl == null)
            {
                _transitionTimer.Stop();
                return;
            }

            int x = _transitionCtrl.Left;
            if (x <= 0)
            {
                _transitionCtrl.Left = 0;
                _transitionCtrl.Dock = DockStyle.Fill;
                _transitionCtrl = null;
                _transitionTimer.Stop();
                return;
            }

            int step = Math.Max(10, x / 4);
            _transitionCtrl.Left = Math.Max(0, x - step);
        }

        private void AnimarIndicador()
        {
            int delta = _indicatorTargetTop - pnlActiveIndicator.Top;
            if (Math.Abs(delta) <= 1)
            {
                pnlActiveIndicator.Top = _indicatorTargetTop;
                _indicatorAnimTimer.Stop();
                return;
            }

            pnlActiveIndicator.Top += Math.Sign(delta) * Math.Max(2, Math.Abs(delta) / 4);
        }
    }
}
