using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEventos
{
    public class InicioControl : UserControl
    {
        private FlowLayoutPanel            pnlEventos = null!;
        private Label                      lblVacio   = null!;
        private System.Windows.Forms.Timer _timer     = null!;

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

            pnlEventos = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                AutoScroll    = true,
                Padding       = new Padding(20, 24, 20, 20),
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

            Controls.Add(pnlEventos);
            Controls.Add(pnlHeader);
        }

        private void IniciarTimer()
        {
            _timer = new System.Windows.Forms.Timer { Interval = 60_000 };
            _timer.Tick += (_, __) => RefrescarEventos();
            _timer.Start();
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

            lblVacio.Visible = eventos.Count == 0;

            int idx = 0;
            foreach (var ev in eventos)
            {
                var card = new EventoCard(ev, idx++);
                card.SolicitarEdicion += AbrirEdicion;
                pnlEventos.Controls.Add(card);
            }
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
    }
}