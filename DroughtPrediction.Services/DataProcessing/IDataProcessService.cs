using DroughtPrediction.Communication;
using Microsoft.AspNetCore.Http;
using NumSharp;
using System.Data;
using TensorNP = Tensorflow.NumPy;

namespace DroughtPrediction.Services.DataProcessing;
public interface IDataProcessService
{

    public SpeiDataReturn GetSpeiValues(DataTable dataSet);
    public List<DateTime> GetMonthValues(DataTable dataSet);
    public Task<SplittedDataSet> SplitSIntoTestAndTrainData(IFormFile file);
    public (TensorNP.NDArray, TensorNP.NDArray) TimeSeriesRearange<T>(List<T> data, int janela, int predictionPoints);
}
