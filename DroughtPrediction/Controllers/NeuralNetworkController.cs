using DroughtPrediction.Communication;
using DroughtPrediction.Services.DataLoading;
using DroughtPrediction.Services.NeuralNetwork;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DroughtPrediction.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkController : ControllerBase
{
    [HttpPost]
    [Route("LoadFromXlsx")]
    public async Task<IActionResult> TrainNetworkFromXlsx(IFormFile file, [FromServices] ITrainNeuralNetworkService service )
    {
        var result = await service.TrainModel(file);

        var response = new
        {
            data = new { 
                result.MeanSquaredError,
                result.RooMeanSquaredError,
                result.AbsoluteError,
                result.RSquared
            },
        };

        byte[] data = Encoding.UTF8.GetBytes(response.data.ToString());
        Response.ContentType = "application/json";

        //return Ok(File(result.imageBytes, "application/octet-stream", "filename.png"));
        return File(result.imageBytes, "application/octet-stream", "filename.png");
    }

    [HttpPost]
    [Route("LoadFromNetCdf")]
    public async Task<IActionResult> TrainNetworkFromNetCDF(IFormFile file, [FromServices] IDataLoadingService service,[FromForm] BalanceCoordinatesObjectJson balanceCoordinatesObjectJson)
    {
        var result = await service.ExtractBalanceFromNetCdfFileData(file, balanceCoordinatesObjectJson);
        return File(result, "text/csv", "filename.csv");
    }
}


