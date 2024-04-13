using DroughtPrediction.Communication.Requests;
using DroughtPrediction.Communication.Responses;
using DroughtPrediction.Exceptions;
using DroughtPrediction.Services.DataLoading;
using System.Data;
using System.Globalization;
using System.Text;
using Tensorflow;
using SDS = Microsoft.Research.Science.Data;
using TensorNP = Tensorflow.NumPy;

namespace DroughtPrediction.Services.DataProcessing;
public class DataProcessService : IDataProcessService
{
    private readonly IDataLoadingService _dataLoadingService;

    public DataProcessService(IDataLoadingService dataLoadingService) 
    { 
        _dataLoadingService = dataLoadingService;
    }

    public List<DateTime> GetMonthValues(DataTable dataSet)
    {
        List<DateTime> monthValues = new();
        string format = "dd/MM/yyyy";

        foreach (DataRow row in dataSet.Rows)
        {
            string value = row[1].ToString();
            DateTime data = DateTime.ParseExact(value, format, null);
            monthValues.Add(data);
        }

        return monthValues;
    }

    public SpeiDataReturn GetSpeiValues(DataTable dataSet)
    {
        List<double> speiValues = new();
        List<double> normalizedSpeiValues = new();

        double minValue = double.MaxValue;
        double maxValue = double.MinValue;

        foreach (DataRow row in dataSet.Rows)
        {
            double value = Convert.ToDouble(row[0]);
            speiValues.Add(value);

            if (value < minValue)
            {
                minValue = value;
            }
            if (value > maxValue)
            {
                maxValue = value;
            }
        }

        foreach (DataRow row in dataSet.Rows)
        {
            double value = Convert.ToDouble(row[0]);
            double normalizedValue = (value - minValue) / (maxValue - minValue);
            normalizedSpeiValues.Add(normalizedValue);
        }

        SpeiDataReturn dataReturn = new()
        {
            SpeiValues = speiValues,
            NormalizedSpeiValues = normalizedSpeiValues
        };

        return dataReturn;
    }

    public async Task<SplittedDataSet> SplitSIntoTestAndTrainData(DataTable dataTable)
    {
        var normalizedSpei = GetSpeiValues(dataTable);
        var monthData = GetMonthValues(dataTable);

        double testPercentage = 0.3;

        int testSize = (int)(normalizedSpei.NormalizedSpeiValues.Count * testPercentage);

        var TrainData = normalizedSpei.NormalizedSpeiValues.Take(normalizedSpei.NormalizedSpeiValues.Count - testSize).ToList();
        var TestData = normalizedSpei.NormalizedSpeiValues.Skip(normalizedSpei.NormalizedSpeiValues.Count - testSize).ToList();

        var monthTrainData = monthData.Take(monthData.Count - testSize).ToList();
        var monthTestData = monthData.Skip(monthData.Count - testSize).ToList();

        SplittedDataSet splittedDataSet = new()
        {
            monthTestData = monthTestData,
            monthTrainData = monthTrainData,
            TestData = TestData,
            TrainData = TrainData
        };

        return splittedDataSet;
    }

    public async Task<byte[]> ExtractBalanceFromNetCdfFileData(SDS.DataSet dataSet, BalanceCoordinatesObjectJson balanceCoordinatesObjectJson)
    {
   
        double lat_seiLa;
        double lon_seiLa;

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

    public RearangeTimeSeriesOutput TimeSeriesRearange<T>(List<T> dataStr, int window, int predictionPoints)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        List<double> data = [];

        if (dataStr.GetType().GetGenericArguments()[0] == typeof(DateTime))
        {
            List<DateTime> dateTimeList = dataStr.ConvertAll(x => Convert.ToDateTime(x));

            data = dateTimeList.Select(dateTime =>
            {
                DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                return (double)dateTimeOffset.ToUnixTimeSeconds();
            }).ToList();
        }
        else
        {
            data = dataStr.ConvertAll(x => Convert.ToDouble(x));
        }

        List<int> outIndex = new();

        for (int i = window; i < data.Count; i += window)
        {
            outIndex.Add(i);
        }

        NumSharp.NDArray outArray = NumSharp.np.array(outIndex.Select(index => data[index]).ToArray());
        int linX = outArray.shape[0];

        List<double> IN = new();

        for (int i = 0; i < window * linX; i++)
        {
            IN.Add(data[i]);
        }

        Shape shape = new(linX, window, 1);
        NumSharp.NDArray reshapedIN = NumSharp.np.reshape(IN.ToArray(), shape);

        NumSharp.NDArray outFinal = reshapedIN[$":, {reshapedIN.shape[1] - predictionPoints}:, "];
        NumSharp.NDArray inFinal = reshapedIN[$":, :{reshapedIN.shape[1] - predictionPoints},  "];

        var sizeOne = outFinal.size;
        var sizeTwo = inFinal.size;

        var INConverted = ConvertNumSharpToTensorflow(inFinal);
        var OUTConverted = ConvertNumSharpToTensorflow(outFinal);

        RearangeTimeSeriesOutput output = new()
        {
            InputData = INConverted,
            OutputData = OUTConverted,
        };

        return output;
    }

    private TensorNP.NDArray ConvertNumSharpToTensorflow(NumSharp.NDArray numSharpArray)
    {
        double[,,] multiArray = numSharpArray.ToMuliDimArray<double>() as double[,,];

        var tensorFlowArray = TensorNP.np.array(multiArray);

        return tensorFlowArray;
    }

    private static List<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
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


