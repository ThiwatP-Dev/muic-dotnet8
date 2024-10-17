var AJAX_Helper = (function(config) {

    var GET = function() {
        return $.ajax({
            url: config.url,
            type: 'GET',
            data: config.data,
            contentType: config.contentType,
            datatype: config.dataType,
        });
    };

    var POST = function() {
        return $.ajax({
            url: config.url,
            type: 'POST',
            data: config.data,
            contentType: config.contentType,
            datatype: config.dataType,
            beforeSend: function(xhr) {
                xhr.setRequestHeader("X-XSRF-Token",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
        });
    };

    return {
        GET: GET,
        POST: POST
    };
});

var SuccessCallback = function(response) {
    Alert.renderAlert("Success", `Response result : ${response}`, "success");
};

var ErrorCallback = function(jqXHR, textStatus, errorThrown) {
    Alert.renderAlert("Fail", textStatus, "error");
};