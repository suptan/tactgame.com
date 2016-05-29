app.controller('PlayerCtrl', ['$scope', '$filter', 'stockSearch', 'portfolioService', function ($scope, $filter, stockSearch, portfolioService) {
    
    $scope.tradeCmds = [{ 'id': 0, 'name': 'Buy' }, { 'id': 1, 'name': 'Sell' }];
    $scope.stocksTrade = new Array();
    $scope.tradeCmdSelected = 0;
    $scope.tradeVol = 0;
    $scope.tradePrice = 0.00;

    $scope.init = function () {
        // Update market data
        portfolioService.searchMarketList(function (response) {
            $scope.marketData = response.ResponseMessage;
            // Setup stocks name for trade
            angular.forEach($scope.marketData, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
            // Remove stock index from trade
            $scope.stocksTrade.splice($scope.stocksTrade.length - 1, 1);
            // Set first stock trade
            $scope.stockTradeSelected = $scope.marketData[0].Name;
            // Set first stock trade price
            $scope.tradePrice = $scope.marketData[0].Price;
        }, function (response) {
            console.log(response);
        });

        updatePortfolio();
    }

    $scope.getPortFolioPL = function () {
        var result = 0;
        // All stocks in portfolio multiple by market price
        angular.forEach($scope.portfolio, function (value, key) {
            result += (value.MarketPrice ? value.MarketPrice : 0) * value.Vol;
        });
        return result;
    }

    // Setup stocks name for trade
    $scope.changeTradeCmd = function () {
        $scope.stocksTrade = [];

        if ($scope.tradeCmdSelected == 0) {
            // Trade stock in market
            angular.forEach($scope.marketData, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
            $scope.stocksTrade.splice($scope.stocksTrade.length - 1, 1);
        } else {
            // Trade stock in portfolio
            angular.forEach($scope.portfolio, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
        }
    }

    // Setup stocks price for trade
    $scope.changeTradePrice = function () {
        // Find stock price in market
        var stock = $filter('getByName')($scope.marketData, $scope.stockTradeSelected);
        $scope.tradePrice = stock.Price;
    }

    // Buy/Sell selected stock
    $scope.stockTrade = function () {
        //var stockId = $filter('getById')($scope.marketData, $scope.stockTradeSelected).ID;
        portfolioService.stockTrade($scope.tradeCmdSelected, 0, $scope.stockTradeSelected, $scope.tradeVol, function (response) {
            if (response.IsSuccess) {
                updatePortfolio();
            } else {
                console.log(response.ResponseMessage);
            }
        }, function (response) {
            console.log(response);
        });

        $scope.tradeVol = 0;
    }

    $scope.getMultiply = function (a, b) {
        return a * b;
    }

    $scope.getDiff = function (a, b) {
        return a - b;
    }

    function updatePortfolio() {
        portfolioService.searchStockList(function (response) {
            $scope.player = response.ResponseMessage;
            $scope.portfolio = $scope.player.Stocks;
            // bind market price of each stock
            angular.forEach($scope.portfolio, function (value, key) {
                var marketStock = $filter('getByName')($scope.marketData, value.Name);
                if (marketStock) {
                    value.MarketPrice = marketStock.Price;
                    value.Dividend = marketStock.Dividend;
                }
            });
        }, function (response) {
            console.log(response);
        });
    }

}]);