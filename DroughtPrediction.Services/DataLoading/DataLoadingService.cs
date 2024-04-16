using CsvHelper;
using CsvHelper.TypeConversion;
using DroughtPrediction.Exceptions;
using FileTypeChecker;
using FileTypeChecker.Extensions;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Data;
using System.Text;
using static DroughtPrediction.Services.DataLoading.FileTypeChecker;
using SDS = Microsoft.Research.Science.Data;

namespace DroughtPrediction.Services.DataLoading;

public class DataLoadingService : IDataLoadingService
{  
    public async Task<DataTable> LoadFromXlsxFileData(IFormFile file)
    {
        FileTypeValidator.RegisterCustomTypes(typeof(Excel).Assembly);

        var fileStream = file.OpenReadStream();
        var isExcel = fileStream.Is<Excel>();
        
        if (isExcel == false)
        {
            throw new IncorrectFileException("The file format is incorrect");
        }

        var dataTable = new DataTable();

        using (MemoryStream stream = new())
        {
            await stream.FlushAsync();
            stream.Position = 0;
            await file.CopyToAsync(stream);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new(stream);

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

        List<DataRow> rowsToDelete = new List<DataRow>();

        foreach (DataColumn col in dataTable.Columns)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row[col.ColumnName].ToString().Equals("NaN"))
                {
                    rowsToDelete.Add(row);
                }
            }
        }

        foreach (DataRow row in rowsToDelete)
        {
            dataTable.Rows.Remove(row);
        }

        return dataTable;
    }

    public async Task<DataTable> LoadFromCsvFileData(IFormFile file)
    {
        DataTable dataTable = new DataTable();

        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        var fileStream = file.OpenReadStream();

        using (fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (TextReader reader = new StreamReader(stream, Encoding.UTF8))
            {

                var cache = new TypeConverterCache();
                cache.AddConverter<float>(new SingleConverter());
                cache.AddConverter<double>(new DoubleConverter());
                var csv = new CsvReader(reader,
                    new CsvHelper.Configuration.CsvConfiguration(global::System.Globalization.CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true
                    });
                csv.Read();
                csv.ReadHeader();

                using (var dr = new CsvDataReader(csv))
                {
                    dataTable.Load(dr);
                }
            }
        }

        List<DataRow> rowsToDelete = new List<DataRow>();

        foreach (DataColumn col in dataTable.Columns)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row[col.ColumnName].ToString().Equals("NaN"))
                {
                    rowsToDelete.Add(row);
                }
            }
        }

        foreach (DataRow row in rowsToDelete)
        {
            dataTable.Rows.Remove(row);
        }

        dataTable.AcceptChanges();

        return dataTable;
    }

    public async Task<DataTable> FileLoader(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName);

        DataTable data = new DataTable();

        if (fileExtension == ".xlsx")
        {
            data = await LoadFromXlsxFileData(file);
        }
        else if (fileExtension == ".csv")
        {
            data = await LoadFromCsvFileData(file);
        }
        else
        {
            throw new IncorrectFileException("The file format is incorrect");
        }

        return data;
    }

    public async Task<SDS.DataSet> LoadFromNetCdfFileData(IFormFile file)
    {
        FileTypeValidator.RegisterCustomTypes(typeof(NetCDF).Assembly);

        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(Path.GetTempPath(), fileName);  

        var fileStream = file.OpenReadStream();

        var isNetCDF = fileStream.Is<NetCDF>();
        if (isNetCDF == false)
        {
            throw new IncorrectFileException("The file format is incorrect");
        }

        using (fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        SDS.DataSet dataSet = SDS.DataSet.Open(filePath);

        return dataSet;
    }
}

