$(document).ready(function() {
    ExcelTable.print('#js-confirm-refund');
    CheckList.renderCheckbox('#js-confirm-refund');

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
    });
});