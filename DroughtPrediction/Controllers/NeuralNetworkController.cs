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

        var response = new
        {
            data = new { 
                result.MeanSquaredError,
                result.RooMeanSquaredError,
                result.AbsoluteError,
                result.RSquared
            },
        };

        Response.ContentType = "application/json";
        //return Ok(File(result.imageBytes, "application/octet-stream", "filename.png"));
        return File(result.imageBytes, "application/octet-stream", "filename.png");
    }
}


