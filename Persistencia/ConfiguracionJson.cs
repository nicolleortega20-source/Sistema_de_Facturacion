using System.Text.Json;
using SistemaFacturacion.Dominio;
namespace SistemaFacturacion.Persistencia;
public sealed class ConfiguracionJson
{
    public ConfiguracionTarifas Cargar(string ruta)
    {
        if (!File.Exists(ruta)) throw new FileNotFoundException("No existe appsettings.json", ruta);
        using FileStream archivo = File.OpenRead(ruta);
        ConfiguracionTarifas config = JsonSerializer.Deserialize<ConfiguracionTarifas>(archivo)
            ?? throw new InvalidDataException("JSON vacío");
        config.Validar();
        return config;
    }
}
