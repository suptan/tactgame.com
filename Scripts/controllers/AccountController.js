app.controller('AcctCtrl', ['$scope', 'portfolioService', function ($scope, portfolioService) {
    
    $scope.userRegex = "\\w{6,10}";
    $scope.passRegex = "\\w{6,20}";
    $scope.username = "";
    $scope.password = "";

    $scope.login = function () {
        if ($scope.username && $scope.password) {
            console.log("pass");
        }
    }

}]);