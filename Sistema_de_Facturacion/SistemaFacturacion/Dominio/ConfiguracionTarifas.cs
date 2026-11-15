namespace SistemaFacturacion.Dominio;
public sealed class ConfiguracionTarifas
{
    public decimal PrecioBaseKg { get; set; }
    public decimal TasaIva { get; set; }
    public decimal CostoFijoEnvio { get; set; }
    public void Validar()
    {
        if (PrecioBaseKg <= 0 || TasaIva < 0 || TasaIva > 1 || CostoFijoEnvio < 0)
            throw new InvalidDataException("Los valores de appsettings.json no son válidos.");
    }
}
