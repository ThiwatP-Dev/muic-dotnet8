$(document).ready(function() {
    $('#payment-log-modal').on('shown.bs.modal', function(e) {
        let id = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: PaymentLogDetailsUrl,
                data: {
                    id: id,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-payment-log').empty().append(response);
            
            var data = $('#display-json').data('model');
            if (data != undefined) {
                document.getElementById("display-json").innerHTML = JSON.stringify(data, undefined, 4);
                console.log(JSON.stringify(data, undefined, 4));
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})