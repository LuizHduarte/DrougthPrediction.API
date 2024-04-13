namespace DroughtPrediction.MachineLearning;
public class LstmNeuralNetwork
{
    public static readonly int TotalPoints = 12;
    public static readonly int PredictionPoints = 6;
    public static readonly int hiddenUnits = 10;
    public static readonly int NumberOfEpochs = 30;
    public static readonly double ParcelDataTrain = 0.7;
    public static readonly int InputShape = TotalPoints - PredictionPoints;
}
