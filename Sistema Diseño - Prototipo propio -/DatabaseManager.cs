using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;

namespace GestionEventos
{
    public static class DatabaseManager
    {
        // Carpeta donde se guardan todos los .db
        private static readonly string DataFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventosData");
        private static readonly string MasterDb =
            Path.Combine(DataFolder, "master.db");

        // ─── Constructor estático ──────────────────────────────────────────────
        static DatabaseManager()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
            InicializarMaster();
        }

        private static SqliteConnection AbrirConexion(string path)
        {
            var conn = new SqliteConnection($"Data Source={path}");
            conn.Open();
            return conn;
        }

        // ─── MASTER DB ─────────────────────────────────────────────────────────
        private static void InicializarMaster()
        {
            using (var conn = AbrirConexion(MasterDb))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Eventos (
                        Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL UNIQUE,
                        Tipo   TEXT NOT NULL,
                        Fecha  TEXT NOT NULL
                    )";
                cmd.ExecuteNonQuery();
            }
        }

        // ─── EVENTOS ───────────────────────────────────────────────────────────
        public static List<Evento> GetEventos()
        {
            var lista = new List<Evento>();
            using (var conn = AbrirConexion(MasterDb))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT Id, Nombre, Tipo, Fecha FROM Eventos ORDER BY Fecha";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(new Evento
                        {
                            Id     = r.GetInt32(0),
                            Nombre = r.GetString(1),
                            Tipo   = r.GetString(2),
                            Fecha  = DateTime.Parse(r.GetString(3))
                        });
                }
            }
            return lista;
        }

        public static void CrearEvento(Evento ev)
        {
            // 1. Insertar en master
            using (var conn = AbrirConexion(MasterDb))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO Eventos (Nombre, Tipo, Fecha) VALUES (@n, @t, @f)";
                cmd.Parameters.AddWithValue("@n", ev.Nombre);
                cmd.Parameters.AddWithValue("@t", ev.Tipo);
                cmd.Parameters.AddWithValue("@f", ev.Fecha.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
            }

            // 2. Crear base de datos propia del evento
            string dbPath = GetEventDbPath(ev.Nombre);
            using (var conn = AbrirConexion(dbPath))
            {
                var tablas = new[]
                {
                    @"CREATE TABLE IF NOT EXISTS Invitados (
                        Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre       TEXT NOT NULL,
                        Telefono     TEXT,
                        Alergias     TEXT,
                        Grupo        TEXT,
                        Confirmado   INTEGER DEFAULT 0,
                        Acompanantes INTEGER DEFAULT 0
                    )",
                    @"CREATE TABLE IF NOT EXISTS Mesas (
                        Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        Numero    INTEGER,
                        Capacidad INTEGER DEFAULT 10,
                        PosX      INTEGER DEFAULT 50,
                        PosY      INTEGER DEFAULT 50
                    )",
                    @"CREATE TABLE IF NOT EXISTS InvitadosMesa (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvitadoId  INTEGER,
                        MesaId      INTEGER,
                        FOREIGN KEY(InvitadoId) REFERENCES Invitados(Id),
                        FOREIGN KEY(MesaId)     REFERENCES Mesas(Id)
                    )",
                    @"CREATE TABLE IF NOT EXISTS Menu (
                        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                        Entrada        TEXT DEFAULT '',
                        PlatilloFuerte TEXT DEFAULT '',
                        Postre         TEXT DEFAULT ''
                    )",
                    @"CREATE TABLE IF NOT EXISTS Bebidas (
                        Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Tipo   TEXT DEFAULT ''
                    )",
                    @"CREATE TABLE IF NOT EXISTS Menu (
                        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                        Entrada        TEXT NOT NULL DEFAULT '',
                        PlatilloFuerte TEXT NOT NULL DEFAULT '',
                        Postre         TEXT NOT NULL DEFAULT ''
                    )",
                    @"CREATE TABLE IF NOT EXISTS Bebidas (
                        Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Tipo   TEXT NOT NULL DEFAULT 'Otra'
                    )"
                };
                foreach (var sql in tablas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>Devuelve la ruta al .db del evento (nombre sanitizado).</summary>
        public static string GetEventDbPath(string nombreEvento)
        {
            string safe = string.Join(
                "_", nombreEvento.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(DataFolder, safe + ".db");
        }

        // ─── INVITADOS ─────────────────────────────────────────────────────────
        public static List<Invitado> GetInvitados(string eventoNombre)
        {
            var lista = new List<Invitado>();
            string path = GetEventDbPath(eventoNombre);
            if (!File.Exists(path)) return lista;

            using (var conn = AbrirConexion(path))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT Id,Nombre,Telefono,Alergias,Grupo,Confirmado,Acompanantes " +
                    "FROM Invitados ORDER BY Nombre";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(new Invitado
                        {
                            Id           = r.GetInt32(0),
                            Nombre       = r.GetString(1),
                            Telefono     = r.IsDBNull(2) ? "" : r.GetString(2),
                            Alergias     = r.IsDBNull(3) ? "" : r.GetString(3),
                            Grupo        = r.IsDBNull(4) ? "" : r.GetString(4),
                            Confirmado   = r.GetInt32(5) == 1,
                            Acompanantes = r.GetInt32(6)
                        });
                }
            }
            return lista;
        }

        public static void AgregarInvitado(string eventoNombre, Invitado inv)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO Invitados " +
                    "(Nombre,Telefono,Alergias,Grupo,Confirmado,Acompanantes) " +
                    "VALUES (@n,@t,@a,@g,@c,@ac)";
                cmd.Parameters.AddWithValue("@n",  inv.Nombre);
                cmd.Parameters.AddWithValue("@t",  inv.Telefono);
                cmd.Parameters.AddWithValue("@a",  inv.Alergias);
                cmd.Parameters.AddWithValue("@g",  inv.Grupo);
                cmd.Parameters.AddWithValue("@c",  inv.Confirmado ? 1 : 0);
                cmd.Parameters.AddWithValue("@ac", inv.Acompanantes);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EditarInvitado(string eventoNombre, Invitado inv)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "UPDATE Invitados SET Nombre=@n,Telefono=@t,Alergias=@a," +
                    "Grupo=@g,Confirmado=@c,Acompanantes=@ac WHERE Id=@id";
                cmd.Parameters.AddWithValue("@n",  inv.Nombre);
                cmd.Parameters.AddWithValue("@t",  inv.Telefono);
                cmd.Parameters.AddWithValue("@a",  inv.Alergias);
                cmd.Parameters.AddWithValue("@g",  inv.Grupo);
                cmd.Parameters.AddWithValue("@c",  inv.Confirmado ? 1 : 0);
                cmd.Parameters.AddWithValue("@ac", inv.Acompanantes);
                cmd.Parameters.AddWithValue("@id", inv.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarInvitado(string eventoNombre, int id)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM InvitadosMesa WHERE InvitadoId=@id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Invitados WHERE Id=@id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ─── MESAS ─────────────────────────────────────────────────────────────
        public static List<Mesa> GetMesas(string eventoNombre)
        {
            var lista = new List<Mesa>();
            string path = GetEventDbPath(eventoNombre);
            if (!File.Exists(path)) return lista;

            using (var conn = AbrirConexion(path))
            {
                // Cargar mesas
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT Id,Numero,Capacidad,PosX,PosY FROM Mesas ORDER BY Numero";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            lista.Add(new Mesa
                            {
                                Id        = r.GetInt32(0),
                                Numero    = r.GetInt32(1),
                                Capacidad = r.GetInt32(2),
                                PosX      = r.GetInt32(3),
                                PosY      = r.GetInt32(4)
                            });
                    }
                }

                // Cargar invitados por mesa
                foreach (var mesa in lista)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText =
                            "SELECT i.Id, i.Nombre FROM Invitados i " +
                            "JOIN InvitadosMesa im ON i.Id = im.InvitadoId " +
                            "WHERE im.MesaId = @mid";
                        cmd.Parameters.AddWithValue("@mid", mesa.Id);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                mesa.Invitados.Add(new Invitado
                                {
                                    Id     = r.GetInt32(0),
                                    Nombre = r.GetString(1)
                                });
                        }
                    }
                }
            }
            return lista;
        }

        public static int AgregarMesa(string eventoNombre, int capacidad, int posX, int posY)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                int numero;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COALESCE(MAX(Numero), 0) + 1 FROM Mesas";
                    numero = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO Mesas (Numero,Capacidad,PosX,PosY) " +
                        "VALUES (@n,@c,@x,@y)";
                    cmd.Parameters.AddWithValue("@n", numero);
                    cmd.Parameters.AddWithValue("@c", capacidad);
                    cmd.Parameters.AddWithValue("@x", posX);
                    cmd.Parameters.AddWithValue("@y", posY);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static void ActualizarPosMesa(string eventoNombre, int mesaId, int posX, int posY)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "UPDATE Mesas SET PosX=@x, PosY=@y WHERE Id=@id";
                cmd.Parameters.AddWithValue("@x",  posX);
                cmd.Parameters.AddWithValue("@y",  posY);
                cmd.Parameters.AddWithValue("@id", mesaId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void AsignarInvitadoMesa(
            string eventoNombre, int invitadoId, int mesaId)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                // Quitar de cualquier mesa previa
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "DELETE FROM InvitadosMesa WHERE InvitadoId=@iid";
                    cmd.Parameters.AddWithValue("@iid", invitadoId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO InvitadosMesa (InvitadoId, MesaId) " +
                        "VALUES (@iid, @mid)";
                    cmd.Parameters.AddWithValue("@iid", invitadoId);
                    cmd.Parameters.AddWithValue("@mid", mesaId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void QuitarInvitadoMesa(string eventoNombre, int invitadoId)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "DELETE FROM InvitadosMesa WHERE InvitadoId=@iid";
                cmd.Parameters.AddWithValue("@iid", invitadoId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarMesa(string eventoNombre, int mesaId)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "DELETE FROM InvitadosMesa WHERE MesaId=@mid";
                    cmd.Parameters.AddWithValue("@mid", mesaId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Mesas WHERE Id=@mid";
                    cmd.Parameters.AddWithValue("@mid", mesaId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ─── EDITAR EVENTO ─────────────────────────────────────────────────────
        /// <summary>
        /// Edita nombre, tipo y fecha de un evento en master.db.
        /// Si el nombre cambió también renombra el archivo .db del evento.
        /// </summary>
        public static void EditarEvento(int id, string nombreAnterior,
            string nuevoNombre, string nuevoTipo, DateTime nuevaFecha)
        {
            if (!string.Equals(nombreAnterior, nuevoNombre,
                    StringComparison.OrdinalIgnoreCase))
            {
                string oldPath = GetEventDbPath(nombreAnterior);
                string newPath = GetEventDbPath(nuevoNombre);
                if (File.Exists(oldPath) && !File.Exists(newPath))
                    File.Move(oldPath, newPath);
            }

            using (var conn = AbrirConexion(MasterDb))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "UPDATE Eventos SET Nombre=@n, Tipo=@t, Fecha=@f WHERE Id=@id";
                cmd.Parameters.AddWithValue("@n",  nuevoNombre);
                cmd.Parameters.AddWithValue("@t",  nuevoTipo);
                cmd.Parameters.AddWithValue("@f",  nuevaFecha.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ─── MENÚ ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Devuelve el menú del evento. Si aún no existe ninguno, devuelve un
        /// objeto vacío con Id = 0 para que la UI lo distinga de un menú guardado.
        /// </summary>
        public static Menu GetMenu(string eventoNombre)
        {
            var menu = new Menu();
            string path = GetEventDbPath(eventoNombre);
            if (!File.Exists(path)) return menu;

            using (var conn = AbrirConexion(path))
            {
                // Asegura que las tablas existen (eventos creados antes de esta versión)
                EnsureMenuTables(conn);

                // Leer fila de menú
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT Id, Entrada, PlatilloFuerte, Postre FROM Menu LIMIT 1";
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            menu.Id             = r.GetInt32(0);
                            menu.Entrada        = r.IsDBNull(1) ? "" : r.GetString(1);
                            menu.PlatilloFuerte = r.IsDBNull(2) ? "" : r.GetString(2);
                            menu.Postre         = r.IsDBNull(3) ? "" : r.GetString(3);
                        }
                    }
                }

                // Leer bebidas
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT Id, Nombre, Tipo FROM Bebidas ORDER BY Tipo, Nombre";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            menu.Bebidas.Add(new Bebida
                            {
                                Id     = r.GetInt32(0),
                                Nombre = r.GetString(1),
                                Tipo   = r.IsDBNull(2) ? "Otra" : r.GetString(2)
                            });
                    }
                }
            }
            return menu;
        }

        /// <summary>
        /// Guarda (INSERT o UPDATE) los platillos del menú.
        /// Las bebidas se gestionan por separado.
        /// </summary>
        public static void GuardarMenu(string eventoNombre, Menu menu)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                EnsureMenuTables(conn);

                bool existe = false;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Menu";
                    existe = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                using (var cmd = conn.CreateCommand())
                {
                    if (existe)
                    {
                        cmd.CommandText =
                            "UPDATE Menu SET Entrada=@e, PlatilloFuerte=@p, Postre=@po";
                    }
                    else
                    {
                        cmd.CommandText =
                            "INSERT INTO Menu (Entrada, PlatilloFuerte, Postre) " +
                            "VALUES (@e, @p, @po)";
                    }
                    cmd.Parameters.AddWithValue("@e",  menu.Entrada);
                    cmd.Parameters.AddWithValue("@p",  menu.PlatilloFuerte);
                    cmd.Parameters.AddWithValue("@po", menu.Postre);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ─── BEBIDAS ───────────────────────────────────────────────────────────
        public static void AgregarBebida(string eventoNombre, Bebida beb)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            {
                EnsureMenuTables(conn);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO Bebidas (Nombre, Tipo) VALUES (@n, @t)";
                    cmd.Parameters.AddWithValue("@n", beb.Nombre);
                    cmd.Parameters.AddWithValue("@t", beb.Tipo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void EliminarBebida(string eventoNombre, int bebidaId)
        {
            using (var conn = AbrirConexion(GetEventDbPath(eventoNombre)))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Bebidas WHERE Id=@id";
                cmd.Parameters.AddWithValue("@id", bebidaId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Crea las tablas Menu y Bebidas en bases de datos antiguas que no las tengan.
        /// </summary>
        private static void EnsureMenuTables(SqliteConnection conn)
        {
            var sqls = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Menu (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Entrada        TEXT NOT NULL DEFAULT '',
                    PlatilloFuerte TEXT NOT NULL DEFAULT '',
                    Postre         TEXT NOT NULL DEFAULT ''
                )",
                @"CREATE TABLE IF NOT EXISTS Bebidas (
                    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    Tipo   TEXT NOT NULL DEFAULT 'Otra'
                )"
            };
            foreach (var sql in sqls)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
