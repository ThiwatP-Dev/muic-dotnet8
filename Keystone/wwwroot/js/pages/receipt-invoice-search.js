$(document).ready(function() {
    $('#js-receipt-details-modal').on('shown.bs.modal', function(event) {
        var button = event.relatedTarget;
        var buttonId = $(button).data('receipt-id');
        var eventType = $(button).data('type')

        if (eventType === "receipt-details") {

            var ajax = new AJAX_Helper(
                {
                    url: ReceiptDetailsUrl,
                    data: {
                        id: buttonId,
                    },
                    dataType: 'html',
                    contentType : "application/json; charset=utf-8"
                }
            );
        
            ajax.GET().done(function (response) { 
                $('#js-receipt-details-modal .modal-content').html(response);
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        } else if (eventType === "transaction") {

            var ajax = new AJAX_Helper(
                {
                    url: ReceiptTransectionUrl,
                    data: {
                        id: buttonId,
                    },
                    dataType: 'html',
                    contentType : "application/json; charset=utf-8"
                }
            );
        
            ajax.GET().done(function (response) { 
                $('#js-receipt-details-modal .modal-content').html(response);
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        } else {
            return;
        }
    })

    $('#js-invoice-details-modal').on('shown.bs.modal', function(e) {
        let invoiceId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: InvoiceDetailUrl,
                data: {
                    id: invoiceId,
                },
                dataType: 'html',
                contentType : "application/json; charset=utf-8"
            }
        );

        ajax.GET().done(function (response) { 
            $('#js-invoice-details-modal .modal-content').html(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})