using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GestionEventos
{
    public class EventoCard : Panel
    {
        public Evento Evento { get; }

        // nullable: se suscribe externamente; puede ser null antes de la suscripción
        public event Action<Evento>? SolicitarEdicion;

        private static readonly Color[] Colores =
        {
            Color.FromArgb(72,  149, 239),
            Color.FromArgb(240, 101, 101),
            Color.FromArgb(56,  195, 164),
            Color.FromArgb(132, 94,  247),
            Color.FromArgb(255, 159, 67),
            Color.FromArgb(50,  168, 82),
            Color.FromArgb(235, 87,  155),
            Color.FromArgb(30,  136, 229),
        };

        private static readonly string[] Tipos  =
            { "Boda","Cumpleaños","Bautizo","Quinceañera",
              "Graduación","Aniversario","Corporativo","Otro" };
        private static readonly string[] Emojis =
            { "💒","🎂","👶","👸","🎓","💑","💼","🎉" };

        private bool _hover;

        public EventoCard(Evento ev, int index)
        {
            Evento = ev;
            Build(index);
        }

        private void Build(int index)
        {
            Size      = new Size(250, 205);
            Margin    = new Padding(14);
            BackColor = Color.Transparent;
            DoubleBuffered = true;

            var baseColor = Colores[index % Colores.Length];

            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 7,
                BackColor = baseColor
            };

            int tipoIdx   = Array.IndexOf(Tipos, Evento.Tipo);
            string emoji  = tipoIdx >= 0 ? Emojis[tipoIdx] : "🎉";

            var lblEmoji = new Label
            {
                Text      = emoji,
                Font      = new Font("Segoe UI Emoji", 20f),
                Size      = new Size(50, 50),
                Location  = new Point(190, 14),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var lblNombre = new Label
            {
                Text      = Evento.Nombre,
                Font      = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 40, 75),
                Location  = new Point(14, 14),
                Size      = new Size(172, 52),
                BackColor = Color.Transparent
            };

            var lblTipo = new Label
            {
                Text      = Evento.Tipo,
                Font      = new Font("Segoe UI", 9.2f),
                ForeColor = baseColor,
                Location  = new Point(14, 69),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            var lblFecha = new Label
            {
                Text      = "📅  " + Evento.FechaFormateada,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(80, 95, 120),
                Location  = new Point(14, 92),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            int dias = (Evento.Fecha.Date - DateTime.Today).Days;
            string diasStr =
                dias > 0  ? $"⏳ En {dias} día{(dias == 1 ? "" : "s")}" :
                dias == 0 ? "🎉 ¡Es hoy!" :
                            $"✔ Hace {-dias} día{(-dias == 1 ? "" : "s")}";
            Color diasColor =
                dias > 0  ? Color.FromArgb(20, 130, 60) :
                dias == 0 ? Color.FromArgb(180, 60, 20) :
                            Color.FromArgb(120, 130, 150);

            var lblDias = new Label
            {
                Text      = diasStr,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = diasColor,
                Location  = new Point(14, 116),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            var btnEditar = new Button
            {
                Text      = "✏️  Editar evento",
                Size      = new Size(222, 34),
                Location  = new Point(14, 156),
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += (_, __) => SolicitarEdicion?.Invoke(Evento);
            btnEditar.MouseEnter += (_, __) => btnEditar.BackColor = ControlPaint.Dark(baseColor, 0.08f);
            btnEditar.MouseLeave += (_, __) => btnEditar.BackColor = baseColor;

            MouseEnter += (_, __) => { _hover = true; Invalidate(); };
            MouseLeave += (_, __) => { _hover = false; Invalidate(); };
            foreach (Control c in new Control[]
                { pnlTop, lblNombre, lblTipo, lblFecha, lblDias, lblEmoji, btnEditar })
            {
                c.MouseEnter += (_, __) => { _hover = true; Invalidate(); };
                c.MouseLeave += (_, __) =>
                {
                    if (!ClientRectangle.Contains(PointToClient(MousePosition)))
                    {
                        _hover = false;
                        Invalidate();
                    }
                };
            }

            Controls.AddRange(new Control[]
                { pnlTop, lblNombre, lblTipo, lblFecha, lblDias, lblEmoji, btnEditar });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var cardRect = new Rectangle(2, _hover ? 1 : 3, Width - 8, Height - 8);
            var shadowRect = new Rectangle(5, _hover ? 4 : 6, Width - 8, Height - 8);

            using (var shadowPath = Rounded(shadowRect, 16))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(_hover ? 40 : 28, 17, 38, 75)))
                e.Graphics.FillPath(shadowBrush, shadowPath);

            using (var cardPath = Rounded(cardRect, 16))
            using (var bgBrush = new SolidBrush(Color.White))
            using (var borderPen = new Pen(_hover
                ? Color.FromArgb(150, 196, 220, 255)
                : Color.FromArgb(210, 220, 235), 1.6f))
            {
                e.Graphics.FillPath(bgBrush, cardPath);
                e.Graphics.DrawPath(borderPen, cardPath);
            }
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}