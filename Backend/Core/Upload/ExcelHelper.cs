using System.Reflection;
using ClosedXML.Excel;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Core.Upload.Attributes;

namespace PureTCOWebApp.Core.Upload;

public static class ExcelHelper
{
    // Excel file signature (magic number)
    private static readonly byte[] ExcelSignature = { 0x50, 0x4B, 0x03, 0x04 }; // PKZip signature used by .xlsx

    public static Result ValidateExcelFile(IFormFile file)
    {
        // convert req.File to bytes array first
        if (file.Length == 0)
        {
            return ExcelDomainError.FileIsEmpty;
        }

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
        {
            return ExcelDomainError.FileSizeExceedsLimit(10);
        }

        if (!file.ContentType.StartsWith("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
        {
            return ExcelDomainError.InvalidFileType;
        }

        return Result.Success();
    }

    public static BulkResult ValidateColumnStructure<T>(byte[] fileData, string? sheetName = null, bool hasHeaderRow = true) where T : new()
    {
        var errors = new List<Error>();

        if (fileData == null || fileData.Length == 0)
        {
            errors.Add(new Error("FileEmpty", "File data cannot be null or empty"));
            return BulkResult.Failure(new Error("VALIDATION_FAILED", "One or more validation errors occurred")).SetErrors(errors);
        }

        // Validate file signature
        if (!IsExcelFile(fileData))
        {
            errors.Add(new Error("InvalidFile", "The provided data is not a valid Excel file"));
            return BulkResult.Failure(new Error("ValidationFailed", "One or more validation errors occurred")).SetErrors(errors);
        }

        try
        {
            using (var memoryStream = new MemoryStream(fileData))
            using (var workbook = new XLWorkbook(memoryStream))
            {
                // Get the worksheet
                var worksheet = string.IsNullOrEmpty(sheetName)
                    ? workbook.Worksheets.FirstOrDefault()
                    : workbook.Worksheets.FirstOrDefault(w => w.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

                if (worksheet == null)
                {
                    errors.Add(new Error("WorksheetNotFound", $"Worksheet {sheetName ?? "default"} not found"));
                    return BulkResult.Failure(new Error("ValidationFailed", "Column structure validation failed")).SetErrors(errors);
                }

                // Get the properties with ExcelColumnAttribute
                var propertyMappings = GetPropertyColumnMappings(typeof(T));

                if (!propertyMappings.Any())
                {
                    errors.Add(new Error("NoExcelColumns", "No properties with ExcelColumn attributes found in the target type"));
                    return BulkResult.Failure(new Error("ValidationFailed", "Column structure validation failed")).SetErrors(errors);
                }

                // If has header row, validate column names
                if (hasHeaderRow)
                {
                    // Get header row
                    var headerRow = worksheet.Row(1);
                    var headerToColumnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    foreach (var cell in headerRow.CellsUsed())
                    {
                        string headerValue = cell.GetString().Trim();
                        if (!string.IsNullOrEmpty(headerValue))
                        {
                            headerToColumnIndex[headerValue] = cell.Address.ColumnNumber;
                        }
                    }

                    // Validate each property mapping that uses column names
                    foreach (var mapping in propertyMappings)
                    {
                        var attribute = mapping.Value;
                        var propertyName = mapping.Key.Name;

                        if (attribute.IdentifierType == ExcelColumnIdentifierType.Name)
                        {
                            // Check if the column name exists in headers
                            if (!headerToColumnIndex.ContainsKey(attribute.Identifier))
                            {
                                errors.Add(new Error("MissingColumn",
                                    $"Required column '{attribute.Identifier}' for '{propertyName}' is not found in Excel headers"));
                            }
                        }
                        else if (!string.IsNullOrEmpty(attribute.ColumnName))
                        {
                            // If attribute has a ColumnName specified, validate it exists in headers
                            if (!headerToColumnIndex.ContainsKey(attribute.ColumnName))
                            {
                                errors.Add(new Error("MissingNamedColumn",
                                    $"Expected column '{attribute.ColumnName}' at column {attribute.Identifier} is not found in Excel headers"));
                            }
                        }
                        else
                        {
                            // For Letter/Number identifiers, validate the column index exists
                            int columnIndex = GetColumnIndex(attribute, headerToColumnIndex);
                            if (columnIndex <= 0)
                            {
                                errors.Add(new Error("InvalidColumnIndex",
                                    $"Invalid column '{attribute.Identifier}' for property '{propertyName}'"));
                            }
                            else if (columnIndex > headerRow.CellsUsed().Count())
                            {
                                errors.Add(new Error("ColumnOutOfRange",
                                    $"Column {attribute.Identifier} for property '{propertyName}' is out of range. Excel has {headerRow.CellsUsed().Count()} columns"));
                            }
                        }
                    }
                }
                else
                {
                    // If no header row, just validate that Letter/Number identifiers are valid
                    foreach (var mapping in propertyMappings)
                    {
                        var attribute = mapping.Value;
                        var propertyName = mapping.Key.Name;

                        if (attribute.IdentifierType == ExcelColumnIdentifierType.Name)
                        {
                            errors.Add(new Error("NoHeaderForName",
                                $"Property '{propertyName}' uses column name '{attribute.Identifier}' but file has no header row"));
                        }
                        else
                        {
                            int columnIndex = GetColumnIndex(attribute, null);
                            if (columnIndex <= 0)
                            {
                                errors.Add(new Error("InvalidColumnIndex",
                                    $"Invalid column identifier '{attribute.Identifier}' for property '{propertyName}'"));
                            }
                        }
                    }
                }

                // Return success or failure based on whether there are errors
                if (errors.Any())
                {
                    return BulkResult.Failure(new Error("ValidationFailed", "Column structure validation failed")).SetErrors(errors);
                }

                return BulkResult.Success();
            }
        }
        catch (Exception ex)
        {
            errors.Add(new Error("ExcelProcessingError", $"Error processing Excel file: {ex.Message}"));
            return BulkResult.Failure(new Error("ValidationFailed", "Column structure validation failed")).SetErrors(errors);
        }
    }

    /// <summary>
    /// Parse Excel binary data into a list of typed objects
    /// </summary>
    /// <typeparam name="T">Type to map Excel rows to</typeparam>
    /// <param name="fileData">Binary data containing the Excel file</param>
    /// <param name="sheetName">Optional sheet name (uses first sheet if not specified)</param>
    /// <param name="hasHeaderRow">Whether the first row contains headers</param>
    /// <returns>A list of parsed objects</returns>
    public static List<T> ParseExcelData<T>(
        byte[] fileData,
        string? columnToBeCheckedForLast = null,
        string? sheetName = null,
        bool hasHeaderRow = true,
        bool trim = true,
        int startRow = 1
    ) where T : new()
    {
        if (fileData == null || fileData.Length == 0)
            throw new ArgumentException("File data cannot be null or empty");

        if (!IsExcelFile(fileData))
            throw new ArgumentException("The provided data is not a valid Excel file");

        using var memoryStream = new MemoryStream(fileData);
        using var workbook = new XLWorkbook(memoryStream);

        var worksheet = string.IsNullOrEmpty(sheetName)
            ? workbook.Worksheets.FirstOrDefault()
            : workbook.Worksheets.FirstOrDefault(w => w.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

        if (worksheet == null)
            throw new ArgumentException($"Worksheet {sheetName ?? "default"} not found");

        Dictionary<string, int>? headerToColumnIndex = null;
        if (hasHeaderRow)
        {
            headerToColumnIndex = new(StringComparer.OrdinalIgnoreCase);
            var headerRow = worksheet.Row(startRow);
            foreach (var cell in headerRow.CellsUsed())
            {
                var headerValue = cell.GetString().Trim();
                if (!string.IsNullOrEmpty(headerValue))
                    headerToColumnIndex[headerValue] = cell.Address.ColumnNumber;
            }
            startRow++;
        }

        var propertyMappings = GetPropertyColumnMappings(typeof(T));
        var result = new List<T>();

        int lastRow = string.IsNullOrEmpty(columnToBeCheckedForLast)
            ? worksheet.LastRowUsed()?.RowNumber() ?? 0
            : worksheet.Column(columnToBeCheckedForLast).LastCellUsed().Address.RowNumber;

        for (int rowIndex = startRow; rowIndex <= lastRow; rowIndex++)
        {
            var row = worksheet.Row(rowIndex);
            if (row.IsEmpty()) continue;

            T item = new();

            foreach (var mapping in propertyMappings)
            {
                int columnIndex = GetColumnIndex(mapping.Value, headerToColumnIndex);
                if (columnIndex > 0)
                {
                    var cell = row.Cell(columnIndex);
                    if (!cell.IsEmpty())
                    {
                        var value = ConvertCellValue(cell, mapping.Key.PropertyType);
                        if (value is string str)
                            value = str.Trim();

                        mapping.Key.SetValue(item, value);
                    }
                }
            }

            result.Add(item);
        }

        if (!result.Any() || result.All(IsObjectEmpty))
            throw new InvalidOperationException("No data found in Excel file.");

        return result;
    }


    /// <summary>
    /// Checks if an object has all null or empty string properties
    /// </summary>
    private static bool IsObjectEmpty<T>(T obj)
    {
        if (obj == null) return true;

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && p.GetCustomAttribute<ExcelColumnAttribute>() != null);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a dictionary mapping properties to their column attributes
    /// </summary>
    private static Dictionary<PropertyInfo, ExcelColumnAttribute> GetPropertyColumnMappings(Type type)
    {
        var result = new Dictionary<PropertyInfo, ExcelColumnAttribute>();

        foreach (var property in type.GetProperties())
        {
            var attribute = property.GetCustomAttribute<ExcelColumnAttribute>();
            if (attribute != null)
            {
                result[property] = attribute;
            }
        }

        return result;
    }

    /// <summary>
    /// Determines the column index based on the attribute settings
    /// </summary>
    private static int GetColumnIndex(ExcelColumnAttribute attribute, Dictionary<string, int>? headerToColumnIndex)
    {
        switch (attribute.IdentifierType)
        {
            case ExcelColumnIdentifierType.Letter:
                // Convert column letter to column number
                return ConvertColumnLetterToNumber(attribute.Identifier);

            case ExcelColumnIdentifierType.Number:
                // Parse column number directly
                if (int.TryParse(attribute.Identifier, out int columnNumber))
                    return columnNumber;
                return -1;

            case ExcelColumnIdentifierType.Name:
                // Look up column by header name
                if (headerToColumnIndex != null && headerToColumnIndex.TryGetValue(attribute.Identifier, out int index))
                    return index;
                return -1;

            default:
                return -1;
        }
    }

    /// <summary>
    /// Converts an Excel column letter into a column number
    /// </summary>
    private static int ConvertColumnLetterToNumber(string columnLetter)
    {
        if (string.IsNullOrEmpty(columnLetter))
            return -1;

        columnLetter = columnLetter.ToUpperInvariant();
        int sum = 0;

        for (int i = 0; i < columnLetter.Length; i++)
        {
            sum *= 26;
            sum += (columnLetter[i] - 'A' + 1);
        }

        return sum;
    }

    /// <summary>
    /// Converts a cell value to the required property type
    /// </summary>
    private static object? ConvertCellValue(IXLCell cell, Type targetType)
    {
        if (cell.IsEmpty())
            return GetDefaultValue(targetType);

        // Handle common data types
        if (targetType == typeof(string))
            return cell.GetString();

        if (targetType == typeof(double) || targetType == typeof(double?))
        {
            var raw = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrEmpty(raw))
                return targetType == typeof(double?) ? null : 0;

            if (double.TryParse(raw, out var result))
                return result;

            throw new FormatException($"Cannot convert '{raw}' in cell {cell.Address} to double.");
        }

        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            var raw = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrEmpty(raw))
                return targetType == typeof(int?) ? null : 0;

            if (int.TryParse(raw, out var result))
                return result;

            throw new FormatException($"Cannot convert '{raw}' in cell {cell.Address} to Int32.");
        }

        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            var raw = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrEmpty(raw))
                return targetType == typeof(decimal?) ? null : 0m;

            if (decimal.TryParse(raw, out var result))
                return result;

            throw new FormatException($"Cannot convert '{raw}' in cell {cell.Address} to Decimal.");
        }

        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
        {
            try
            {
                return cell.GetValue<DateTime>();
            }
            catch
            {
                if (targetType == typeof(DateTime?))
                    return null;
                throw new FormatException($"Invalid DateTime format in cell {cell.Address}.");
            }
        }

        if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
        {
            try
            {
                return DateOnly.FromDateTime(cell.GetValue<DateTime>());
            }
            catch
            {
                if (targetType == typeof(DateOnly?))
                    return null;
                throw new FormatException($"Invalid DateOnly format in cell {cell.Address}.");
            }
        }

        if (targetType == typeof(bool) || targetType == typeof(bool?))
            return cell.GetValue<bool>();

        // Default - try to convert using ChangeType
        try
        {
            return Convert.ChangeType(cell.Value, targetType);
        }
        catch
        {
            return GetDefaultValue(targetType);
        }
    }

    /// <summary>
    /// Gets the default value for a type
    /// </summary>
    private static object? GetDefaultValue(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }

    /// <summary>
    /// Validates if the given data represents an Excel file
    /// </summary>
    private static bool IsExcelFile(byte[] fileData)
    {
        // Check if the data has enough bytes for signature check
        if (fileData.Length < ExcelSignature.Length)
            return false;

        // Check the file signature
        for (int i = 0; i < ExcelSignature.Length; i++)
        {
            if (fileData[i] != ExcelSignature[i])
                return false;
        }

        // Additional validation could include checking for the extension
        // in the central directory of the ZIP structure, but that's more complex
        return true;
    }
}
