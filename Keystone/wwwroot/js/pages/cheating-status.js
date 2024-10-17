$(document).ready(function() {
    $('#details-cheating-status-modal').on('shown.bs.modal', function(e) {
        let cheatingStatusId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.CheatingStatusDetailsUrl,
                data: {
                    id: cheatingStatusId,
                },
                dataType: 'json'
            }
        );

        ajax.POST().done(function (response) { 
            $('#modalWrapper-cheating-status-details').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#edit-cheating-status-modal').on('shown.bs.modal', function(e) {
        let cheatingStatusId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.CheatingStatusEditUrl,
                data: {
                    id: cheatingStatusId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-cheating-status-edit').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})