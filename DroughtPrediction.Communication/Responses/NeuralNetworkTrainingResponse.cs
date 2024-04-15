namespace DroughtPrediction.Communication.Responses;
public class NeuralNetworkTrainingResponse
{
    public byte[] LossHitory { get; set; } = [];
    public byte[] PredictedValues { get; set; } = [];
    public byte[] ModelWeights { get; set; } = [];
}
