var Alert = (function() {

    var RenderAlert = function(title, text, type) {
        swal({
            title: title,
            text: text,
            icon: type, // "warning", "error", "success" or "info".
            button: "Ok",
        });
    }

    return {
        renderAlert : RenderAlert
    };
})();