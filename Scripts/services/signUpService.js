app.service('signUpService', ['$http', function ($http) {

    this.signUp = function (username, password, successCallback, errorCallback) {
        return $http.post("/Account/UserSignUp", {
            username: username,
            password: password
        })
            .success(function (response) {
                successCallback(response);
            }).error(function (response) {
                errorCallback(response);
            });
    }
}]);