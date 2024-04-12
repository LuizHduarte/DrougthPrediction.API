using Tensorflow;
using Tensorflow.NumPy;

namespace DroughtPrediction.DataVisualization;

public interface IDataVisualizationService
{
    public byte[] LossVisualization(List<double> lossValues);
    public byte[] PredictedDataVisualization(Tensor predictedValues, NDArray trueValues, NDArray months);
    public byte[] SpeiDataVisualization(List<double> speiData, List<DateTime> monthData);
}