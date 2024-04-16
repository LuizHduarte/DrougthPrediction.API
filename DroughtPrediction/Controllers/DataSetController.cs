using DroughtPrediction.Communication.Requests;
using DroughtPrediction.DataVisualization;
using DroughtPrediction.Services.DataLoading;
using DroughtPrediction.Services.DataProcessing;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DroughtPrediction.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DataManipulation : ControllerBase
{
    [HttpPost]
    [Route("Extract/SpeiGraph/CSV")]
    public async Task<IActionResult> GetSpeiData([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file, [FromServices] IDataVisualizationService dataVisualizationService )
    {
        var data = await dataLoadingService.FileLoader(file);

        var speiValues = dataProcessService.GetSpeiValues(data);
        var monthValues = dataProcessService.GetMonthValues(data);

        var response = dataVisualizationService.SpeiDataVisualization(speiValues.SpeiValues, monthValues);

        return File(response, "application/octet-stream", "SpeiTimeSeries.png");
    }

    [HttpPost]
    [Route("Extract/Balance/NetCDF")]
    public async Task<IActionResult> GetBalance([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file)
    {
        var netCDFile = await dataLoadingService.LoadFromNetCdfFileData(file);
        var response = await dataProcessService.ExtractBalanceFromNetCdfFileData(netCDFile);

        return File(response, "text/csv", "BalanceData.csv");
    }

    [HttpPost]
    [Route("Extract/Balance/Coordinates/NetCDF")]
    public async Task<IActionResult> GetBalanceFromCoordinates([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file, [FromForm] BalanceCoordinatesObjectJson balanceCoordinatesObjectJson)
    {
        var netCDFile = await dataLoadingService.LoadFromNetCdfFileData(file);
        var response = await dataProcessService.ExtractBalanceFromCoordinatesNetCdfFileData(netCDFile, balanceCoordinatesObjectJson);

        return File(response, "text/csv", "BalanceData.csv");
    }

    [HttpPost]
    [Route("Calculate/SPEI/FromBalance/CSV")]
    public async Task<IActionResult> GetSpei([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file )
    {
        var data = await dataLoadingService.FileLoader(file);

        var response = await dataProcessService.CalculateSPEIFromBalance(data);

        return File(response, "text/csv", "SPEI.csv");
    }
}


