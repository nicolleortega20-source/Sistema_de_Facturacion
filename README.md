# Sistema de Facturación

Aplicación de escritorio desarrollada en **C# con Windows Forms y .NET 10**, orientada al procesamiento y auditoría de facturación de una empresa de logística y entregas. Permite capturar lotes de paquetes, validar la información, calcular los importes correspondientes, guardar los envíos en una base de datos SQLite y generar reportes CSV.

Proyecto desarrollado **en equipo** con fines académicos.

## Características

- **Procesamiento de lotes:** captura de varios paquetes mediante líneas de texto separadas por punto y coma.
- **Validación de datos:** identificación de registros inválidos sin interrumpir el procesamiento del resto del lote.
- **Cálculo de facturación:** cálculo de subtotal por peso, recargos por zona, penalizaciones, seguro, costo fijo e IVA, según las tarifas configuradas.
- **Resultados y detalles:** tabla de paquetes procesados, indicadores y desglose de importes por envío.
- **Persistencia SQLite:** almacenamiento de envíos y consulta del historial desde el módulo de auditoría.
- **Configuración JSON:** lectura de tarifas desde `appsettings.json` y opción para recargar la configuración.
- **Reportes CSV:** exportación del lote actual y del historial de envíos.
- **Interfaz bilingüe:** selección entre español e inglés.
- **Interfaz gráfica:** diseño en tonos naranja, café y crema, con navegación entre módulos.

## Requisitos

- Windows 10 u 11.
- Para ejecutar una publicación dependiente del framework: [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0), en la arquitectura correspondiente.
- Para compilar: **SDK de .NET 10** y Visual Studio con la carga de trabajo **Desarrollo de escritorio con .NET**.
- Acceso a NuGet para restaurar la dependencia `Microsoft.Data.Sqlite`.

## Instalación y ejecución

### Opción 1: Programa publicado

> Esta opción requiere que el equipo entregue, por separado, una carpeta de publicación probada. El ZIP del código fuente no es un instalador.

1. Descarga y descomprime el paquete de instalación proporcionado por el equipo.
2. Si la publicación depende de .NET, instala el **.NET 10 Desktop Runtime** correspondiente.
3. Conserva los archivos de configuración junto al ejecutable.
4. Ejecuta `SistemaFacturacion.exe`.

Si una política de seguridad de Windows impide ejecutarlo, solicita autorización al administrador del equipo; no desactives las protecciones.

### Opción 2: Compilar desde el código fuente

Clona el repositorio del equipo (sustituye la dirección cuando esté publicado):

```bash
git clone URL_DEL_REPOSITORIO
cd NOMBRE_DE_LA_CARPETA
```

1. Abre `SistemaFacturacion.sln` en Visual Studio.
2. Espera a que se restauren las dependencias NuGet.
3. Selecciona la configuración **Debug** o **Release**.
4. Presiona **F5** para ejecutar.

También puedes publicar desde la terminal, dentro de la carpeta que contiene el archivo `.csproj`:

```bash
dotnet publish SistemaFacturacion.csproj -c Release -r win-x64 --self-contained false -o publicado
```

La publicación anterior requiere el runtime de escritorio de .NET 10 en el equipo de destino. Verifica que `appsettings.json` y `Traducciones.json` estén incluidos en la carpeta publicada.

## Uso básico

1. Abre **Sistema de Facturación** y selecciona **Español** o **English**.
2. En **Procesar lotes**, escribe un paquete por línea con el formato:

   ```text
   PACK-001; Juan Perez; Zona A; 12.5; 150.0
   PACK-002; Maria Lopez; Zona C; -5.0; 200.0
   PACK-003; Pedro Gomez; Zona B; ABC; 100.0
   PACK-004; Ana Ruiz; Zona C; 23.5; 1500.0
   ```

3. Presiona **Procesar lote** para validar los registros y calcular los importes.
4. Revisa los indicadores, los errores y el detalle de cada paquete en la tabla.
5. Presiona **Guardar SQLite** para almacenar los paquetes válidos.
6. Abre **Auditoría de envíos** para consultar los registros guardados.
7. En **Configuración JSON**, consulta las tarifas cargadas y utiliza **Recargar JSON** cuando corresponda.
8. En **Reportes CSV**, exporta el lote actual o el historial completo.
9. Utiliza **Limpiar** para borrar los datos visibles del lote actual, sin eliminar el historial de SQLite.
10. Presiona **Salir** y confirma para cerrar el programa.

## Estructura del proyecto

| Archivo o carpeta | Descripción |
| --- | --- |
| `Program.cs` | Punto de entrada y configuración regional de la aplicación. |
| `Presentacion/FormPrincipal.cs` | Lógica de interacción, navegación y eventos del formulario principal. |
| `Presentacion/FormPrincipal.Designer.cs` | Controles y diseño de la interfaz Windows Forms. |
| `Dominio/Paquete.cs` | Modelo de datos de los paquetes. |
| `Dominio/ResultadoLote.cs` | Resultado del procesamiento de un lote. |
| `Dominio/ConfiguracionTarifas.cs` | Modelo de tarifas. |
| `Servicios/ProcesadorLotes.cs` | Procesamiento y validación de registros. |
| `Servicios/CalculadoraEnvios.cs` | Cálculo de importes de los envíos. |
| `Servicios/ServicioIdiomas.cs` | Gestión de los textos de la interfaz. |
| `Servicios/Traducciones.json` | Recursos de traducción utilizados por el sistema. |
| `Persistencia/RepositorioEnvios.cs` | Almacenamiento y consulta de registros en SQLite. |
| `Persistencia/ConfiguracionJson.cs` | Lectura de tarifas desde JSON. |
| `Persistencia/ExportadorCsv.cs` | Generación de archivos CSV. |
| `appsettings.json` | Configuración de tarifas. |
| `SistemaFacturacion.csproj` | Configuración y dependencias del proyecto. |
| `SistemaFacturacion.sln` | Solución para Visual Studio. |

## Almacenamiento de datos

Los envíos guardados se almacenan en una base de datos SQLite ubicada en:

```text
%LOCALAPPDATA%\SistemaFacturacion\envios.db
```

La opción **Limpiar** no elimina esa base de datos. Los archivos CSV se guardan en la ubicación elegida por el usuario.

## Tecnologías

- C# y Windows Forms.
- .NET 10.
- SQLite mediante `Microsoft.Data.Sqlite`.
- JSON para configuración y traducciones.
- CSV para exportación de reportes.
- Git y GitHub para control de versiones y trabajo colaborativo.

## Repositorio

**GitHub:** `URL_DEL_REPOSITORIO`

Sustituir por la URL real del repositorio del equipo antes de entregar. El repositorio debe incluir el código fuente y los archivos necesarios para compilar el proyecto, pero no contraseñas ni archivos temporales de Visual Studio.

## Equipo de desarrollo

| Integrante | ID universitario | 
| --- | --- |
| Carmen Elizabeth González Guerrero | 277472 | 
| Carla Alejandra Montalvo Ojeda | 428942 | 
| Nicolle Ortega Moreno | 432378 | 


## Contexto académico

Proyecto académico de desarrollo de software realizado en equipo para la **Universidad Autónoma de Aguascalientes**.

## Observaciones

- Las tarifas pueden modificarse en `appsettings.json`, respetando la estructura esperada por el programa.
- El cambio de idioma de la interfaz no modifica los importes almacenados.
