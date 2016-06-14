app.controller('PlayerCtrl', ['$scope', '$filter', '$mdDialog', '$mdMedia', 'stockSearch', 'portfolioService', function ($scope, $filter, $mdDialog, $mdMedia, stockSearch, portfolioService) {
    
    $scope.tradeCmds = [{ 'id': 0, 'name': 'Buy' }, { 'id': 1, 'name': 'Sell' }];
    $scope.stocksTrade = new Array();
    $scope.tradeCmdSelected = 'BUY';
    $scope.tradeVol = 0;
    $scope.tradePrice = 0.00;
    $scope.commissionRate = 0.00;
    $scope.windowWidth = window.innerWidth;

    $scope.init = function () {
        // Update market data
        portfolioService.searchMarketList(function (response) {
            $scope.marketData = response.ResponseMessage;
            // Setup stocks name for trade
            angular.forEach($scope.marketData, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
            // Remove stock index from trade when not index fund manager
                //$scope.stocksTrade.splice($scope.stocksTrade.length - 1, 1);
            // Set first stock trade
            $scope.stockTradeSelected = $scope.marketData[0].Name;
            // Set first stock trade price
            $scope.tradePrice = $scope.marketData[0].Price;
        }, function (response) {
            console.log(response);
        }).then(function () {
            // Get player portfolio
            updatePortfolio();
        });
        // Get current commission rate
        portfolioService.searchCommissionRate(function (response) {
            $scope.commissionRate = response.ResponseMessage;
        }, function (response) {
            showErrorDialog(ev, response);
        });
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

        if ($scope.tradeCmdSelected === 'BUY') {
            // Trade stock in market
            angular.forEach($scope.marketData, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
        } else {
            // Trade stock in portfolio
            angular.forEach($scope.portfolio, function (value, key) {
                $scope.stocksTrade.push({ 'id': value.ID, 'name': value.Name });
            });
        }
        $scope.stockTradeSelected = $scope.stocksTrade[0].name;
    }

    // Setup stocks price for trade
    $scope.changeTradePrice = function () {
        // Find stock price in market
        var stock = $filter('getByName')($scope.marketData, $scope.stockTradeSelected);
        $scope.tradePrice = stock.Price;
    }

    // Buy/Sell selected stock
    $scope.stockTrade = function (ev) {
        // If not type in volume
        if ($scope.tradeVol < 100) {
            showErrorDialog(ev, 'Please insert correct volumn.');
            return;
        }

        // Must trade upto a hundrad
        $scope.tradeVol = Math.floor($scope.tradeVol / 100) * 100;
        // Total cost
        var total = $scope.tradeVol * $scope.tradePrice;
        // Setup confirmation data
        $scope.confirmStock = {
            stockTradeSelected: $scope.stockTradeSelected,
            tradeVol: $scope.tradeVol,
            tradePrice: $scope.tradePrice,
            total: total,
            commissionRate: $scope.commissionRate * total
        };
        //console.log($scope.tradeCmdSelected);
        //return;
        $mdDialog.show({
            controller: DialogController,
            locals: { stock: $scope.confirmStock },
            templateUrl: 'tradeDialog.tpl.cshtml',
            parent: angular.element(document.body),
            targetEvent: ev,
            clickOutsideToClose: true,
            fullscreen: false
        })
        .then(function () {
            // Press confirm 
            portfolioService.stockTrade($scope.tradeCmdSelected, 0, $scope.stockTradeSelected, $scope.tradeVol, function (response) {
                if (response.IsSuccess) {
                    updatePortfolio();
                } else {
                    //console.log(response.ResponseMessage);
                    showErrorDialog(ev, response.ResponseMessage);
                }
            }, function (response) { // Error from process
                // If error is redirect
                if (response.data.ResponseMessage.uri) {
                    $(location).attr('href', response.uri);
                }
                console.log(response);
            });

            $scope.tradeVol = 0;
        }, function () {
            // Press cancel at confirm dialog
            //$scope.status = 'You decided to keep your debt.';
        });
    }

    // Selected stock to buy when click at stock in market
    $scope.selectedBuyStock = function (index) {
        // Switch to BUY tab
        $scope.onClickTab($scope.tabs[0]);
        $scope.stockTradeSelected = $scope.marketData[index].Name;
    }

    // Selected stock to sell when click at stock in portfolio
    $scope.selectedSellStock = function (index) {
        // Switch to SELL tab
        $scope.onClickTab($scope.tabs[1]);
        $scope.stockTradeSelected = $scope.portfolio[index].Name;
    }

    // Expand/Collapse stock detail
    $scope.showStockDetail = function (ev, index) {
        var stock = ev.currentTarget;
        var detail = $(stock).next();
        detail.slideToggle(500, function () { });

        $scope.selectedSellStock(index);
    }

    //------------//
    // Trade tabs //
    //------------//
    $scope.tabs = [{
        title: 'BUY'
    }, {
        title: 'SELL'
    }];

    $scope.onClickTab = function (tab) {
        $scope.tradeCmdSelected = tab.title;
        $scope.changeTradeCmd();
    }
    
    $scope.isActiveTab = function (tabTitle) {
        return tabTitle === $scope.tradeCmdSelected;
    }
     
    //------------//
    // MathHelper //
    //------------//
    $scope.getMultiply = function (a, b) {
        return a * b;
    }

    $scope.getDiff = function (a, b) {
        return a - b;
    }

    $scope.megaNumber = function (num) {
        if (!num) return 0.00;
        return $filter('megaNumber')(num);
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

    function showErrorDialog(ev, msg) {
        $mdDialog.show(
            $mdDialog.alert()
            .parent(angular.element(document.querySelector('#popupContainer')))
            .clickOutsideToClose(true)
            .title('Error')
            .textContent(msg)
            .ariaLabel('Error Dialog')
            .ok('Got it!')
            .targetEvent(ev)
        );
    }

    function DialogController($scope, $mdDialog, stock) {
        $scope.stock = stock;

        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.confirm = function () {
            $mdDialog.hide();
        };
        $scope.answer = function (answer) {
            $mdDialog.hide(answer);
        };
    }
}]);