app.filter('getByName', function () {
    return function (input, name) {
        var i = 0, len = input.length;
        for (; i < len; i++) {
            if (input[i].Name === name) {
                return input[i];
            }
        }
        return null;
    }
});

app.filter('getById', function () {
    return function (input, id) {
        var i = 0, len = input.length;
        for (; i < len; i++) {
            if (input[i].ID == id) {
                return input[i];
            }
        }
        return null;
    }
});