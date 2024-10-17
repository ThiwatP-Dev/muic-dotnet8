$(function () {
    $('#course-tuition-fee-modal-create').on('shown.bs.modal', function (event) {
        $('.chosen-select').chosen();
    })

    $('#course-tuition-fee-modal-edit').on('shown.bs.modal', function (event) {
        var button = event.relatedTarget;
        var academicLevelId = $(button).data('academic-level-id');
        var courseId = $(button).data('course-id');
        var tuitionFeeId = $(button).data('course-tuition-fee-id');

        if (tuitionFeeId != 0) {
            var ajax = new AJAX_Helper(
                {
                    url: CourseTuitionFeeUrl,
                    data: {
                        academicLevelId: academicLevelId,
                        courseId: courseId,
                        tuitionFeeId: tuitionFeeId,
                    },
                    dataType: 'html',
                    contentType: "application/json; charset=utf-8"
                }
            );

            ajax.GET().done(function (response) {
                $('#modalWrapper-course-tuition-fee-edit').empty().append(response);
                $('.chosen-select').chosen();
            })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    ErrorCallback(jqXHR, textStatus, errorThrown);
                });
        }
    })
    
})