// PROGRAMA COMPLETO ORIGINAL + MEJORAS (CSV, JSON, TXT, XML)
// Incluye TODAS tus funciones + generación de nuevos formatos

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace FucionDatosConsola2
{
    class Program
    {
        static string connectionString = "Server=localhost\\SQLEXPRESS;Database=FucionDatosDB;Trusted_Connection=True;TrustServerCertificate=True;";
        static List<DataItem> lista = new List<DataItem>();
        static string rutaCarpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Datos_Proyecto");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("===== DATA FUSION ARENA =====");
                Console.WriteLine("1. Cargar desde SQL");
                Console.WriteLine("2. Mostrar Tabla");
                Console.WriteLine("3. Gráfica");
                Console.WriteLine("4. Resumen");
                Console.WriteLine("5. Filtrar >50");
                Console.WriteLine("6. Ordenar");
                Console.WriteLine("7. Insertar en SQL");
                Console.WriteLine("8. Detectar duplicados");
                Console.WriteLine("9. Generar archivos (CSV, JSON, TXT, XML)");
                Console.WriteLine("10. Cargar CSV");
                Console.WriteLine("11. Cargar JSON");
                Console.WriteLine("12. Limpiar lista");
                Console.WriteLine("0. Salir");

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
                    case "12": lista.Clear(); break;
                    case "0": salir = true; break;
                }

                Console.ReadKey();
            }
        }

        // MODELO
        public class DataItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public double Value { get; set; }
        }

        // SQL
        static void CargarDesdeBD()
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "SELECT Id, Name, Category, Value FROM DataItems";
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new DataItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Value = reader.GetDouble(3)
                });
            }
        }

        // GENERAR ARCHIVOS (CSV, JSON, TXT, XML)
        static void GenerarArchivosMasivos()
        {
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            int cantidad = 10000;
            string[] cats = { "Tecnologia", "Muebles", "Papeleria" };
            string[] prods = { "Monitor", "Silla", "Lapiz" };
            Random rnd = new Random();

            // CSV
            string rutaCSV = Path.Combine(rutaCarpeta, "datos_masivos.csv");
            StringBuilder csv = new StringBuilder("Nombre,Categoria,Valor\n");
            for (int i = 1; i <= cantidad; i++)
                csv.AppendLine($"{prods[rnd.Next(prods.Length)]},{cats[rnd.Next(cats.Length)]},{rnd.Next(100, 5000)}");
            File.WriteAllText(rutaCSV, csv.ToString());

            // JSON
            string rutaJSON = Path.Combine(rutaCarpeta, "datos_masivos.json");
            var listaJson = new List<DataItem>();
            for (int i = 1; i <= cantidad; i++)
                listaJson.Add(new DataItem { Id = i, Name = "Item" + i, Category = cats[rnd.Next(cats.Length)], Value = rnd.Next(100, 5000) });
            File.WriteAllText(rutaJSON, JsonSerializer.Serialize(listaJson, new JsonSerializerOptions { WriteIndented = true }));

            // TXT
            string rutaTXT = Path.Combine(rutaCarpeta, "datos_masivos.txt");
            StringBuilder txt = new StringBuilder();
            for (int i = 1; i <= cantidad; i++)
                txt.AppendLine($"Producto:{prods[rnd.Next(prods.Length)]}|Categoria:{cats[rnd.Next(cats.Length)]}|Valor:{rnd.Next(100, 5000)}");
            File.WriteAllText(rutaTXT, txt.ToString());

            // XML
            string rutaXML = Path.Combine(rutaCarpeta, "datos_masivos.xml");
            var serializer = new XmlSerializer(typeof(List<DataItem>));
            using (var writer = new StreamWriter(rutaXML))
            {
                serializer.Serialize(writer, listaJson);
            }

            Console.WriteLine("✔ Archivos CSV, JSON, TXT y XML generados");
        }

        // CSV
        static void CargarDesdeCSV()
        {
            string ruta = Path.Combine(rutaCarpeta, "datos_masivos.csv");
            var lineas = File.ReadAllLines(ruta);

            for (int i = 1; i < lineas.Length; i++)
            {
                var c = lineas[i].Split(',');
                if (c.Length < 3) continue;

                lista.Add(new DataItem
                {
                    Name = c[0],
                    Category = c[1],
                    Value = double.Parse(c[2])
                });
            }
        }

        // JSON
        static void CargarDesdeJSON()
        {
            string ruta = Path.Combine(rutaCarpeta, "datos_masivos.json");
            var json = File.ReadAllText(ruta);
            var datos = JsonSerializer.Deserialize<List<DataItem>>(json);
            lista.AddRange(datos);
        }

        // PROCESOS
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
        }

        static List<DataItem> Filtrar(double min)
        {
            return lista.Where(x => x.Value > min).ToList();
        }

        static void DetectarDuplicados()
        {
            Dictionary<string, int> conteo = new Dictionary<string, int>();
            foreach (var d in lista)
            {
                if (!conteo.ContainsKey(d.Name)) conteo[d.Name] = 0;
                conteo[d.Name]++;
            }

            foreach (var item in conteo.Where(x => x.Value > 1))
                Console.WriteLine($"Duplicado: {item.Key} ({item.Value})");
        }

        // VISUAL
        static void MostrarTabla(List<DataItem> datos)
        {
            Console.WriteLine("ID   NOMBRE        CATEGORIA     VALOR");
            foreach (var d in datos.Take(20))
                Console.WriteLine($"{d.Id} {d.Name} {d.Category} {d.Value}");
        }

        static void MostrarGrafica()
        {
            var grupos = lista.GroupBy(x => x.Category);
            foreach (var g in grupos)
            {
                Console.Write(g.Key + ": ");
                for (int i = 0; i < g.Count(); i++) Console.Write("█");
                Console.WriteLine();
            }
        }

        static void MostrarResumen()
        {
            var resumen = lista.GroupBy(x => x.Category)
                .Select(g => new { Cat = g.Key, Total = g.Sum(x => x.Value) });

            foreach (var r in resumen)
                Console.WriteLine($"{r.Cat}: {r.Total}");
        }

        static void MenuInsertar()
        {
            Console.Write("Nombre: "); string n = Console.ReadLine();
            Console.Write("Categoria: "); string c = Console.ReadLine();
            Console.Write("Valor: "); double v = double.Parse(Console.ReadLine());

            using SqlConnection conn = new SqlConnection(connectionString);
            string query = "INSERT INTO DataItems (Name, Category, Value) VALUES (@n, @c, @v)";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@n", n);
            cmd.Parameters.AddWithValue("@c", c);
            cmd.Parameters.AddWithValue("@v", v);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
