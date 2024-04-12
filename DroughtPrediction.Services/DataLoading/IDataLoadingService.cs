using Microsoft.AspNetCore.Http;
using System.Data;

namespace DroughtPrediction.Services.DataLoading;
public interface IDataLoadingService
{
    public Task<DataTable> LoadFileData(IFormFile file);
}
