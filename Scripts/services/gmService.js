app.service('gmService', ['$http', function ($http) {

    this.searchPlayerList = function (successCallback, errorCallback) {
        return $http.post("/GM/PlayersSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.changeStockInMarket = function (name, flag, successCallback, errorCallback) {
        return $http.post("/GM/ChangeStockInMarket", {
            stockName: name,
            isAdd: flag
        }).then(successCallback, errorCallback);
            //.success(function (response) {
            //    successCallback(response);
            //}).error(function (response) {
            //    errorCallback(response);
            //});
    }
}]);