using DroughtPrediction.Communication.Responses;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace DroughtPrediction.MachineLearning.NeuralNetwork;
public interface ITrainNeuralNetworkService
{
    public Task<byte[]> TrainModel(DataTable file);
    //public Task<NeuralNetworkEvaluationResult> TrainModel(DataTable file);
    public Task<byte[]> LoadModel(IFormFile file, DataTable dataTable);
}
