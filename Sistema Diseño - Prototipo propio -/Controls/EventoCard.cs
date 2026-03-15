using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEventos
{
    public class EventoCard : Panel
    {
        public  Evento  Evento { get; }

        /// <summary>Se dispara cuando el usuario hace clic en "Editar".</summary>
        public event Action<Evento> SolicitarEdicion;

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

        public EventoCard(Evento ev, int index)
        {
            Evento = ev;
            Build(index);
        }

        private void Build(int index)
        {
            Size      = new Size(235, 195);
            Margin    = new Padding(14);
            BackColor = Color.White;

            var baseColor = Colores[index % Colores.Length];

            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 8,
                BackColor = baseColor
            };

            int tipoIdx = Array.IndexOf(Tipos, Evento.Tipo);
            string emoji = tipoIdx >= 0 ? Emojis[tipoIdx] : "🎉";

            var lblEmoji = new Label
            {
                Text      = emoji,
                Font      = new Font("Segoe UI Emoji", 20f),
                Size      = new Size(50, 50),
                Location  = new Point(177, 14),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var lblNombre = new Label
            {
                Text      = Evento.Nombre,
                Font      = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 40, 75),
                Location  = new Point(12, 14),
                Size      = new Size(162, 50),
                BackColor = Color.Transparent
            };

            var lblTipo = new Label
            {
                Text      = Evento.Tipo,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = baseColor,
                Location  = new Point(12, 66),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            var lblFecha = new Label
            {
                Text      = "📅  " + Evento.FechaFormateada,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(80, 95, 120),
                Location  = new Point(12, 88),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            int dias = (Evento.Fecha.Date - DateTime.Today).Days;
            string diasStr = dias > 0  ? $"⏳ En {dias} día{(dias == 1 ? "" : "s")}" :
                             dias == 0 ? "🎉 ¡Es hoy!" :
                                         $"✔ Hace {-dias} día{(-dias == 1 ? "" : "s")}";
            Color diasColor = dias > 0  ? Color.FromArgb(20, 130, 60) :
                              dias == 0 ? Color.FromArgb(180, 60, 20) :
                                          Color.FromArgb(120, 130, 150);

            var lblDias = new Label
            {
                Text      = diasStr,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = diasColor,
                Location  = new Point(12, 110),
                AutoSize  = true,
                BackColor = Color.Transparent
            };

            // ── Botón Editar ────────────────────────────────────────────────
            var btnEditar = new Button
            {
                Text      = "✏️  Editar evento",
                Size      = new Size(211, 32),
                Location  = new Point(12, 148),
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += (_, __) => SolicitarEdicion?.Invoke(Evento);

            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 220, 235), 1.5f))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };

            Controls.AddRange(new Control[]
                { pnlTop, lblNombre, lblTipo, lblFecha, lblDias, lblEmoji, btnEditar });
        }
    }
}
