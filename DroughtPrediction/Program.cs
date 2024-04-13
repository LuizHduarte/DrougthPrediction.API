using DroughtPrediction.DataVisualization;
using DroughtPrediction.MachineLearning.Evaluation;
using DroughtPrediction.MachineLearning.NeuralNetwork;
using DroughtPrediction.Services.DataLoading;
using DroughtPrediction.Services.DataProcessing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Drought Prediction API", Version = "v1" });
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddScoped<ITrainNeuralNetworkService, TrainNeuralNetworkService>();
builder.Services.AddScoped<IDataProcessService, DataProcessService>();
builder.Services.AddScoped<IDataLoadingService, DataLoadingService>();
builder.Services.AddScoped<IEvaluateModelService, EvaluateModelService>();
builder.Services.AddScoped<IDataVisualizationService, DataVisualizationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
