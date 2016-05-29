app.service('loginService', ['$http', function ($http) {

    this.searchPlayerList = function (successCallback, errorCallback) {
        return $http.post("/Login/PlayersSearch", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }
}]);