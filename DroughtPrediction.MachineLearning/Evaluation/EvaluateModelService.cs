using Numpy;


namespace DroughtPrediction.MachineLearning.Evaluation;
public class EvaluateModelService : IEvaluateModelService
{
    public double CalculateAbsoluteError(NDarray yTrue, NDarray yPred)
    {
        var absoluteDifference = np.abs(yTrue - yPred);

        var meanAbsoluteError = np.mean(absoluteDifference);

        return meanAbsoluteError;
    }
    public double CalculateMeanSquaredError(NDarray yTrue, NDarray yPred)
    {
        var squaredDifference = np.square(yTrue - yPred);

        var meanSquaredError = np.mean(squaredDifference);

        return meanSquaredError;
    }

    public NDarray CalculateRooMeanSquaredError(NDarray yTrue, NDarray yPred)
    {

        var squaredDifference = np.square(yTrue - yPred);

        var meanSquaredError = np.mean(squaredDifference);

        var rootMeanSquaredError = np.sqrt(np.array(meanSquaredError));

        return rootMeanSquaredError;
    }

    // Função para calcular o coeficiente de determinação R²
    public NDarray CalculateRSquared(NDarray yTrue, NDarray yPred) 
    {
        var totalError = np.sum(np.square(yTrue - np.mean(yTrue)));

        var residualError = np.sum(np.square(yTrue - yPred));

        var rSquared = 1 - (residualError / totalError);

        return rSquared;
    }
}
