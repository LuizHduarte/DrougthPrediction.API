using Numpy;

namespace DroughtPrediction.Communication.Responses;
public class OrquestredDataForNeuralNetworkPrediction
{
    public NDarray DataX { get; set; }
    public NDarray DataY { get; set; }
    public NDarray DataXMonth { get; set; }
    public NDarray DataYMonth { get; set; }
}
