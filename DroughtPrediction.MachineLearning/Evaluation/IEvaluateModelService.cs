using Numpy;

namespace DroughtPrediction.MachineLearning.Evaluation;
public interface IEvaluateModelService
{
    public double CalculateAbsoluteError(NDarray predictedData, NDarray trueData);
    public double CalculateMeanSquaredError(NDarray yTrue, NDarray yPred);
    public NDarray CalculateRooMeanSquaredError(NDarray yTrue, NDarray yPred);
    public NDarray CalculateRSquared(NDarray yTrue, NDarray yPred);
}
