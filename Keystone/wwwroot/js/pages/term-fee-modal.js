$(document).ready(function() {

    $('#term-fee-details-modal').on('shown.bs.modal', function(e) {
        let termFeeInfoId = $(e.relatedTarget).data('value');
        
        var ajax = new AJAX_Helper(
            {
                url: TermFeeDetailsModal,
                data: {
                    id: termFeeInfoId,
                },
                dataType: 'html'
            }
        );

        ajax.GET().done(function (response) {
            $('#modalWrapper-term-fee-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
});