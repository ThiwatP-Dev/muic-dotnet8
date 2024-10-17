$(document).ready(function() {
    $('#change-course-log-modal').on('shown.bs.modal', function(e) {
        let registrationCourseId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: ModifyRegistrationCourseLogsUrl,
                data: {
                    id: registrationCourseId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-change-course-log-details').empty().append(response);
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})