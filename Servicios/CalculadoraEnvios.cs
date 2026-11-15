using SistemaFacturacion.Dominio;
namespace SistemaFacturacion.Servicios;
public sealed class CalculadoraEnvios
{
    public void Calcular(Paquete paquete, ConfiguracionTarifas tarifas)
    {
        decimal porcentajeZona = paquete.Zona switch
        {
            "ZONA A" => 0.05m,
            "ZONA B" => 0.10m,
            "ZONA C" => 0.18m,
            _ => throw new ArgumentException("Zona desconocida")
        };
        decimal subtotal = decimal.Round(paquete.PesoKg * tarifas.PrecioBaseKg, 2);
        decimal zona = decimal.Round(subtotal * porcentajeZona, 2);
        decimal penalizacion = paquete.PesoKg > 20m
            ? decimal.Round((paquete.PesoKg - 20m) * tarifas.PrecioBaseKg * 0.25m, 2) : 0m;
        decimal seguro = paquete.ValorDeclarado > 1000m
            ? decimal.Round(paquete.ValorDeclarado * 0.02m, 2) : 0m;
        decimal baseGravable = subtotal + zona + penalizacion + seguro + tarifas.CostoFijoEnvio;
        decimal iva = decimal.Round(baseGravable * tarifas.TasaIva, 2);
        paquete.EstablecerDesglose(subtotal, zona, penalizacion, seguro,
            tarifas.CostoFijoEnvio, iva, baseGravable + iva);
    }
}
