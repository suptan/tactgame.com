app.controller('GMCtrl', ['$scope', '$filter', 'stockSearch', 'gameCtrl', 'gmService', function ($scope, $filter, stockSearch, gameCtrl, gmService) {
    
    $scope.init = function () {
        // Set game turn
        $scope.currentTurn();
        $scope.marketStatus = 'OPENED';
        $scope.marketData = new Array();

        // Get market data from json
        stockSearch.searchStockList(function (response) {
            $scope.stockList = new Array();
            $scope.stockListUsed = new Array();
            // Create list of stocks name
            angular.forEach(response.ResponseMessage, function (value, key) {
                $scope.stockList.push(value.Name);
                $scope.stockListUsed.push(true);
            });
        }, function (response) {
            console.log(response);
        }).then(function () {
            // Initialize market data
            updateMarketData();

            // Set score board
            playerData();
        });

        
    }

    // Change game turn
    $scope.nextTurn = function () {
        gameCtrl.nextTurn(function (response) {
            $scope.currentTurn = response.ResponseMessage;
            updateMarketData();
            playerData();
        }, function (response) {
            console.log(response);
        });
    }

    $scope.currentTurn = function () {
        gameCtrl.currentTurn(function (response) {
            $scope.currentTurn = response.ResponseMessage;
        }, function (response) {
            console.log(response);
        });
    }

    $scope.resetTurn = function () {
        gameCtrl.resetTurn(function (response) {
            $scope.currentTurn = response.ResponseMessage;
            updateMarketData();
        }, function (response) {
            console.log(response);
        });
    }

    $scope.marketOpen = function () {
        gameCtrl.marketOpen(function (response) {
            $scope.marketStatus = "OPENED";
        }, function (response) {
            console.log(response);
        });
    }

    $scope.marketClose = function () {
        gameCtrl.marketClose(function (response) {
            $scope.marketStatus = "CLOSED";
        }, function (response) {
            console.log(response);
        });
    }

    $scope.addToMarket = function (stockName) {
        changeStock(stockName, true);
    }

    $scope.removeFromMarket = function (stockName) {
        changeStock(stockName, false);
    }

    $scope.getMultiply = function (a, b) {
        return a * b;
    }

    $scope.getDiff = function (a, b) {
        return a - b;
    }

    $scope.$watch('marketData', function () {
        // Update stock market price in score board
        angular.forEach($scope.playerList, function (value, key) {
            angular.forEach(value.Stocks, function (value, key) {
                var marketStock = $filter('getByName')($scope.marketData, value.Name);
                if (marketStock) {
                    value.MarketPrice = marketStock.Price;
                    value.Dividend = marketStock.Dividend;
                }
            });
        });
        // Update stock list
        if ($scope.stockList) {
            for (var i = 0; i < $scope.stockList.length; i++) {
                $scope.stockListUsed[i] = $filter('getByName')($scope.marketData, $scope.stockList[i]) ? true : false;
            }
        }
    });

    function changeStock(stockName, isAdd) {
        gmService.changeStockInMarket(stockName, isAdd, function (response) {
            // Use http post with then get different json format
            if (response.data.IsSuccess) {
                $scope.marketData = response.data.ResponseMessage;
            } else { // Error from process
                // If error is redirect
                if (response.data.ResponseMessage.uri) {
                    $(location).attr('href', response.uri);
                }

            }
        }, function (response) {
            console.log(response);
        });
    }

    function updateMarketData() {
        stockSearch.searchMarketList(function (response) {
            $scope.marketData = response.ResponseMessage;
        }, function (response) {
            console.log(response);
        });
    }

    function playerData() {
        gmService.searchPlayerList(function (response) {
            $scope.playerList = response.ResponseMessage;
            // Calculate summation of investment
            angular.forEach($scope.playerList, function (value, key) {
                angular.forEach(value.Stocks, function (value, key) {
                    // Calculate stock market price
                    for (var i = 0; i < $scope.marketData.length; i++) {
                        if ($scope.marketData[i].Name === value.Name) {
                            value.MarketPrice = $scope.marketData[i].Price;
                            value.Dividend = $scope.marketData[i].Dividend;
                            break;
                        }
                    }
                });
            });
        }, function (response) {
            console.log(response);
        });
    }
}]);