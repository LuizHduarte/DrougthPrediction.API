using DroughtPrediction.Communication;
using DroughtPrediction.Exceptions;
using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Python.Runtime;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Linq;
using SDS = Microsoft.Research.Science.Data;
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

    private class NetCDF : FileType, IFileType
    {
        public const string TypeName = "NetCDF";
        public const string TypeExtension = "nc";
        private static readonly byte[] MagicBytes = [0x89, 0x48, 0x44, 0x46];

        public NetCDF() : base(TypeName, TypeExtension, MagicBytes) { }
    }

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

    public async Task<byte[]> ExtractBalanceFromNetCdfFileData(IFormFile file, BalanceCoordinatesObjectJson balanceCoordinatesObjectJson)
    {
        FileTypeValidator.RegisterCustomTypes(typeof(NetCDF).Assembly);

        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        var fileStream = file.OpenReadStream();
        var isExcel = fileStream.Is<Excel>();

        var isNetCDF = fileStream.Is<NetCDF>();
        if (isNetCDF == false)
        {
            throw new IncorrectFileException("The file format is incorrect");
        }

        using (fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        double lat_seiLa;
        double lon_seiLa; 

        using (SDS.DataSet dataSet = SDS.DataSet.Open(filePath))
        {
            var time = dataSet["time"].GetData();

            var firstYear = new DateTime(1966, 01, 01);
            var lastYear = new DateTime(2020, 12, 31);

            var lat = dataSet["latitude"].GetData();
            var lon = dataSet["longitude"].GetData();

            float[] latArray = (float[])lat;
            float[] lonArray = (float[])lon;

            if (lonArray.Contains(balanceCoordinatesObjectJson.Longitude) && latArray.Contains(balanceCoordinatesObjectJson.Latitude))
            {
                lat_seiLa = balanceCoordinatesObjectJson.Latitude;
                lon_seiLa = balanceCoordinatesObjectJson.Longitude;
            }
            else
            {
                throw new IncorrectLatitudeOrLongitudeException("The latitude/longitude does not exists or are incorrect.");
            }

            List<DateTime> dateRange = GetDateRange(firstYear, lastYear);

            var df = new DataTable();
            df.Columns.Add("Date", typeof(DateTime));
            df.Columns.Add("Balance", typeof(double));
            
            for (int i = 0; i < dateRange.Count; i++)
            {
                DataRow row = df.NewRow();
                row["Date"] = dateRange[i];
                row["Balance"] = 0.0;
                df.Rows.Add(row);
            }

            float[] sq_diff_lat = new float[lat.Length];
            for (int i = 0; i < lat.Length; i++)
            {
                sq_diff_lat[i] = (float)Math.Pow(latArray[i] - lat_seiLa, 2);
            }

            double[] sq_diff_lon = new double[lon.Length];
            for (int i = 0; i < lon.Length; i++)
            {
                sq_diff_lon[i] = Math.Pow(lonArray[i] - lon_seiLa, 2);
            }

            int min_index_lat = Array.IndexOf(sq_diff_lat, sq_diff_lat.Min());
            int min_index_lon = Array.IndexOf(sq_diff_lon, sq_diff_lon.Min());

            var balance = dataSet.Variables["w"].GetData();

            for (int t_index = 0; t_index < dateRange.Count; t_index++)
            {
                Console.WriteLine("Recording the value for : " + dateRange[t_index].ToString());
                df.Rows[t_index]["Balance"] = balance.GetValue(t_index, min_index_lat, min_index_lon);
            }

            StringBuilder sb = new StringBuilder();

            string[] columnNames = df.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            sb.AppendLine(string.Join(";", columnNames));

            foreach (DataRow row in df.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                sb.AppendLine(string.Join(";", fields));
            }

            byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

            return buffer;
        }
    }

    public static List<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
    {
        var dateRange = new List<DateTime>();

        while (startDate <= endDate)
        {
            dateRange.Add(startDate);
            startDate = startDate.AddMonths(1);
        }

        return dateRange;
    }
}

