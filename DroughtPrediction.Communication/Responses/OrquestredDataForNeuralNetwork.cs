using Numpy;

namespace DroughtPrediction.Communication.Responses;
public class OrquestredDataForNeuralNetwork
{
    public NDarray trainDataX { get; set; }
    public NDarray trainDataY { get; set; }
    public NDarray testDataX { get; set; }
    public NDarray testDataY { get; set; }
    public NDarray trainDataYMonth { get; set; }
    public NDarray testDataYMonth { get; set; }
}
