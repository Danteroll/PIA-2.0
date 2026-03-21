using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    public class InicioControl : UserControl
    {
        private FlowLayoutPanel            pnlEventos = null!;
        private Panel                      pnlRight   = null!;
        private Label                      lblVacio   = null!;
        private Label                      lblTotalEventosVal = null!;
        private Label                      lblProximosVal     = null!;
        private Label                      lblInvitadosVal    = null!;
        private Label                      lblConfirmadosVal  = null!;
        private ListBox                    lstAgenda          = null!;
        private System.Windows.Forms.Timer _timer     = null!;
        private System.Windows.Forms.Timer _animTimer = null!;
        private readonly List<Evento> _pendientesAnim = new List<Evento>();
        private int _animIndex;

        public InicioControl()
        {
            BuildUI();
            IniciarTimer();
            RefrescarEventos();
        }

        private void BuildUI()
        {
            BackColor = Color.FromArgb(235, 240, 250);
            Dock      = DockStyle.Fill;

            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = Color.White
            };
            pnlHeader.Paint += (s, e) =>
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(215, 225, 245)),
                    0, pnlHeader.Height - 1, pnlHeader.Width, 1);

            var lblTitulo = new Label
            {
                Text      = "🏠   Eventos activos",
                Font      = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 38, 70),
                Location  = new Point(22, 0),
                Size      = new Size(400, 68),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnNuevo = new Button
            {
                Text      = "+ Crear nuevo evento",
                Size      = new Size(200, 40),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.FromArgb(18, 30, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnNuevo.Location = new Point(0, 14);
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += BtnNuevo_Click;

            pnlHeader.Resize += (s, e) =>
                btnNuevo.Location =
                    new Point(pnlHeader.Width - btnNuevo.Width - 20, 14);

            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(btnNuevo);

            var pnlBody = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 240, 250)
            };

            pnlEventos = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                AutoScroll    = true,
                Padding       = new Padding(20, 20, 20, 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = Color.FromArgb(235, 240, 250)
            };

            lblVacio = new Label
            {
                Text      = "No hay eventos próximos.\n\n" +
                            "Haz clic en '+ Crear nuevo evento' para empezar.",
                Font      = new Font("Segoe UI", 13),
                ForeColor = Color.FromArgb(150, 165, 190),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                Visible   = false
            };
            pnlEventos.Controls.Add(lblVacio);

            var pnlLeft = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 240, 250)
            };

            var lblSub = new Label
            {
                Text      = "Panel de próximos eventos",
                Dock      = DockStyle.Top,
                Height    = 28,
                Padding   = new Padding(22, 8, 0, 0),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(88, 105, 136)
            };
            pnlLeft.Controls.Add(pnlEventos);
            pnlLeft.Controls.Add(lblSub);

            pnlRight = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(229, 235, 247),
                Padding   = new Padding(12, 14, 12, 14)
            };

            var lblDash = new Label
            {
                Text      = "Resumen rápido",
                Dock      = DockStyle.Top,
                Height    = 30,
                Font      = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 54, 99)
            };

            var metrics = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 180,
                ColumnCount = 2,
                RowCount    = 2,
                BackColor   = Color.Transparent
            };
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            metrics.Controls.Add(MakeMetricCard("Eventos", out lblTotalEventosVal, Color.FromArgb(61, 125, 224)), 0, 0);
            metrics.Controls.Add(MakeMetricCard("Próx. 7 días", out lblProximosVal, Color.FromArgb(28, 160, 110)), 1, 0);
            metrics.Controls.Add(MakeMetricCard("Invitados", out lblInvitadosVal, Color.FromArgb(169, 109, 46)), 0, 1);
            metrics.Controls.Add(MakeMetricCard("Confirmados", out lblConfirmadosVal, Color.FromArgb(168, 77, 172)), 1, 1);

            var sep = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };

            var agendaCard = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Padding   = new Padding(10)
            };
            agendaCard.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(Color.FromArgb(214, 223, 241)),
                    0, 0, agendaCard.Width - 1, agendaCard.Height - 1);

            var lblAgenda = new Label
            {
                Text      = "Agenda próxima",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 62, 106)
            };

            lstAgenda = new ListBox
            {
                Dock                = DockStyle.Fill,
                BorderStyle         = BorderStyle.None,
                Font                = new Font("Segoe UI", 9.5f),
                BackColor           = Color.White,
                ForeColor           = Color.FromArgb(48, 60, 88),
                IntegralHeight      = false,
                ScrollAlwaysVisible = true
            };

            agendaCard.Controls.Add(lstAgenda);
            agendaCard.Controls.Add(lblAgenda);

            pnlRight.Controls.Add(agendaCard);
            pnlRight.Controls.Add(sep);
            pnlRight.Controls.Add(metrics);
            pnlRight.Controls.Add(lblDash);

            pnlRight.Width = 330;
            pnlRight.Dock  = DockStyle.Right;

            pnlLeft.Dock = DockStyle.Fill;

            pnlBody.Controls.Add(pnlLeft);
            pnlBody.Controls.Add(pnlRight);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
        }

        private static Panel MakeMetricCard(string title, out Label valueLabel, Color accent)
        {
            var card = new Panel
            {
                Margin    = new Padding(6),
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Padding   = new Padding(10, 8, 10, 8)
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(
                    new Pen(Color.FromArgb(216, 225, 241)),
                    0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 4, card.Height);
            };

            var lblTitle = new Label
            {
                Text      = title,
                Dock      = DockStyle.Top,
                Height    = 24,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(87, 102, 133)
            };

            valueLabel = new Label
            {
                Text      = "0",
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 52, 97),
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(lblTitle);
            return card;
        }

        private void IniciarTimer()
        {
            _timer = new System.Windows.Forms.Timer { Interval = 60_000 };
            _timer.Tick += (_, __) => RefrescarEventos();
            _timer.Start();

            _animTimer = new System.Windows.Forms.Timer { Interval = 85 };
            _animTimer.Tick += (_, __) => AgregarSiguienteCardAnimada();
        }

        public void RefrescarEventos()
        {
            if (InvokeRequired) { Invoke(new Action(RefrescarEventos)); return; }

            var tarjetas = pnlEventos.Controls.OfType<EventoCard>().ToList();
            foreach (var t in tarjetas)
                pnlEventos.Controls.Remove(t);

            var eventos = DatabaseManager.GetEventos()
                .Where(e => !e.Pasado)
                .OrderBy(e => e.Fecha)
                .ToList();

            RefrescarDashboard(eventos);

            lblVacio.Visible = eventos.Count == 0;

            _animTimer.Stop();
            _pendientesAnim.Clear();
            _animIndex = 0;

            if (eventos.Count == 0)
                return;

            _pendientesAnim.AddRange(eventos);
            AgregarSiguienteCardAnimada();
            if (_pendientesAnim.Count > 0)
                _animTimer.Start();
        }

        private void AgregarSiguienteCardAnimada()
        {
            if (_pendientesAnim.Count == 0)
            {
                _animTimer.Stop();
                return;
            }

            var ev = _pendientesAnim[0];
            _pendientesAnim.RemoveAt(0);

            var card = new EventoCard(ev, _animIndex++);
            card.SolicitarEdicion += AbrirEdicion;
            pnlEventos.Controls.Add(card);

            if (_pendientesAnim.Count == 0)
                _animTimer.Stop();
        }

        private void RefrescarDashboard(List<Evento> eventos)
        {
            int totalEventos = eventos.Count;
            int proximos7 = eventos.Count(e =>
                e.Fecha.Date >= DateTime.Today && e.Fecha.Date <= DateTime.Today.AddDays(7));

            int invitados = 0;
            int confirmados = 0;
            foreach (var ev in eventos)
            {
                var invs = DatabaseManager.GetInvitados(ev.Nombre);
                invitados += invs.Count;
                confirmados += invs.Count(i => i.Confirmado);
            }

            lblTotalEventosVal.Text = totalEventos.ToString();
            lblProximosVal.Text     = proximos7.ToString();
            lblInvitadosVal.Text    = invitados.ToString();
            lblConfirmadosVal.Text  = confirmados.ToString();

            lstAgenda.Items.Clear();
            foreach (var ev in eventos.Take(10))
                lstAgenda.Items.Add($"{ev.Fecha:dd MMM}  •  {ev.Nombre}");

            if (lstAgenda.Items.Count == 0)
                lstAgenda.Items.Add("Sin eventos próximos");
        }

        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            using var dlg = new NuevoEventoForm();
            if (dlg.ShowDialog(this) == DialogResult.OK)
                RefrescarEventos();
        }

        private void AbrirEdicion(Evento ev)
        {
            using var dlg = new EditarEventoForm(ev);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                RefrescarEventos();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _animTimer?.Stop();
                _animTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}