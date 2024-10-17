$(document).ready(function() {
    $('#course-fee-details-modal').on('shown.bs.modal', function(e) {
        let courseFeeId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: CourseFeeDetailsUrl,
                data: {
                    id: courseFeeId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-course-fee-details').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})