app.controller('SignUpCtrl', ['$scope', 'signUpService', function ($scope, signUpService) {
    
    $scope.userRegex = "\\w{6,10}";
    $scope.passRegex = "\\w{6,20}";

    $scope.signUp = function () {
        // Clear error msg
        $scope.userErrorMsg = "";
        $scope.passErrorMsg = "";
        $scope.retyPassErrorMsg = "";
        $scope.existedMsg = "";

        // Check username and password match condition
        $scope.userErrorMsg = $scope.username ? "" : "Username can contain only letters and numbers from 6 - 10 character.";
        // Check password match condition
        $scope.passErrorMsg = $scope.password ? "" : "Password can contain only letters and numbers from 6 - 20 character.";
        // Check re-password match with password
        $scope.retyPassErrorMsg = $scope.rePassword && $scope.rePassword === $scope.password ? "" : "Password does not matched.";

        if ($scope.userErrorMsg && $scope.passErrorMsg && $scope.retyPassErrorMsg) return;

        signUpService.signUp($scope.username, $scope.password, function (response) {
            if (response.IsSuccess) {
                $scope.createdMsg = response.ResponseMessage;
                // Redirect user to login
                setInterval(function () {
                    var uri = window.location.origin;
                    $(location).attr('href', uri);
                }, 2000);
            } else {
                $scope.existedMsg = response.ResponseMessage;
            }
        }, function (response) {
            console.log(response);
        });
        
    }
}]);