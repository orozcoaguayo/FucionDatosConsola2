using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FucionDatosConsola2
{
    class Program
    {
        static string connectionString = "Server=localhost\\SQLEXPRESS;Database=FucionDatosDB;Trusted_Connection=True;TrustServerCertificate=True;";
        static List<DataItem> lista = new List<DataItem>();

        static string rutaCarpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Datos_Proyecto");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("====================================================");
                Console.WriteLine("       DATA FUSION ARENA - SISTEMA INTEGRADO        ");
                Console.WriteLine("====================================================");
                Console.WriteLine("1.  Cargar desde SQL Server");
                Console.WriteLine("2.  Mostrar Tabla");
                Console.WriteLine("3.  Ver Gráfica");
                Console.WriteLine("4.  Ver Resumen");
                Console.WriteLine("5.  Filtrar > 50");
                Console.WriteLine("6.  Ordenar (Bubble Sort)");
                Console.WriteLine("7.  Insertar en SQL");
                Console.WriteLine("8.  Detectar Duplicados");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("9.  Generar 10,000 datos");
                Console.WriteLine("10. Cargar CSV");
                Console.WriteLine("11. Cargar JSON");
                Console.WriteLine("12. Limpiar lista");
                Console.WriteLine("0.  Salir");
                Console.WriteLine("====================================================");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1": CargarDesdeBD(); break;
                    case "2": MostrarTabla(lista); break;
                    case "3": MostrarGrafica(); break;
                    case "4": MostrarResumen(); break;
                    case "5": MostrarTabla(Filtrar(50)); break;
                    case "6": Ordenar(); break;
                    case "7": MenuInsertar(); break;
                    case "8": DetectarDuplicados(); break;
                    case "9": GenerarArchivosMasivos(); break;
                    case "10": CargarDesdeCSV(); break;
                    case "11": CargarDesdeJSON(); break;
                    case "12": lista.Clear(); Console.WriteLine("Lista vaciada"); break;
                    case "0": salir = true; break;
                }

                Console.WriteLine("\nPresiona una tecla...");
                Console.ReadKey();
            }
        }

        // ===== MODELO =====
        public class DataItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public double Value { get; set; }
        }

        // ===== SQL =====
        static void CargarDesdeBD()
        {
            try
            {
                lista.Clear(); // 🔥 IMPORTANTE

                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                string query = "SELECT Id, Name, Category, Value FROM DataItems";
                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                int contador = 0;

                while (reader.Read())
                {
                    lista.Add(new DataItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = reader.IsDBNull(2) ? "Sin categoría" : reader.GetString(2),
                        Value = reader.GetDouble(3)
                    });
                    contador++;
                }

                Console.WriteLine($"✔ {contador} registros cargados desde SQL");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SQL: " + ex.Message);
            }
        }

        // ===== GENERAR ARCHIVOS =====
        static void GenerarArchivosMasivos()
        {
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            int cantidad = 10000;
            string[] cats = { "Tecnologia", "Muebles", "Papeleria" };
            string[] prods = { "Monitor", "Silla", "Lapiz" };

            Random rnd = new Random();

            string rutaCSV = Path.Combine(rutaCarpeta, "datos_masivos.csv");
            StringBuilder csv = new StringBuilder("Nombre,Categoria,Valor\n");

            for (int i = 1; i <= cantidad; i++)
                csv.AppendLine($"{prods[rnd.Next(3)]} {i},{cats[rnd.Next(3)]},{rnd.Next(100, 5000)}");

            File.WriteAllText(rutaCSV, csv.ToString());

            string rutaJSON = Path.Combine(rutaCarpeta, "datos_masivos.json");
            var listaJson = new List<DataItem>();

            for (int i = 1; i <= cantidad; i++)
                listaJson.Add(new DataItem
                {
                    Name = "Item " + i,
                    Category = cats[rnd.Next(3)],
                    Value = rnd.Next(100, 5000)
                });

            File.WriteAllText(rutaJSON, JsonSerializer.Serialize(listaJson, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine("✔ Archivos generados");
        }

        // ===== CSV =====
        static void CargarDesdeCSV()
        {
            string ruta = Path.Combine(rutaCarpeta, "datos_masivos.csv");

            if (!File.Exists(ruta))
            {
                Console.WriteLine("No existe CSV");
                return;
            }

            foreach (var linea in File.ReadAllLines(ruta).Skip(1))
            {
                var c = linea.Split(',');

                if (c.Length < 3) continue;

                lista.Add(new DataItem
                {
                    Name = c[0],
                    Category = c[1],
                    Value = double.Parse(c[2])
                });
            }

            Console.WriteLine("CSV cargado");
        }

        // ===== JSON =====
        static void CargarDesdeJSON()
        {
            string ruta = Path.Combine(rutaCarpeta, "datos_masivos.json");

            if (!File.Exists(ruta))
            {
                Console.WriteLine("No existe JSON");
                return;
            }

            var datos = JsonSerializer.Deserialize<List<DataItem>>(File.ReadAllText(ruta));
            lista.AddRange(datos);

            Console.WriteLine("JSON cargado");
        }

        // ===== PROCESAMIENTO =====
        static void Ordenar()
        {
            for (int i = 0; i < lista.Count - 1; i++)
                for (int j = i + 1; j < lista.Count; j++)
                    if (lista[i].Value > lista[j].Value)
                    {
                        var temp = lista[i];
                        lista[i] = lista[j];
                        lista[j] = temp;
                    }

            Console.WriteLine("Ordenado");
        }

        static List<DataItem> Filtrar(double min)
        {
            return lista.Where(x => x.Value > min).ToList();
        }

        static void DetectarDuplicados()
        {
            var dic = new Dictionary<string, int>();

            foreach (var d in lista)
            {
                if (!dic.ContainsKey(d.Name)) dic[d.Name] = 0;
                dic[d.Name]++;
            }

            foreach (var x in dic.Where(x => x.Value > 1))
                Console.WriteLine($"Duplicado: {x.Key} ({x.Value})");
        }

        // ===== VISUAL =====
        static void MostrarTabla(List<DataItem> datos)
        {
            if (datos.Count == 0)
            {
                Console.WriteLine("No hay datos para mostrar.");
                return;
            }

            var muestra = datos.Take(20).ToList();

            int colId = Math.Max(4, muestra.Max(d => d.Id.ToString().Length));
            int colNombre = Math.Max(6, muestra.Max(d => (d.Name ?? "").Length));
            int colCategoria = Math.Max(9, muestra.Max(d => (d.Category ?? "").Length));
            int colValor = Math.Max(10, muestra.Max(d => d.Value.ToString("C0").Length));

            string lineaHorizontal = $"+{new string('-', colId + 2)}+{new string('-', colNombre + 2)}+{new string('-', colCategoria + 2)}+{new string('-', colValor + 2)}+";

            Console.WriteLine(lineaHorizontal);
            Console.WriteLine($"| {"ID".PadRight(colId)} | {"NOMBRE".PadRight(colNombre)} | {"CATEGORIA".PadRight(colCategoria)} | {"VALOR".PadLeft(colValor)} |");
            Console.WriteLine(lineaHorizontal);

            foreach (var d in muestra)
            {
                string id = d.Id.ToString().PadRight(colId);
                string nombre = (d.Name ?? "").PadRight(colNombre);
                string categoria = (d.Category ?? "").PadRight(colCategoria);
                string valor = d.Value.ToString("C0").PadLeft(colValor);

                Console.WriteLine($"| {id} | {nombre} | {categoria} | {valor} |");
            }

            Console.WriteLine(lineaHorizontal);
            Console.WriteLine($"Mostrando {muestra.Count} de {datos.Count} registros.");
        }

        static void MostrarGrafica()
        {
            if (lista.Count == 0)
            {
                Console.WriteLine("No hay datos para graficar.");
                return;
            }

            var grupos = lista.GroupBy(x => x.Category)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .ToList();

            int maxLabel = grupos.Max(g => (g.Categoria ?? "").Length);
            int maxCantidad = grupos.Max(g => g.Cantidad);
            int anchoMaxBarra = 40;

            Console.WriteLine();
            Console.WriteLine("  GRÁFICA POR CATEGORÍA");
            Console.WriteLine($"  {new string('─', maxLabel + anchoMaxBarra + 15)}");

            foreach (var g in grupos)
            {
                int longBarra = maxCantidad > 0
                    ? (int)Math.Round((double)g.Cantidad / maxCantidad * anchoMaxBarra)
                    : 0;

                string etiqueta = (g.Categoria ?? "").PadRight(maxLabel);
                string barra = new string('█', longBarra);

                Console.WriteLine($"  {etiqueta}  {barra} ({g.Cantidad})");
            }

            Console.WriteLine($"  {new string('─', maxLabel + anchoMaxBarra + 15)}");
            Console.WriteLine($"  Total: {lista.Count} registros");
        }

        static void MostrarResumen()
        {
            var resumen = lista.GroupBy(x => x.Category)
                .Select(g => new { g.Key, Total = g.Sum(x => x.Value) });

            foreach (var r in resumen)
                Console.WriteLine($"{r.Key}: {r.Total}");
        }

        static void MenuInsertar()
        {
            Console.Write("Nombre: ");
            string n = Console.ReadLine();

            Console.Write("Categoria: ");
            string c = Console.ReadLine();

            Console.Write("Valor: ");
            double v = double.Parse(Console.ReadLine());

            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            string query = "INSERT INTO DataItems (Name, Category, Value) VALUES (@n,@c,@v)";
            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@n", n);
            cmd.Parameters.AddWithValue("@c", c);
            cmd.Parameters.AddWithValue("@v", v);

            cmd.ExecuteNonQuery();

            Console.WriteLine("Guardado en SQL");
        }
    }
}