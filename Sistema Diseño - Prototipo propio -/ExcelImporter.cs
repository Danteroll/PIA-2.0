// ============================================================
//  ExcelImporter.cs
//  Requiere: EPPlus 7.x (NuGet)
// ============================================================
using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;

namespace GestionEventos
{
    public static class ExcelImporter
    {
        // ══════════════════════════════════════════════════════════════════════
        //  IMPORTAR INVITADOS
        //  Columnas: Nombre* | Teléfono | Grupo | Alergias | Confirmado
        // ══════════════════════════════════════════════════════════════════════
        public static (List<Invitado> invitados, List<string> errores)
            LeerInvitadosDeExcel(string excelPath)
        {
            var invitados = new List<Invitado>();
            var errores   = new List<string>();

            if (!File.Exists(excelPath))
            { errores.Add("El archivo no existe."); return (invitados, errores); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var pkg = new ExcelPackage(new FileInfo(excelPath));

            // Buscar por nombre primero, luego por encabezado como fallback
            var sheet = BuscarHoja(pkg, "invitados", "hoja1")
                     ?? BuscarHojaPorEncabezado(pkg, "nombre");

            if (sheet?.Dimension == null)
            { errores.Add("El archivo esta vacio o no se encontro la hoja de invitados."); return (invitados, errores); }

            var hdr = LeerEncabezados(sheet);
            if (!hdr.ContainsKey("nombre"))
            { errores.Add("Falta la columna 'Nombre'."); return (invitados, errores); }

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    string nombre = GetCelda(sheet, row, hdr, "nombre").Trim();
                    if (string.IsNullOrWhiteSpace(nombre)) continue;

                    invitados.Add(new Invitado
                    {
                        Nombre       = nombre,
                        Telefono     = GetCelda(sheet, row, hdr, "telefono", "telefono"),
                        Alergias     = GetCelda(sheet, row, hdr, "alergias", "alergia"),
                        Grupo        = GetCelda(sheet, row, hdr, "grupo"),
                        Confirmado   = ParseBool(GetCelda(sheet, row, hdr, "confirmado")),
                        Acompanantes = 0
                    });
                }
                catch (Exception ex) { errores.Add($"Fila {row}: {ex.Message}"); }
            }

