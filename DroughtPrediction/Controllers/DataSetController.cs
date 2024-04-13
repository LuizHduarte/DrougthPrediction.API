using DroughtPrediction.Communication.Requests;
using DroughtPrediction.DataVisualization;
using DroughtPrediction.Services.DataLoading;
using DroughtPrediction.Services.DataProcessing;
using Microsoft.AspNetCore.Mvc;

namespace DroughtPrediction.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DataManipulation : ControllerBase
{
    [HttpPost]
    [Route("originalSpeiData")]
    public async Task<IActionResult> GetSpeiData([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file, [FromServices] IDataVisualizationService dataVisualizationService )
    {
        var data = await dataLoadingService.LoadFromXlsxFileData(file);
        var speiValues = dataProcessService.GetSpeiValues(data);
        var monthValues = dataProcessService.GetMonthValues(data);

        var response = dataVisualizationService.SpeiDataVisualization(speiValues.SpeiValues, monthValues);

        return File(response, "application/octet-stream", "SpeiData.png");
    }

    [HttpPost]
    [Route("LoadFromNetCdf")]
    public async Task<IActionResult> GetBalance([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file, [FromForm] BalanceCoordinatesObjectJson balanceCoordinatesObjectJson)
    {
        var netCDFile = await dataLoadingService.LoadFromNetCdfFileData(file);
        var response = await dataProcessService.ExtractBalanceFromNetCdfFileData(netCDFile, balanceCoordinatesObjectJson);

        return File(response, "text/csv", "filename.csv");
    }
}


