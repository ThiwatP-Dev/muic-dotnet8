$(function () {
    $('#ability-course-modal-create').on('shown.bs.modal', function (event) {
        $('.chosen-select').chosen();
    })

    $('#ability-course-modal-edit').on('shown.bs.modal', function (event) {
        var button = event.relatedTarget;
        var curriculumCourseId = $(button).data('curriculum-course-id');
        var sequence = $(button).data('sequence');
        var specializationGroupId = $(button).data('specialization-group-id');

        if (curriculumCourseId != 0) {
            var ajax = new AJAX_Helper(
                {
                    url: GetAbilityCourseUrl,
                    data: {
                        curriculumCourseId: curriculumCourseId,
                        sequence: sequence,
                        specializationGroupId: specializationGroupId
                    },
                    dataType: 'html',
                    contentType: "application/json; charset=utf-8"
                }
            );

            ajax.GET().done(function (response) {
                $('#modalWrapper-ability-course-edit').empty().append(response);
                $('.chosen-select').chosen();
            })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    ErrorCallback(jqXHR, textStatus, errorThrown);
                });
        }
    })
})