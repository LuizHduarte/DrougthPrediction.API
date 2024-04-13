using DroughtPrediction.Communication;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace DroughtPrediction.Services.DataLoading;
public interface IDataLoadingService
{
    public Task<DataTable> LoadFromXlsxFileData(IFormFile file);
    public Task<byte[]> ExtractBalanceFromNetCdfFileData(IFormFile file, BalanceCoordinatesObjectJson balanceCoordinatesObjectJson);
}
