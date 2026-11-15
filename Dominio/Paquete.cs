namespace SistemaFacturacion.Dominio;
public sealed class Paquete
{
    public string Codigo { get; private set; }
    public string Cliente { get; private set; }
    public string Zona { get; private set; }
    public decimal PesoKg { get; private set; }
    public decimal ValorDeclarado { get; private set; }
    public decimal SubtotalPeso { get; private set; }
    public decimal RecargoZona { get; private set; }
    public decimal PenalizacionPeso { get; private set; }
    public decimal Seguro { get; private set; }
    public decimal CostoFijo { get; private set; }
    public decimal Iva { get; private set; }
    public decimal Total { get; private set; }
    public Paquete(string codigo, string cliente, string zona, decimal pesoKg, decimal valorDeclarado)
    {
        Codigo = codigo; Cliente = cliente; Zona = zona; PesoKg = pesoKg; ValorDeclarado = valorDeclarado;
    }
    public void EstablecerDesglose(decimal subtotalPeso, decimal recargoZona, decimal penalizacionPeso,
        decimal seguro, decimal costoFijo, decimal iva, decimal total)
    {
        SubtotalPeso = subtotalPeso; RecargoZona = recargoZona; PenalizacionPeso = penalizacionPeso;
        Seguro = seguro; CostoFijo = costoFijo; Iva = iva; Total = total;
    }
}
