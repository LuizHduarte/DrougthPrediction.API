namespace DroughtPrediction.Communication.Responses;
public class NeuralNetworkEvaluationResult
{
    public float AbsoluteError { get; set; }
    public float MeanSquaredError { get; set; }
    public float RooMeanSquaredError { get; set; }
    public float RSquared { get; set; }
    public byte[] imageBytes { get; set; } = [];
}
