using Tensorflow;
using Tensorflow.Keras.Losses;
using Tensorflow.NumPy;
using static Tensorflow.Binding;


namespace DroughtPrediction.Services.Evaluation;
public class EvaluateModelService : IEvaluateModelService
{
    public float CalculateAbsoluteError(Tensor predictedData, NDArray trueData)
    {
        var tensorToNumpy = predictedData.numpy();

        var absoluteDifference = tf.abs(tensorToNumpy - trueData);

        var meanAbsoluteError = tf.reduce_mean(absoluteDifference);

        // Obtenha os dados do NDArray como um array de float
        float[] data = meanAbsoluteError.ToArray<float>();

        return data[0];
    }
    public float CalculateMeanSquaredError(Tensor y_true, Tensor y_pred)
    {
        var squaredDifference = tf.square(y_true - y_pred);

        var meanSquaredError = tf.reduce_mean(squaredDifference);

        float[] data = meanSquaredError.ToArray<float>();

        return data[0];
    }

    public float CalculateRooMeanSquaredError(Tensor y_true, Tensor y_pred)
    {
        var squaredDifference = tf.square(y_true - y_pred);

        var meanSquaredError = tf.reduce_mean(squaredDifference);

        var rootMeanSquaredError = tf.sqrt(meanSquaredError);

        float[] data = rootMeanSquaredError.ToArray<float>();

        return data[0];
    }

    // Função para calcular o coeficiente de determinação R²
    public float CalculateRSquared(Tensor y_true, Tensor y_pred)
    {
        var totalError = tf.reduce_sum(tf.square(y_true - tf.reduce_mean(y_true)));

        var residualError = tf.reduce_sum(tf.square(y_true - y_pred));

        var rSquared = 1 - (residualError / totalError);

        float[] data = rSquared.ToArray<float>();

        return data[0];

    }
}
