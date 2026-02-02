using DocumentFormat.OpenXml.Spreadsheet;

namespace PureTCOWebApp.Core.Upload.Attributes;

/// <summary>
/// Specifies how the column is identified in the Excel file
/// </summary>
public enum ExcelColumnIdentifierType
{
    /// <summary>
    /// Column identified by letter (e.g., A, B, C, AA, AB)
    /// </summary>
    Letter,

    /// <summary>
    /// Column identified by number (e.g., 1, 2, 3)
    /// </summary>
    Number,

    /// <summary>
    /// Column identified by header name
    /// </summary>
    Name
}

/// <summary>
/// Attribute to map class properties to Excel columns
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelColumnAttribute : Attribute
{
    /// <summary>
    /// The column identifier
    /// </summary>
    public string Identifier { get; }
    public string? ColumnName { get; } = null;

    /// <summary>
    /// The type of identifier
    /// </summary>
    public ExcelColumnIdentifierType IdentifierType { get; }

    /// <summary>
    /// Creates a new ExcelColumn attribute with the specified column letter
    /// </summary>
    /// <param name="columnLetter">Excel column letter (e.g., A, B, C, AA, AB)</param>
    public ExcelColumnAttribute(string columnLetter, string? columnName = null)
    {
        Identifier = columnLetter;
        IdentifierType = ExcelColumnIdentifierType.Letter;
        ColumnName = columnName;
    }

    /// <summary>
    /// Creates a new ExcelColumn attribute with the specified column number
    /// </summary>
    /// <param name="columnNumber">Excel column number (e.g., 1, 2, 3)</param>
    public ExcelColumnAttribute(int columnNumber, string? columnName = null)
    {
        Identifier = columnNumber.ToString();
        IdentifierType = ExcelColumnIdentifierType.Number;
        ColumnName = columnName;
    }
}
