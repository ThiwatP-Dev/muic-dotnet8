$(document).ready(function() {
    $('#details-agency-modal').on('shown.bs.modal', function(e) {
        let agencyId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: AgencyDetailUrl,
                data: {
                    id: agencyId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-agency-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})