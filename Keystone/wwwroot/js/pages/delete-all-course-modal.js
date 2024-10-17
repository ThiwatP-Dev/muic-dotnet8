
$(document).ready(function() {
    $('#registration-course-details-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let studentId = button.data('student-id')
        let termId = button.data('term-id')
        var returnUrl = button.data('return-url');
        console.log(studentId,termId);
    
        var ajax = new AJAX_Helper(
            {
                url: StudentRegistrationCourseByTerm,
                data: {
                    id: studentId,
                    termId: termId,
                    returnUrl: returnUrl
                },
                dataType: 'html'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-registration-course-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})