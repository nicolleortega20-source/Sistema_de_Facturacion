using System.Globalization;
using System.Text.RegularExpressions;
using SistemaFacturacion.Dominio;
namespace SistemaFacturacion.Servicios;
public sealed class ProcesadorLotes
{
    private readonly CalculadoraEnvios _calculadora = new();
    private static readonly Regex CodigoValido = new(@"^[A-Z]+-[0-9]+$", RegexOptions.Compiled);
    public ResultadoLote Procesar(string entrada, ConfiguracionTarifas tarifas)
    {
        string[] lineas = entrada.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        Paquete[] arregloBase = new Paquete[5];
        string[] arregloErrores = new string[5];
        int validos = 0, errores = 0;
        for (int i = 0; i < lineas.Length; i++)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lineas[i])) throw new FormatException("Línea vacía");
                string[] campos = lineas[i].Split(';');
                if (campos.Length != 5) throw new FormatException("Se requieren exactamente cinco campos");
                for (int j = 0; j < campos.Length; j++) campos[j] = campos[j].Trim();
                string codigo = campos[0].ToUpperInvariant();
                string zona = Regex.Replace(campos[2].ToUpperInvariant(), @"\s+", " ");
                if (!CodigoValido.IsMatch(codigo)) throw new FormatException("Código inválido");
                if (string.IsNullOrWhiteSpace(campos[1])) throw new FormatException("Cliente vacío");
                if (zona != "ZONA A" && zona != "ZONA B" && zona != "ZONA C")
                    throw new FormatException("Zona inválida");
                if (!decimal.TryParse(campos[3], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out decimal peso)) throw new FormatException("Peso no numérico (use punto decimal)");
                if (!decimal.TryParse(campos[4], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out decimal valor)) throw new FormatException("Valor declarado no numérico (use punto decimal)");
                if (peso <= 0 || valor < 0) throw new FormatException("Peso debe ser positivo y valor no negativo");
                Paquete paquete = new(codigo, campos[1], zona, peso, valor);
                _calculadora.Calcular(paquete, tarifas);
                if (validos == arregloBase.Length)
                {
                    Paquete[] ampliado = new Paquete[arregloBase.Length * 2];
                    for (int j = 0; j < validos; j++) ampliado[j] = arregloBase[j];
                    arregloBase = ampliado;
                }
                arregloBase[validos++] = paquete;
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
            {
                if (errores == arregloErrores.Length)
                {
                    string[] ampliado = new string[arregloErrores.Length * 2];
                    for (int j = 0; j < errores; j++) ampliado[j] = arregloErrores[j];
                    arregloErrores = ampliado;
                }
                arregloErrores[errores++] = $"Línea {i + 1}: {ex.Message}";
            }
        }
        // Ordenamiento por selección: de mayor a menor costo.
        for (int i = 0; i < validos - 1; i++)
        {
            int mayor = i;
            for (int j = i + 1; j < validos; j++)
                if (arregloBase[j].Total > arregloBase[mayor].Total) mayor = j;
            (arregloBase[i], arregloBase[mayor]) = (arregloBase[mayor], arregloBase[i]);
        }
        Paquete[] resultado = new Paquete[validos];
        for (int i = 0; i < validos; i++) resultado[i] = arregloBase[i];
        string[] mensajes = new string[errores];
        for (int i = 0; i < errores; i++) mensajes[i] = arregloErrores[i];
        return new ResultadoLote(resultado, mensajes);
    }
}
