using DroughtPrediction.Communication.Responses;
using System.Data;

namespace DroughtPrediction.MachineLearning.NeuralNetwork;
public interface ITrainNeuralNetworkService
{
    public Task<NeuralNetworkEvaluationResult> TrainModel(DataTable file);
}
