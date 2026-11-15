using System.Globalization;
using System.Text;
using SistemaFacturacion.Dominio;
namespace SistemaFacturacion.Persistencia;
public sealed class ExportadorCsv
{
    private static string Escapar(string valor) => "\"" + valor.Replace("\"", "\"\"") + "\"";
    public void Exportar(string ruta, Paquete[] paquetes)
    {
        using StreamWriter escritor = new(ruta, false, new UTF8Encoding(true));
        escritor.WriteLine("Codigo,Cliente,Zona,PesoKg,ValorDeclarado,SubtotalPeso,RecargoZona,PenalizacionPeso,Seguro,CostoFijo,IVA,Total");
        foreach (Paquete p in paquetes)
        {
            string[] columnas = { Escapar(p.Codigo), Escapar(p.Cliente), Escapar(p.Zona),
                p.PesoKg.ToString(CultureInfo.InvariantCulture), p.ValorDeclarado.ToString(CultureInfo.InvariantCulture),
                p.SubtotalPeso.ToString(CultureInfo.InvariantCulture), p.RecargoZona.ToString(CultureInfo.InvariantCulture),
                p.PenalizacionPeso.ToString(CultureInfo.InvariantCulture), p.Seguro.ToString(CultureInfo.InvariantCulture),
                p.CostoFijo.ToString(CultureInfo.InvariantCulture), p.Iva.ToString(CultureInfo.InvariantCulture),
                p.Total.ToString(CultureInfo.InvariantCulture) };
            escritor.WriteLine(string.Join(",", columnas));
        }
    }
}
