using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq; // Para usar algunas bondades de colecciones

class Program
{
    // 1. La cadena de conexión (asegúrate que el nombre de la BD sea el de SQL)
    static string connectionString = "Server=localhost\\SQLEXPRESS;Database=FucionDatosDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // 2. AQUÍ DEBE ESTAR LA LISTA. Si no está aquí, te dará el error de "no existe en el contexto".
    static List<DataItem> lista = new List<DataItem>();
    static void Main()
    {
        bool salir = false;
        while (!salir)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("       DATA FUSION ARENA v2.0         ");
            Console.WriteLine("======================================");
            Console.WriteLine("1. Cargar/Refrescar Datos desde BD");
            Console.WriteLine("2. Mostrar Tabla de Datos");
            Console.WriteLine("3. Ver Gráfica de Categorías");
            Console.WriteLine("4. Ver Resumen Financiero (Totales)");
            Console.WriteLine("5. Filtrar por Valor (>50)");
            Console.WriteLine("6. Ordenar por Valor (Menor a Mayor)");
            Console.WriteLine("7. Insertar Nuevo Registro");
            Console.WriteLine("8. Detectar Duplicados");
            Console.WriteLine("0. Salir");
            Console.Write("\nSelecciona una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1": CargarDesdeBD(); break;
                case "2": MostrarTabla(lista); break;
                case "3": MostrarGrafica(); break;
                case "4": MostrarResumen(); break;
                case "5": var filtrados = Filtrar(50); MostrarTabla(filtrados); break;
                case "6": Ordenar(); MostrarTabla(lista); break;
                case "7": MenuInsertar(); break;
                case "8": DetectarDuplicados(); break;
                case "0": salir = true; break;
                default: Console.WriteLine("Opción no válida."); break;
            }

            if (!salir)
            {
                Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    class DataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double Value { get; set; }
    }

    // ===== CARGAR DATOS (CON TRY-CATCH) =====
    static void CargarDesdeBD()
    {
        try
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "SELECT Id, Name, Category, Value FROM DataItems";

            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            lista.Clear();
            while (reader.Read())
            {
                lista.Add(new DataItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.IsDBNull(2) ? "Sin categoría" : reader.GetString(2),
                    Value = reader.GetDouble(3)
                });
            }
            Console.WriteLine($"\n✔ Éxito: {lista.Count} registros cargados.");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"\n❌ Error de SQL: {ex.Message}");
        }
    }

    // ===== INSERTAR DATOS (PARAMETRIZADO) =====
    static void MenuInsertar()
    {
        try
        {
            Console.Write("Nombre del producto: ");
            string nombre = Console.ReadLine();
            Console.Write("Categoría: ");
            string cat = Console.ReadLine();
            Console.Write("Valor (numérico): ");
            double val = double.Parse(Console.ReadLine());

            using SqlConnection conn = new SqlConnection(connectionString);
            string query = "INSERT INTO DataItems (Name, Category, Value) VALUES (@n, @c, @v)";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@n", nombre);
            cmd.Parameters.AddWithValue("@c", cat);
            cmd.Parameters.AddWithValue("@v", val);

            conn.Open();
            cmd.ExecuteNonQuery();
            Console.WriteLine("✔ Registro guardado en base de datos. Recarga (opción 1) para ver cambios.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al insertar: {ex.Message}");
        }
    }

    // ===== MOSTRAR TABLA =====
    static void MostrarTabla(List<DataItem> datos)
    {
        if (datos.Count == 0) { Console.WriteLine("No hay datos para mostrar."); return; }

        Console.WriteLine("\n{0,-5} {1,-15} {2,-15} {3,-10}", "ID", "NOMBRE", "CATEGORIA", "VALOR");
        Console.WriteLine(new string('-', 50));

        foreach (var d in datos)
        {
            Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10:C}", d.Id, d.Name, d.Category, d.Value);
        }
    }

    // ===== RESUMEN FINANCIERO =====
    static void MostrarResumen()
    {
        Console.WriteLine("\n--- RESUMEN FINANCIERO (TOTALES POR CATEGORÍA) ---");
        var resumen = lista.GroupBy(x => x.Category)
                           .Select(g => new { Cat = g.Key, Total = g.Sum(v => v.Value) });

        foreach (var item in resumen)
        {
            Console.WriteLine($"{item.Cat.PadRight(15)}: {item.Total:C}");
        }
    }

    // ===== FILTRO =====
    static List<DataItem> Filtrar(double min)
    {
        return lista.Where(d => d.Value > min).ToList();
    }

    // ===== ORDENAR (BUBBLE SORT) =====
    static void Ordenar()
    {
        for (int i = 0; i < lista.Count - 1; i++)
        {
            for (int j = i + 1; j < lista.Count; j++)
            {
                if (lista[i].Value > lista[j].Value)
                {
                    var temp = lista[i];
                    lista[i] = lista[j];
                    lista[j] = temp;
                }
            }
        }
        Console.WriteLine("\nLista ordenada por valor.");
    }

    // ===== GRAFICA =====
    static void MostrarGrafica()
    {
        Console.WriteLine("\n--- DISTRIBUCIÓN POR CANTIDAD ---");
        var conteo = lista.GroupBy(x => x.Category);

        foreach (var grupo in conteo)
        {
            Console.Write($"{grupo.Key.PadRight(12)}: ");
            for (int i = 0; i < grupo.Count(); i++) Console.Write("█");
            Console.WriteLine($" ({grupo.Count()})");
        }
    }

    // ===== DUPLICADOS =====
    static void DetectarDuplicados()
    {
        var duplicados = lista.GroupBy(x => x.Name)
                             .Where(g => g.Count() > 1);

        if (!duplicados.Any()) Console.WriteLine("No se encontraron nombres duplicados.");

        foreach (var d in duplicados)
        {
            Console.WriteLine($"Duplicado: {d.Key} (Aparece {d.Count()} veces)");
        }
    }
}