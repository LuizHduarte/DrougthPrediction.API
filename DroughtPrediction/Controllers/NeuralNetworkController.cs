using DroughtPrediction.MachineLearning.NeuralNetwork;
using DroughtPrediction.Services.DataLoading;
using Microsoft.AspNetCore.Mvc;

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

        Response.ContentType = "application/json";
        return File(result, "application/octet-stream", "modelWeights.h5");
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


