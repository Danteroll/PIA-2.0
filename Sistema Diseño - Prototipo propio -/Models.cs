using System;
using System.Collections.Generic;

namespace GestionEventos
{
    public class Evento
    {
        public int      Id     { get; set; }
        public string   Nombre { get; set; }
        public string   Tipo   { get; set; }
        public DateTime Fecha  { get; set; }

        public bool   Pasado          => Fecha.Date < DateTime.Today;
        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy");

        public override string ToString() => Nombre;
    }

    public class Invitado
    {
        public int    Id          { get; set; }
        public string Nombre      { get; set; } = "";
        public string Telefono    { get; set; } = "";
        public string Alergias    { get; set; } = "";
        public string Grupo       { get; set; } = "";
        public bool   Confirmado  { get; set; }
        public int    Acompanantes{ get; set; }

        public override string ToString() =>
            Nombre + (Confirmado ? "  ✓" : "");
    }

    public class Mesa
    {
        public int             Id        { get; set; }
        public int             Numero    { get; set; }
        public int             Capacidad { get; set; }
        public int             PosX      { get; set; }
        public int             PosY      { get; set; }
        public List<Invitado>  Invitados { get; set; } = new List<Invitado>();

        public override string ToString() =>
            $"Mesa {Numero}  ({Invitados.Count}/{Capacidad})";
    }

    public class Menu
    {
        public int    Id             { get; set; }
        public string Entrada        { get; set; } = "";
        public string PlatilloFuerte { get; set; } = "";
        public string Postre         { get; set; } = "";
        public List<Bebida> Bebidas  { get; set; } = new List<Bebida>();
    }

    public class Bebida
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo   { get; set; } = "";   // Alcohólica / Sin alcohol / Otra

        public override string ToString() =>
            string.IsNullOrEmpty(Tipo) || string.Equals(Nombre, Tipo, StringComparison.OrdinalIgnoreCase)
                ? Nombre
                : $"{Nombre}  [{Tipo}]";
    }
}
