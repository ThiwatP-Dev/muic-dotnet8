$(document).ready(function() {
    $('#fee-details-modal').on('shown.bs.modal', function(e) {
        let feeTemplateId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: FeeTemplateDetailsUrl,
                data: {
                    id: feeTemplateId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-fee-details').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})