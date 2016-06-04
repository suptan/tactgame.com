app.service('loginService', ['$http', function ($http) {

    this.searchPlayerList = function (username, password, successCallback, errorCallback) {
        return $http.post("/Login/UserLogin", {
            username: username,
            password: password
        })
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }

    this.logoff = function (successCallback, errorCallback) {
        return $http.post("/Login/Logoff", {})
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }
}]);