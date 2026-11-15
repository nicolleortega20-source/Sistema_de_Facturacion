namespace SistemaFacturacion.Dominio;
public sealed class ResultadoLote
{
    public Paquete[] Paquetes { get; }
    public string[] Errores { get; }
    public decimal TotalLote { get; }
    public ResultadoLote(Paquete[] paquetes, string[] errores)
    {
        Paquetes = paquetes; Errores = errores;
        decimal total = 0;
        foreach (Paquete paquete in paquetes) total += paquete.Total;
        TotalLote = total;
    }
}
