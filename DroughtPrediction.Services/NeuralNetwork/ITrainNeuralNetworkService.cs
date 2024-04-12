using DroughtPrediction.Communication;
using Microsoft.AspNetCore.Http;

namespace DroughtPrediction.Services.NeuralNetwork;
public interface ITrainNeuralNetworkService
{
    public Task<NeuralNetworkEvaluationResult> TrainModel(IFormFile file);
}
