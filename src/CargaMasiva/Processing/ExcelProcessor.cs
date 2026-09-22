using ClosedXML.Excel;
using CargaMasiva.Models;

namespace CargaMasiva.Processing;

public class ExcelProcessor{
    private readonly ILogger<ExcelProcessor> _logger;

    public ExcelProcessor(ILogger<ExcelProcessor> logger){
        _logger = logger;
    }

    public List<ResultadoRegistro> Procesar(byte[] archivo)
    {
        var registros = new List<ResultadoRegistro>();

        using var stream = new MemoryStream(archivo);
        using var workbook = new XLWorkbook(stream);

        foreach (var worksheet in workbook.Worksheets){
            var range = worksheet.RangeUsed();

            if (range is null){
                _logger.LogInformation("La hoja {Hoja} está vacía.",worksheet.Name);
                continue;
            }

            _logger.LogInformation("Procesando hoja {Hoja}. Filas: {Filas}. Columnas: {Columnas}",worksheet.Name,range.RowCount(),range.ColumnCount());

            foreach (var row in range.RowsUsed().Skip(1)){
                if (row.Cells().All(c =>string.IsNullOrWhiteSpace(c.GetString()))){
                    _logger.LogInformation("Fila {Fila} ignorada porque está vacía.",row.RowNumber());
                    continue;
                }

                var registro = new RegistroExcel{
                    Periodo = row.Cell(1).GetString().Trim(),
                    CodigoProducto = row.Cell(2).GetString().Trim(),
                    Descripcion = row.Cell(3).GetString().Trim(),
                    Precio = ObtenerPrecio(row.Cell(4))
                };

                var resultado = new ResultadoRegistro{
                    Fila = row.RowNumber(),
                    Registro = registro,
                    EsValido = true
                };

                AplicarValoresPorDefecto(resultado);
                ValidarCamposObligatorios(resultado);

                registros.Add(resultado);
            }
        }

        ValidarCodigosDuplicados(registros);

        foreach (var resultado in registros){
            _logger.LogInformation("Resultado: CodigoProducto={CodigoProducto}, Valido={EsValido}, Observacion={Observacion}",resultado.Registro.CodigoProducto,resultado.EsValido,
                resultado.Observacion);
        }

        _logger.LogInformation("Procesamiento finalizado. Registros encontrados: {Cantidad}",registros.Count);

        return registros;
    }

    private void ValidarCodigosDuplicados(List<ResultadoRegistro> registros){
        var duplicados = registros
            .Where(x =>
            x.EsValido &&
            !string.IsNullOrWhiteSpace(x.Registro.CodigoProducto))
            .GroupBy(x => x.Registro.CodigoProducto)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var codigo in duplicados){
            var registrosDuplicados = registros
                .Where(x =>x.Registro.CodigoProducto == codigo)
                .ToList();

            // El primero se mantiene como válido.
            foreach (var registro in registrosDuplicados.Skip(1)){
                registro.EsValido = false;
                registro.Observacion = "Existente";

                _logger.LogWarning("CodigoProducto duplicado: {CodigoProducto}",codigo);
            }
        }
    }

    private static void AplicarValoresPorDefecto(ResultadoRegistro resultado){
        var registro = resultado.Registro;

        if (string.IsNullOrWhiteSpace(registro.Descripcion)){
            registro.Descripcion = "SIN DESCRIPCION";
        }

        if (registro.Precio is null){
            registro.Precio = 0;
        }
    }

    private static void ValidarCamposObligatorios(ResultadoRegistro resultado){
        var registro = resultado.Registro;

        if (string.IsNullOrWhiteSpace(registro.Periodo)){
            resultado.EsValido = false;
            resultado.Observacion = "Periodo obligatorio";
            return;
        }

        if (string.IsNullOrWhiteSpace(registro.CodigoProducto)){
            resultado.EsValido = false;
            resultado.Observacion = "CodigoProducto obligatorio";
        }
    }

    private static decimal? ObtenerPrecio(IXLCell cell){
        if (cell.IsEmpty())
            return null;

        if (cell.TryGetValue<decimal>(out var precio))
            return precio;

        return null;
    }
}