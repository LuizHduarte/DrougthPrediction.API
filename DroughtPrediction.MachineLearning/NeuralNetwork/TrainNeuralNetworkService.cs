using DroughtPrediction.DataVisualization;
using DroughtPrediction.Domain;
using DroughtPrediction.MachineLearning.Evaluation;
using DroughtPrediction.Services.DataProcessing;
using Keras.Layers;
using Keras.Models;
using Microsoft.AspNetCore.Http;
using System.Data;
namespace DroughtPrediction.MachineLearning.NeuralNetwork;

public class TrainNeuralNetworkService : ITrainNeuralNetworkService
{
    private readonly IDataProcessService _dataProcessService;
    private readonly IEvaluateModelService _evaluateModelService;
    private readonly IDataVisualizationService _dataVisualizationService;

    public TrainNeuralNetworkService(IDataProcessService dataProcessService, IEvaluateModelService evaluateModelService, IDataVisualizationService dataVisualizationService)
    {
        _dataProcessService = dataProcessService;
        _evaluateModelService = evaluateModelService;
        _dataVisualizationService = dataVisualizationService;
    }

    private Sequential DefineLstmModel()
    {
        var model = new Sequential();
        var input_shape = (LstmNeuralNetwork.InputShape,1);
        model.Add(new LSTM(units: LstmNeuralNetwork.hiddenUnits, activation: "relu", input_shape: input_shape));
        model.Add(new Dense(units: LstmNeuralNetwork.PredictionPoints, activation: "linear"));
        model.Add(new Dense(units: LstmNeuralNetwork.PredictionPoints, activation: "linear"));
        model.Add(new Dense(units: LstmNeuralNetwork.PredictionPoints, activation: "linear"));

        model.Compile(loss: "mean_squared_error", optimizer: "adam");
        model.Summary();

        return model;
    }

    public async Task<byte[]> TrainModel(DataTable dataTable)
    {
        var orquestredData = await _dataProcessService.OrquestDataForNeuralNetworkTrain(dataTable);
        var SplitedData = await _dataProcessService.SplitSIntoTestAndTrainData(dataTable);

        var model = DefineLstmModel();
        var history = model.Fit(orquestredData.trainDataX, orquestredData.trainDataY, batch_size: 1, epochs: LstmNeuralNetwork.NumberOfEpochs, verbose: 1);
        double[] lossHistory = history.HistoryLogs["loss"];
        var imageBytes = SaveModel(model);

        var predictionValues = model.Predict(orquestredData.testDataX);

        var absError = _evaluateModelService.CalculateAbsoluteError(orquestredData.testDataY, predictionValues);
        var mseError = _evaluateModelService.CalculateMeanSquaredError(orquestredData.testDataY, predictionValues);
        var rmseError = _evaluateModelService.CalculateRooMeanSquaredError(orquestredData.testDataY, predictionValues);
        var rSquared = _evaluateModelService.CalculateRSquared(orquestredData.testDataY, predictionValues);

        double[] predictedValuesList =  Array.ConvertAll(predictionValues.GetData<float>(), x => (double)x);
        double[] realValuesList = orquestredData.testDataY.GetData<double>();
        double[] monthValuesList = orquestredData.testDataYMonth.GetData<double>();

        //var lossImage = _dataVisualizationService.LossVisualization(lossValues);
        var CompareValues = _dataVisualizationService.PredictedDataVisualization(predictedValuesList, realValuesList, monthValuesList);

        /*
        NeuralNetworkEvaluationResult result = new()
        {
            AbsoluteError = absError,
            MeanSquaredError = mseError,
            RooMeanSquaredError = rmseError,
            RSquared = rSquared,
            imageBytes = CompareValues
        };
        */

        return imageBytes;
    }

    public async Task<byte[]> LoadModel(IFormFile file, DataTable dataTable)
    {
        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        var fileStream = file.OpenReadStream();

        using (fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var orquestredData = await _dataProcessService.OrquestDataForNeuralNetworkPrediction(dataTable);

        var model = DefineLstmModel();
        model.Compile(loss: "mean_squared_error", optimizer: "adam");
        model.Summary();

        model.LoadWeight(filePath);

        model.Summary();

        var predictionValues = model.Predict(orquestredData.DataX);

        var absError = _evaluateModelService.CalculateAbsoluteError(orquestredData.DataY, predictionValues);
        var mseError = _evaluateModelService.CalculateMeanSquaredError(orquestredData.DataY, predictionValues);
        var rmseError = _evaluateModelService.CalculateRooMeanSquaredError(orquestredData.DataY, predictionValues);
        var rSquared = _evaluateModelService.CalculateRSquared(orquestredData.DataY, predictionValues);

        double[] predictedValuesList = Array.ConvertAll(predictionValues.GetData<float>(), x => (double)x);
        double[] realValuesList = orquestredData.DataY.GetData<double>();
        double[] monthValuesList = orquestredData.DataYMonth.GetData<double>();

        var CompareValues = _dataVisualizationService.PredictedDataVisualization(predictedValuesList, realValuesList, monthValuesList);

        return CompareValues;

    }

    private byte[] SaveModel(Sequential model)
    {
        var initialPath = Path.GetTempPath();
        var temp = "tempWWW.h5";
        var tempPath = Path.Combine(initialPath, temp);

        model.SaveWeight(tempPath);

        var imageBytes = File.ReadAllBytes(tempPath);

        return imageBytes;
    }
}

