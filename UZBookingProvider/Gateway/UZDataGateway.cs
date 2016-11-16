﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using UZBookingProvider;
using UZBookingProvider.Domain;

namespace UZBookingProvider
{
    class UZDataGateway: IUZDataGateway, IDisposable
    {
        #region Fields: Private

        private bool _disposed = false;
        private IHttpRequestExecutor<FormUrlEncodedContent> _requestExecutor;
        private UZAPIConfig apiConfig;

        #endregion

        #region Methods: Private

        private FormUrlEncodedContent SerializeRequest(object request) {
            var json = JsonConvert.SerializeObject(request);
            var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return new FormUrlEncodedContent(jsonDict.ToArray());
        }

        private T DeserializeResponse<T>(string response) where T: UZSet, new() {
            try {
                var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                object error;
                if (jsonDict.TryGetValue("error", out error) && Convert.ToBoolean(error)) {
                    throw new Exception(jsonDict["value"] as string);
                }
                return JsonConvert.DeserializeObject<T>(response);
            } catch (Exception e) {
                var errorResponse = new T();
                errorResponse.ErrorMessage = e.Message;
                errorResponse.IsError = true;
                return errorResponse;
            }
        }

        private void Dispose(bool disposing) {
            if (!_disposed && disposing) {
                if (_requestExecutor != null) {
                    var re = (IDisposable)_requestExecutor;
                    _requestExecutor = null;
                    re.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion

        #region Constructors: Public

        public UZDataGateway(UZAPIConfig config) {
            apiConfig = config;
            var baseURI = string.Format("{0}/{1}", config.Host, config.Culture);
            _requestExecutor = new UZHttpRequestExecutor(baseURI, new UZToken());
            _requestExecutor.InitConnection();
       } 

        #endregion

        #region Methods: Public

        public async Task<UZTrainSet> GetTrains(UZTrainsRequest request) {
            var response = await _requestExecutor.PostAsync(apiConfig.TrainsURI, SerializeRequest(request));
            var trainSet = DeserializeResponse<UZTrainSet>(response);
            trainSet.OwnerRequest = request;
            return trainSet;
        }

        public async Task<UZCoachSet> GetCoaches(UZCoachesRequest request) {
            var response = await _requestExecutor.PostAsync(apiConfig.CoachesURI, SerializeRequest(request));
            var coachSet = DeserializeResponse<UZCoachSet>(response);
            coachSet.OwnerRequest = request;
            return coachSet;
        }

        public async Task<UZPlacesSet> GetPlaces(UZPlacesRequest request) {
            var response = await _requestExecutor.PostAsync(apiConfig.PlacesURI, SerializeRequest(request));
            var placesSet = DeserializeResponse<UZPlacesSet>(response);
            placesSet.OwnerRequest = request;
            return placesSet;
        }

        public async Task<UZCardSet> AddPlaceToCard(UZCardRequest request) {
            var response =  await _requestExecutor.PostAsync(apiConfig.CardURI, SerializeRequest(request));
            var cardSet = DeserializeResponse<UZCardSet>(response);
            cardSet.OwnerRequest = request;
            cardSet.Cookies = _requestExecutor.Cookies;
            return cardSet;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}