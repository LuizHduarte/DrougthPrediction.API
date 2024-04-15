using DroughtPrediction.MachineLearning.NeuralNetwork;
using DroughtPrediction.Services.DataLoading;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace DroughtPrediction.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkController : ControllerBase
{
    [HttpPost]
    [Route("TrainLSTMNeuralNetwork")]
    public async Task<IActionResult> TrainNetworkFromXlsx(IFormFile file, [FromServices] ITrainNeuralNetworkService service, [FromServices] IDataLoadingService dataLoadingService )
    {
        var test = await dataLoadingService.LoadFromXlsxFileData(file);
        var result = await service.TrainModel(test);
        /*
        var response = new
        {
            data = new { 
                result.MeanSquaredError,
                result.RooMeanSquaredError,
                result.AbsoluteError,
                result.RSquared
            },
        };
        */

        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Cria uma nova entrada no arquivo zip
                var zipArchiveEntryLoss = archive.CreateEntry("LossHistory.png", CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntryLoss.Open())
                {
                    zipStream.Write(result.LossHitory, 0, result.LossHitory.Length);
                }
                var zipArchiveEntryValues = archive.CreateEntry("PredictedValues.png", CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntryValues.Open())
                {
                    zipStream.Write(result.PredictedValues, 0, result.PredictedValues.Length);
                }
                var zipArchiveEntryModel = archive.CreateEntry("modelWeights.h5", CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntryModel.Open())
                {
                    zipStream.Write(result.ModelWeights, 0, result.ModelWeights.Length);
                }
            }
            return File(memoryStream.ToArray(), "application/zip", "NeuralNetworkResponse.zip");
        }
    }

    [HttpPost]
    [Route("LoadLstmModel")]
    public async Task<IActionResult> LoadModel(IFormFile modelWeight, IFormFile xlsx, [FromServices] ITrainNeuralNetworkService service, [FromServices] IDataLoadingService dataLoadingService)
    {
        var request = await dataLoadingService.LoadFromXlsxFileData(xlsx);
        var response = await service.LoadModel(modelWeight, request);
        return File(response, "application/octet-stream", "modelWeights.png");
    }
}


