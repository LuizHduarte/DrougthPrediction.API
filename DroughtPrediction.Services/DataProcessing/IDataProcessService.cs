﻿using DroughtPrediction.Communication.Requests;
using DroughtPrediction.Communication.Responses;
using System.Data;
using SDS = Microsoft.Research.Science.Data;

namespace DroughtPrediction.Services.DataProcessing;
public interface IDataProcessService
{
    public SpeiDataReturn GetSpeiValues(DataTable dataSet);
    public List<DateTime> GetMonthValues(DataTable dataSet);
    public Task<SplittedDataSet> SplitSIntoTestAndTrainData(DataTable file);
    public RearangeTimeSeriesOutput TimeSeriesRearange<T>(List<T> data, int window, int predictionPoints);
    public Task<byte[]> ExtractBalanceFromNetCdfFileData(SDS.DataSet dataSet);
    public Task<byte[]> ExtractBalanceFromCoordinatesNetCdfFileData(SDS.DataSet dataSet, BalanceCoordinatesObjectJson balanceCoordinatesObjectJson);
    public Task<OrquestredDataForNeuralNetwork> OrquestDataForNeuralNetworkTrain(DataTable dataTable);
    public Task<OrquestredDataForNeuralNetworkPrediction> OrquestDataForNeuralNetworkPrediction(DataTable dataTable);
    public Task<byte[]> CalculateSPEIFromBalance(DataTable dataTable);
}
