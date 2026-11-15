using Microsoft.Data.Sqlite;
using SistemaFacturacion.Dominio;
namespace SistemaFacturacion.Persistencia;
public sealed class RepositorioEnvios
{
    private readonly string _cadenaConexion;
    public RepositorioEnvios(string ruta)
    {
        _cadenaConexion = new SqliteConnectionStringBuilder { DataSource = ruta }.ToString();
    }
    public void Inicializar()
    {
        using SqliteConnection conexion = new(_cadenaConexion);
        conexion.Open();
        using SqliteCommand comando = conexion.CreateCommand();
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS Envios (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              LoteId TEXT NOT NULL, FechaUtc TEXT NOT NULL,
              Codigo TEXT NOT NULL, Cliente TEXT NOT NULL, Zona TEXT NOT NULL,
              PesoKg TEXT NOT NULL, ValorDeclarado TEXT NOT NULL,
              SubtotalPeso TEXT NOT NULL, RecargoZona TEXT NOT NULL,
              PenalizacionPeso TEXT NOT NULL, Seguro TEXT NOT NULL,
              CostoFijo TEXT NOT NULL, Iva TEXT NOT NULL, Total TEXT NOT NULL
            );
            """;
        comando.ExecuteNonQuery();
    }
    public int GuardarLote(Paquete[] paquetes)
    {
        if (paquetes.Length == 0) return 0;
        using SqliteConnection conexion = new(_cadenaConexion);
        conexion.Open();
        using SqliteTransaction transaccion = conexion.BeginTransaction();
        string loteId = Guid.NewGuid().ToString("N");
        string fecha = DateTime.UtcNow.ToString("O");
        foreach (Paquete p in paquetes)
        {
            using SqliteCommand comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = """
                INSERT INTO Envios (LoteId,FechaUtc,Codigo,Cliente,Zona,PesoKg,ValorDeclarado,
                SubtotalPeso,RecargoZona,PenalizacionPeso,Seguro,CostoFijo,Iva,Total)
                VALUES ($lote,$fecha,$codigo,$cliente,$zona,$peso,$valor,$subtotal,$recargo,$penalizacion,$seguro,$fijo,$iva,$total)
                """;
            comando.Parameters.AddWithValue("$lote", loteId);
            comando.Parameters.AddWithValue("$fecha", fecha);
            comando.Parameters.AddWithValue("$codigo", p.Codigo);
            comando.Parameters.AddWithValue("$cliente", p.Cliente);
            comando.Parameters.AddWithValue("$zona", p.Zona);
            comando.Parameters.AddWithValue("$peso", p.PesoKg.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$valor", p.ValorDeclarado.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$subtotal", p.SubtotalPeso.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$recargo", p.RecargoZona.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$penalizacion", p.PenalizacionPeso.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$seguro", p.Seguro.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$fijo", p.CostoFijo.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$iva", p.Iva.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$total", p.Total.ToString(System.Globalization.CultureInfo.InvariantCulture));
            comando.ExecuteNonQuery();
        }
        transaccion.Commit();
        return paquetes.Length;
    }
    public Paquete[] ConsultarHistorial()
    {
        using SqliteConnection conexion = new(_cadenaConexion);
        conexion.Open();
        using SqliteCommand comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Codigo,Cliente,Zona,PesoKg,ValorDeclarado,SubtotalPeso,RecargoZona,PenalizacionPeso,Seguro,CostoFijo,Iva,Total FROM Envios ORDER BY Id DESC";
        using SqliteDataReader lector = comando.ExecuteReader();
        var paquetes = new List<Paquete>();
        decimal Convertir(int i) => decimal.Parse(lector.GetString(i), System.Globalization.CultureInfo.InvariantCulture);
        while (lector.Read())
        {
            var p = new Paquete(lector.GetString(0), lector.GetString(1), lector.GetString(2), Convertir(3), Convertir(4));
            p.EstablecerDesglose(Convertir(5), Convertir(6), Convertir(7), Convertir(8), Convertir(9), Convertir(10), Convertir(11));
            paquetes.Add(p);
        }
        return paquetes.ToArray();
    }
}
