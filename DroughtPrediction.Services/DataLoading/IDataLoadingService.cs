using Microsoft.AspNetCore.Http;
using System.Data;
using SDS = Microsoft.Research.Science.Data;

namespace DroughtPrediction.Services.DataLoading;
public interface IDataLoadingService
{
    public Task<DataTable> LoadFromXlsxFileData(IFormFile file);
    public Task<DataTable> LoadFromCsvFileData(IFormFile file);
    public Task<SDS.DataSet> LoadFromNetCdfFileData(IFormFile file);
    public Task<DataTable> FileLoader(IFormFile file);
}
