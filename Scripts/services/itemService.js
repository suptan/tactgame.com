app.service('stockSearch', ['$http', function ($http) {

    this.searchStockList = function (successCallback, errorCallback) {
        return $http.post("/GM/StockListSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.searchMarketList = function (successCallback, errorCallback) {
        return $http.post("/GM/MarketSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

}]);