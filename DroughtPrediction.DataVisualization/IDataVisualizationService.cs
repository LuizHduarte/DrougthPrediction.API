

namespace DroughtPrediction.DataVisualization;

public interface IDataVisualizationService
{
    public byte[] LossVisualization(List<double> lossValues);
    public byte[] PredictedDataVisualization(double[] predictedValues, double[] trueValues, double[] months);
    public byte[] SpeiDataVisualization(List<double> speiData, List<DateTime> monthData);
}