app.controller('AcctCtrl', ['$scope', 'loginService', function ($scope, loginService) {
    
    $scope.userRegex = "\\w{6,10}";
    $scope.passRegex = "\\w{6,20}";
    $scope.username = "";
    $scope.password = "";

    $scope.login = function ($event) {
        // Show login progress
        var $this = $($event.currentTarget);
        $this.button('loading');
        // Clear error msg
        $scope.loginErrorMsg = "";
        // Check username and password match condition
        if ($scope.username && $scope.password) {
            loginService.searchPlayerList($scope.username, $scope.password, function (response) {
                // If username registered, redirect to role page
                if (response.IsSuccess) {
                    var uri = window.location.href + response.ResponseMessage;
                    $(location).attr('href', uri);
                } else {
                    $scope.loginErrorMsg = response.ResponseMessage;
                }
            }, function (response) {
                console.log(response);
            });
        } else {
            $scope.loginErrorMsg = "Username or password is incorrect";
        }
        $this.button('reset');
    }

    $scope.loginShortcut = function ($event) {
        // Check for enter press
        if (event.keyCode == 13) {
            $scope.login(angular.element(document.querySelector('#loginBtn')));
        }
    }

    $scope.logoff = function () {
        $(location).attr('href', 'Login\\Logoff');
    }
}]);