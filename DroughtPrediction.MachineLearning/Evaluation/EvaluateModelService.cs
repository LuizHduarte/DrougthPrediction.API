using Tensorflow;
using Tensorflow.NumPy;
using static Tensorflow.Binding;


namespace DroughtPrediction.MachineLearning.Evaluation;
public class EvaluateModelService : IEvaluateModelService
{
    public float CalculateAbsoluteError(Tensor predictedData, NDArray trueData)
    {
        var tensorToNumpy = predictedData.numpy();

        var absoluteDifference = tf.abs(tensorToNumpy - trueData);

        var meanAbsoluteError = tf.reduce_mean(absoluteDifference);

        float[] data = meanAbsoluteError.ToArray<float>();

        return data[0];
    }
    public float CalculateMeanSquaredError(Tensor yTrue, Tensor yPred)
    {
        var squaredDifference = tf.square(yTrue - yPred);

        var meanSquaredError = tf.reduce_mean(squaredDifference);

        float[] data = meanSquaredError.ToArray<float>();

        return data[0];
    }

    public float CalculateRooMeanSquaredError(Tensor yTrue, Tensor yPred)
    {
        var squaredDifference = tf.square(yTrue - yPred);

        var meanSquaredError = tf.reduce_mean(squaredDifference);

        var rootMeanSquaredError = tf.sqrt(meanSquaredError);

        float[] data = rootMeanSquaredError.ToArray<float>();

        return data[0];
    }

    // Função para calcular o coeficiente de determinação R²
    public float CalculateRSquared(Tensor yTrue, Tensor yPred)
    {
        var totalError = tf.reduce_sum(tf.square(yTrue - tf.reduce_mean(yTrue)));

        var residualError = tf.reduce_sum(tf.square(yTrue - yPred));

        var rSquared = 1 - (residualError / totalError);

        float[] data = rSquared.ToArray<float>();

        return data[0];

    }
}
