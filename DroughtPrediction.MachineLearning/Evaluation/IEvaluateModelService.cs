using Tensorflow;
using Tensorflow.NumPy;

namespace DroughtPrediction.MachineLearning.Evaluation;
public interface IEvaluateModelService
{
    public float CalculateAbsoluteError(Tensor predictedData, NDArray trueData);
    public float CalculateMeanSquaredError(Tensor yTrue, Tensor yPred);
    public float CalculateRooMeanSquaredError(Tensor yTrue, Tensor yPred);
    public float CalculateRSquared(Tensor yTrue, Tensor yPred);
}
