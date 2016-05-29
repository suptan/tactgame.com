app.service('gameCtrl', ['$http', function ($http) {

    this.nextTurn = function (successCallback, errorCallback) {
        return $http.post("/GM/NextTurn", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.currentTurn = function (successCallback, errorCallback) {
        return $http.post("/GM/CurrentTurn", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.resetTurn = function (successCallback, errorCallback) {
        return $http.post("/GM/ResetTurn", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.marketClose = function (successCallback, errorCallback) {
        return $http.post("/GM/MarketClose", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.marketOpen = function (successCallback, errorCallback) {
        return $http.post("/GM/MarketOpen", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }
}]);