            return (invitados, errores);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  IMPORTAR MESAS
        //  Hoja "Mesas":        NumeroMesa* | Capacidad | PosX | PosY
        //  Hoja "Asignaciones": NumeroMesa* | NombreInvitado*  (opcional)
        //
        //  VALIDACIONES:
        //   1. NumeroMesa en Asignaciones debe existir en hoja Mesas.
        //   2. No se pueden asignar mas invitados que la capacidad de la mesa.
        //   3. Un invitado no puede aparecer mas de una vez en toda la hoja.
        //
        //  IMPORTANTE: Las hojas se buscan por NOMBRE, no por indice.
        //  La plantilla contiene una hoja oculta "_Invitados" que queda ANTES
        //  de "Mesas" en el libro => Worksheets[0] es "_Invitados", no "Mesas".
        //  Buscar por indice fijo es la causa del error "No se encontraron mesas".
        // ══════════════════════════════════════════════════════════════════════
        public static (List<(Mesa mesa, List<string> nombres)> mesas, List<string> errores)
            LeerMesasDeExcel(string excelPath)
        {
            var resultado = new List<(Mesa, List<string>)>();
            var errores   = new List<string>();

            if (!File.Exists(excelPath))
            { errores.Add("El archivo no existe."); return (resultado, errores); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var pkg = new ExcelPackage(new FileInfo(excelPath));
            if (pkg.Workbook.Worksheets.Count == 0)
            { errores.Add("El archivo no tiene hojas."); return (resultado, errores); }

            // ── Buscar hoja Mesas por nombre; fallback por encabezado ─────────
            var shMesas = BuscarHoja(pkg, "mesas", "hoja1", "mesa")
                       ?? BuscarHojaPorEncabezado(pkg, "numeromesa", "numero");

            if (shMesas == null || shMesas.Dimension == null)
            { errores.Add("No se encontro la hoja 'Mesas' en el archivo."); return (resultado, errores); }

            var hMesas = LeerEncabezados(shMesas);
            if (!hMesas.ContainsKey("numeromesa") && !hMesas.ContainsKey("numero"))
            { errores.Add("Falta la columna 'NumeroMesa' en la hoja Mesas."); return (resultado, errores); }

            var dict = new Dictionary<int, (Mesa mesa, List<string> nombres)>();

            for (int row = 2; row <= shMesas.Dimension.End.Row; row++)
            {
                try
                {
                    string rawNum = GetCelda(shMesas, row, hMesas,
                                       "numeromesa", "numero", "mesa").Trim();
                    if (string.IsNullOrWhiteSpace(rawNum)) continue;

                    if (!int.TryParse(rawNum, out int numero))
                    { errores.Add($"Mesas fila {row}: NumeroMesa '{rawNum}' no es entero."); continue; }
                    if (dict.ContainsKey(numero))
                    { errores.Add($"Mesas fila {row}: Mesa {numero} duplicada."); continue; }

                    int cap  = ParseInt(GetCelda(shMesas, row, hMesas, "capacidad", "cap"), 10);
                    int posX = ParseInt(GetCelda(shMesas, row, hMesas, "posx"), 50);
                    int posY = ParseInt(GetCelda(shMesas, row, hMesas, "posy"), 50);

                    dict[numero] = (new Mesa
                    {
                        Numero    = numero,
                        Capacidad = Math.Max(1, cap),
                        PosX      = posX,
                        PosY      = posY
                    }, new List<string>());
                }
                catch (Exception ex) { errores.Add($"Mesas fila {row}: {ex.Message}"); }
            }

            // ── Buscar hoja Asignaciones por nombre ───────────────────────────
            var shAsig = BuscarHoja(pkg, "asignaciones", "asignacion", "hoja2");

            if (shAsig?.Dimension != null)
            {
                var hAsig   = LeerEncabezados(shAsig);
                bool tieneN = hAsig.ContainsKey("numeromesa") || hAsig.ContainsKey("numero");
                bool tieneI = hAsig.ContainsKey("nombreinvitado") ||
                              hAsig.ContainsKey("invitado") || hAsig.ContainsKey("nombre");

                if (tieneN && tieneI)
                {
                    var contadorPorMesa      = new Dictionary<int, int>();
                    var invitadosYaAsignados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    for (int row = 2; row <= shAsig.Dimension.End.Row; row++)
                    {
                        try
                        {
                            string rawNum = GetCelda(shAsig, row, hAsig,
                                               "numeromesa", "numero").Trim();
                            string invNom = GetCelda(shAsig, row, hAsig,
                                               "nombreinvitado", "invitado", "nombre").Trim();
                            if (string.IsNullOrWhiteSpace(rawNum) ||
                                string.IsNullOrWhiteSpace(invNom)) continue;

                            if (!int.TryParse(rawNum, out int num))
                            {
                                errores.Add($"Asignaciones fila {row}: NumeroMesa '{rawNum}' no es un entero valido.");
                                continue;
                            }

                            // VALIDACION 1: la mesa debe existir en hoja Mesas
                            if (!dict.ContainsKey(num))
                            {
                                errores.Add($"Asignaciones fila {row}: Mesa {num} no existe " +
                                            $"en la hoja 'Mesas'. Solo se permiten mesas definidas ahi.");
                                continue;
                            }

                            // VALIDACION 2: invitado duplicado (en cualquier mesa)
                            if (invitadosYaAsignados.Contains(invNom))
                            {
                                errores.Add($"Asignaciones fila {row}: '{invNom}' ya esta " +
                                            $"asignado a una mesa. Un invitado solo puede aparecer una vez.");
                                continue;
                            }

                            // VALIDACION 3: capacidad maxima de la mesa
                            int capacidad = dict[num].mesa.Capacidad;
                            contadorPorMesa.TryGetValue(num, out int yaAsignados);
                            if (yaAsignados >= capacidad)
                            {
                                errores.Add($"Asignaciones fila {row}: Mesa {num} tiene capacidad " +
                                            $"{capacidad} y ya alcanzo el limite. '{invNom}' no fue asignado.");
                                continue;
                            }

                            dict[num].nombres.Add(invNom);
                            contadorPorMesa[num] = yaAsignados + 1;
                            invitadosYaAsignados.Add(invNom);
                        }
                        catch (Exception ex) { errores.Add($"Asignaciones fila {row}: {ex.Message}"); }
                    }
                }
            }

            foreach (var kv in dict)
                resultado.Add((kv.Value.mesa, kv.Value.nombres));

            return (resultado, errores);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PLANTILLA INVITADOS
        // ══════════════════════════════════════════════════════════════════════
        public static void GenerarPlantillaInvitados(string destPath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Invitados");

            string[] hdrs = { "Nombre", "Telefono", "Grupo", "Alergias", "Confirmado" };
            EscribirEncabezados(ws, hdrs);

            ws.Cells[2, 1].Value = "Juan Perez";
            ws.Cells[2, 2].Value = "555-1234";
            ws.Cells[2, 3].Value = "Familia";
            ws.Cells[2, 4].Value = "Ninguna";
            ws.Cells[2, 5].Value = "Si";

            var val = ws.DataValidations.AddListValidation("E2:E1000");
            val.ShowErrorMessage = true;
            val.ErrorTitle       = "Valor invalido";
            val.Error            = "Use: Si o No";
            val.Formula.Values.Add("Si");
            val.Formula.Values.Add("No");

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            pkg.SaveAs(new FileInfo(destPath));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PLANTILLA MESAS
        //  Orden de hojas en el libro:
        //    1. _Invitados  (oculta) — fuente del desplegable de invitados
        //    2. Mesas                — el usuario define sus mesas aqui
        //    3. Asignaciones         — el usuario asigna invitados a mesas
        //
        //  El desplegable de NumeroMesa en Asignaciones usa un Named Range
        //  que apunta a Mesas!A2:A500, por lo que se actualiza automaticamente
        //  cuando el usuario agrega nuevas mesas en la hoja Mesas.
        // ══════════════════════════════════════════════════════════════════════
        public static void GenerarPlantillaMesas(string destPath,
            List<Invitado> invitadosExistentes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var pkg = new ExcelPackage();

            // ── Hoja oculta _Invitados ────────────────────────────────────────
            var wsNombres = pkg.Workbook.Worksheets.Add("_Invitados");
            wsNombres.Hidden = eWorkSheetHidden.VeryHidden;

            var nombresUnicos = new List<string>();
            var vistos        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var inv in invitadosExistentes)
            {
                if (vistos.Add(inv.Nombre))
                {
                    nombresUnicos.Add(inv.Nombre);
                    wsNombres.Cells[nombresUnicos.Count, 1].Value = inv.Nombre;
                }
            }

            // ── Hoja Mesas ────────────────────────────────────────────────────
            var wsMesas = pkg.Workbook.Worksheets.Add("Mesas");
            EscribirEncabezados(wsMesas, new[] { "NumeroMesa", "Capacidad", "PosX", "PosY" });

            // Formato entero explicito en col A — evita que EPPlus guarde como
            // double y que cell.Text quede vacio al releer el archivo
            wsMesas.Column(1).Style.Numberformat.Format = "0";
            wsMesas.Column(2).Style.Numberformat.Format = "0";
            wsMesas.Column(3).Style.Numberformat.Format = "0";
            wsMesas.Column(4).Style.Numberformat.Format = "0";

            wsMesas.Cells[2, 1].Value = 1;  wsMesas.Cells[2, 2].Value = 10;
            wsMesas.Cells[2, 3].Value = 30; wsMesas.Cells[2, 4].Value = 30;
            wsMesas.Cells[3, 1].Value = 2;  wsMesas.Cells[3, 2].Value = 8;
            wsMesas.Cells[3, 3].Value = 215; wsMesas.Cells[3, 4].Value = 30;

            wsMesas.Cells[5, 1].Value =
                "Los numeros de mesa aqui son los unicos validos en la hoja Asignaciones.";
            wsMesas.Cells[5, 1].Style.Font.Italic = true;
            wsMesas.Cells[5, 1, 5, 4].Merge = true;

            wsMesas.Cells[wsMesas.Dimension.Address].AutoFitColumns();

            // Named Range dinamico — col A de Mesas, cubre hasta 500 filas
            // Las celdas vacias son ignoradas por Excel en el desplegable.
            pkg.Workbook.Names.Add("ListaMesas", wsMesas.Cells["A2:A500"]);

            // ── Hoja Asignaciones ─────────────────────────────────────────────
            var wsAsig = pkg.Workbook.Worksheets.Add("Asignaciones");
            EscribirEncabezados(wsAsig, new[] { "NumeroMesa", "NombreInvitado" });

            wsAsig.Cells[1, 3].Value =
                "NumeroMesa debe existir en 'Mesas'. Cada invitado solo puede aparecer una vez.";
            wsAsig.Cells[1, 3].Style.Font.Italic = true;
            wsAsig.Cells[1, 3].Style.Font.Color
                .SetColor(System.Drawing.Color.FromArgb(180, 60, 20));

            // Formato entero en col A de Asignaciones
            wsAsig.Column(1).Style.Numberformat.Format = "0";

            // Filas de ejemplo
            if (nombresUnicos.Count >= 1) { wsAsig.Cells[2, 1].Value = 1; wsAsig.Cells[2, 2].Value = nombresUnicos[0]; }
            if (nombresUnicos.Count >= 2) { wsAsig.Cells[3, 1].Value = 1; wsAsig.Cells[3, 2].Value = nombresUnicos[1]; }
            if (nombresUnicos.Count >= 3) { wsAsig.Cells[4, 1].Value = 2; wsAsig.Cells[4, 2].Value = nombresUnicos[2]; }

            // Desplegable col A: Named Range (se actualiza al agregar mesas en Hoja Mesas)
            var dvMesa = wsAsig.DataValidations.AddListValidation("A2:A500");
            dvMesa.ShowErrorMessage = true;
            dvMesa.ErrorStyle       = ExcelDataValidationWarningStyle.stop;
            dvMesa.ErrorTitle       = "Mesa no valida";
            dvMesa.Error            =
                "Este numero de mesa no existe en la hoja 'Mesas'. " +
                "Registra la mesa ahi primero.";
            dvMesa.Formula.ExcelFormula = "ListaMesas";

            // Desplegable col B: invitados del evento
            if (nombresUnicos.Count > 0)
            {
                var dvList = wsAsig.DataValidations.AddListValidation("B2:B500");
                dvList.ShowErrorMessage = true;
                dvList.ErrorStyle       = ExcelDataValidationWarningStyle.stop;
                dvList.ErrorTitle       = "Invitado no valido";
                dvList.Error            = "Selecciona un invitado de la lista desplegable.";
                dvList.Formula.ExcelFormula = $"'_Invitados'!$A$1:$A${nombresUnicos.Count}";
            }

            wsAsig.Cells[wsAsig.Dimension.Address].AutoFitColumns();
            wsAsig.Column(3).Width = 72;

            pkg.SaveAs(new FileInfo(destPath));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS PRIVADOS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca una hoja por nombre (sin distincion de mayusculas ni acentos).
        /// Acepta varios nombres candidatos.
        /// </summary>
        private static ExcelWorksheet? BuscarHoja(ExcelPackage pkg, params string[] nombres)
        {
            foreach (var nombre in nombres)
            {
                string norm = Norm(nombre) ?? "";
                foreach (ExcelWorksheet ws in pkg.Workbook.Worksheets)
                {
                    if (Norm(ws.Name) == norm)
                        return ws;
                }
            }
            return null;
        }

        /// <summary>
        /// Fallback: devuelve la primera hoja visible cuya fila 1 contenga
        /// alguno de los encabezados indicados.
        /// </summary>
        private static ExcelWorksheet? BuscarHojaPorEncabezado(ExcelPackage pkg,
            params string[] encabezados)
        {
            foreach (ExcelWorksheet ws in pkg.Workbook.Worksheets)
            {
                if (ws.Hidden != eWorkSheetHidden.Visible) continue;
                if (ws.Dimension == null) continue;
                var hdrs = LeerEncabezados(ws);
                foreach (var enc in encabezados)
                    if (hdrs.ContainsKey(Norm(enc) ?? ""))
                        return ws;
            }
            return null;
        }

        private static Dictionary<string, int> LeerEncabezados(ExcelWorksheet ws)
        {
            var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (ws.Dimension == null) return d;
            for (int c = 1; c <= ws.Dimension.End.Column; c++)
            {
                // Encabezados son texto: .Text es fiable aqui
                string? v = Norm(ws.Cells[1, c].Text);
                if (!string.IsNullOrEmpty(v) && !d.ContainsKey(v)) d[v] = c;
            }
            return d;
        }

        private static string GetCelda(ExcelWorksheet ws, int row,
            Dictionary<string, int> hdr, params string[] aliases)
        {
            foreach (var a in aliases)
            {
                if (!hdr.TryGetValue(Norm(a) ?? "", out int c)) continue;
                return LeerCeldaComoTexto(ws.Cells[row, c]);
            }
            return "";
        }

        /// <summary>
        /// Lee el valor de una celda como string de forma robusta.
        ///
        /// Raiz del problema "no se encontraron mesas":
        ///   EPPlus carga celdas numericas sin formato explicito como double.
        ///   - cell.Text  => "" (numFmtId=0 sin formato asignado)
        ///   - cell.Value => double 1.0
        ///   - "1.0".ToString() => falla en int.TryParse
        ///
        /// Solucion: si el valor es double sin decimales reales, convertir a long.
        /// </summary>
        private static string LeerCeldaComoTexto(ExcelRangeBase cell)
        {
            if (cell.Value is null) return "";

            if (cell.Value is string s) return s.Trim();

            if (cell.Value is double d)
            {
                return d == Math.Truncate(d)
                    ? ((long)d).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (cell.Value is IConvertible conv)
            {
                try
                {
                    double dv = conv.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
                    return dv == Math.Truncate(dv)
                        ? ((long)dv).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        : dv.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { /* fallback */ }
            }

            string txt = cell.Text?.Trim() ?? "";
            return txt.Length > 0 ? txt : (cell.Value.ToString() ?? "").Trim();
        }

        private static string? Norm(string? s) =>
            s?.Trim()
              .Replace(" ","").Replace("a\u0301","a").Replace("e\u0301","e")
              .Replace("i\u0301","i").Replace("o\u0301","o").Replace("u\u0301","u")
              .Replace("\u00e1","a").Replace("\u00e9","e").Replace("\u00ed","i")
              .Replace("\u00f3","o").Replace("\u00fa","u").Replace("\u00f1","n")
              .ToLowerInvariant();

        private static bool ParseBool(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return false;
            return v.Trim().ToLowerInvariant() is "si" or "yes" or "true" or "1" or "x";
        }

        // Acepta "1", "1.0", "1,0" — robusto ante distintas culturas
        private static int ParseInt(string v, int def = 0)
        {
            v = v.Trim();
            if (int.TryParse(v, out int n)) return n;
            if (double.TryParse(v,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double d))
                return (int)Math.Round(d);
            return def;
        }

        private static void EscribirEncabezados(ExcelWorksheet ws, string[] hdrs)
        {
            for (int i = 0; i < hdrs.Length; i++)
            {
                ws.Cells[1, i+1].Value           = hdrs[i];
                ws.Cells[1, i+1].Style.Font.Bold = true;
                ws.Cells[1, i+1].Style.Fill.PatternType =
                    OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, i+1].Style.Fill.BackgroundColor
                    .SetColor(System.Drawing.Color.FromArgb(18, 30, 58));
                ws.Cells[1, i+1].Style.Font.Color
                    .SetColor(System.Drawing.Color.White);
            }
        }
    }
}