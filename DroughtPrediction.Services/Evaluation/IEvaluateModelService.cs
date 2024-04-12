using Tensorflow;
using Tensorflow.NumPy;

namespace DroughtPrediction.Services.Evaluation;
public interface IEvaluateModelService
{
    public float CalculateAbsoluteError(Tensor predictedData, NDArray trueData);
    public float CalculateMeanSquaredError(Tensor y_true, Tensor y_pred);
    public float CalculateRooMeanSquaredError(Tensor y_true, Tensor y_pred);
    public float CalculateRSquared(Tensor y_true, Tensor y_pred);
}
