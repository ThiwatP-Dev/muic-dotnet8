$(document).ready(function() {
    $('#details-previous-school-modal').on('shown.bs.modal', function(e) {
        let preschoolId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: PreviousSchoolDetailsUrl,
                data: {
                    id: preschoolId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-previous-school-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})