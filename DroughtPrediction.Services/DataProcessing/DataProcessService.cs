using DroughtPrediction.Communication;
using DroughtPrediction.Services.DataLoading;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Globalization;
using System.Text;
using Tensorflow;
using TensorflowLib = Tensorflow;
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
        var monthValues = new List<DateTime>();
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
        var speiValues = new List<double>();
        var normalizedSpeiValues = new List<double>();

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

        // Normalizar os valores da primeira coluna
        foreach (DataRow row in dataSet.Rows)
        {
            double value = Convert.ToDouble(row[0]);
            double normalizedValue = (value - minValue) / (maxValue - minValue);
            normalizedSpeiValues.Add(normalizedValue); // Adicionar o valor normalizado à lista de valores normalizados
        }

        SpeiDataReturn dataReturn = new()
        {
            SpeiValues = speiValues,
            NormalizedSpeiValues = normalizedSpeiValues
        };

        return dataReturn;
    }

    public async Task<SplittedDataSet> SplitSIntoTestAndTrainData(IFormFile file)
    {
        var dt = await _dataLoadingService.LoadFromXlsxFileData(file);
        var normalizedSpei = GetSpeiValues(dt);
        var monthData = GetMonthValues(dt);

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

    public (TensorNP.NDArray, TensorNP.NDArray) TimeSeriesRearange<T>(List<T> dataStr, int janela, int predictionPoints)
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

        // Cálculo dos índices de saída
        List<int> OUT_indices = new List<int>();
        for (int i = janela; i < data.Count; i += janela)
        {
            OUT_indices.Add(i);
        }

        NumSharp.NDArray OUT = NumSharp.np.array(OUT_indices.Select(index => data[index]).ToArray());
        int lin_x = OUT.shape[0];

        // Cálculo do IN
        List<double> IN = new List<double>();
        for (int i = 0; i < janela * lin_x; i++)
        {
            IN.Add(data[i]);
        }

        TensorflowLib.Shape theShape = new(lin_x, janela, 1);
        NumSharp.NDArray reshapedIN = NumSharp.np.reshape(IN.ToArray(), theShape);

        TensorNP.NDArray reshapedINNew = TensorNP.np.reshape(IN.ToArray(), theShape);
        TensorNP.NDArray reshapedINNewTwo = TensorNP.np.reshape(IN.ToArray(), theShape);


        // Ajustando os valores de OUT e IN final
        NumSharp.NDArray OUT_final = reshapedIN[$":, {reshapedIN.shape[1] - predictionPoints}:, "];
        NumSharp.NDArray IN_final = reshapedIN[$":, :{reshapedIN.shape[1] - predictionPoints},  "];

        var sizeOne = OUT_final.size;
        var sizeTwo = IN_final.size;

        var INConverted = ConvertNumSharpToTensorflow(IN_final);
        var OUTConverted = ConvertNumSharpToTensorflow(OUT_final);

        return (INConverted, OUTConverted);
    }

    private TensorNP.NDArray ConvertNumSharpToTensorflow(NumSharp.NDArray numSharpArray)
    {
        // Convert NumSharp NDArray to a multi-dimensional array
        double[,,] multiArray = numSharpArray.ToMuliDimArray<double>() as double[,,];

        // Convert the multi-dimensional array to a TensorFlow NDArray
        var tensorflowArray = TensorNP.np.array(multiArray);

        return tensorflowArray;
    }


}


