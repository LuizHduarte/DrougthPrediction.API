using DroughtPrediction.Exceptions;
using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Data;

namespace DroughtPrediction.Services.DataLoading;
public class DataLoadingService : IDataLoadingService
{
    private class Excel : FileType, IFileType
    {
        public const string TypeName = "Excel";
        public const string TypeExtension = "xlsx";
        private static readonly byte[] MagicBytes = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00, 0x08, 0x00, 0x00, 0x00, 0x21, 0x00];

        public Excel() : base(TypeName, TypeExtension, MagicBytes) { }
    }

    public async Task<DataTable> LoadFileData(IFormFile file)
    {
        FileTypeValidator.RegisterCustomTypes(typeof(Excel).Assembly);

        var fileStream = file.OpenReadStream();
        var isExcel = fileStream.Is<Excel>();

        if (isExcel == false)
        {
            throw new IncorrectFileException("The file format is incorrect");
        }

        var dataTable = new DataTable();

        using (var stream = new MemoryStream())
        {
            await stream.FlushAsync();
            stream.Position = 0;
            await file.CopyToAsync(stream);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            foreach (var firstRowCell in worksheet.Cells[1, 1, 1, 2])
            {
                dataTable.Columns.Add(firstRowCell.Text);
            }

            for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                ExcelRangeBase row = worksheet.Cells[rowNum, 1, rowNum, 2];
                DataRow newRow = dataTable.Rows.Add();
                foreach (var cell in row)
                {
                    newRow[cell.Start.Column - 1] = cell.Text;
                }
            }
        }
        return dataTable;
    }
}
