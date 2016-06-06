app.service('portfolioService', ['$http', function ($http) {

    this.searchStockList = function (successCallback, errorCallback) {
        return $http.post("/Player/PortfolioSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.searchMarketList = function (successCallback, errorCallback) {
        return $http.post("/Player/MarketSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.stockTrade = function (tradeCmd ,id, stockName, stockVol, successCallback, errorCallback) {
        var uri = "/Player/BuyStock";

        if (tradeCmd == 1) {
            uri = "/Player/SellStock";
        }

        return $http.post(uri, {
            id: id,
            name: stockName,
            vol: stockVol
        })
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.searchCommissionRate = function (successCallback, errorCallback) {
        return $http.post("/Player/CommissionRateSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }
}]);