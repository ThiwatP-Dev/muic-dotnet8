$(document).on('change', '.js-cascade-match-course', function() {
    var termId = $('.js-cascade-match-term').val();
    var section = $(this).closest('tr').find('.js-cascade-match-section');

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: $(this).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(section).empty();
        section.append(getDefaultSelectOption(section));

        response.forEach((item) => {
            $(section).append(getSelectOptions(item.value, item.text));
        });

        $(section).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$('#js-submit-match-course').on('click', '.js-submit-match-course', function(e) {
    $('#preloader').fadeIn();
    e.preventDefault();
    
    let targetAction = $(this).data('target');
    $(e.delegateTarget).attr('action', `/MatchCourse/${ targetAction }`).trigger("submit");

    $('#preloader').fadeOut();
})

$( function() {
    $("#receipt-preview-modal").one('shown.bs.modal', function () {
        let studentId = $(`[name="StudentId"]`).val();
        let currentTermId = $(`[name="CurrentTermId"]`).val();
        let termFeeRows = $('#js-term-fee').find('tbody tr');
        var termFee = [];
        let courseRows = $('#js-match-course').find('tbody tr');
        var courses = [];

        termFeeRows.each(function (index) {
            let feeItemId = $(`[name="MatchCourseTermFees[${index}].FeeItemId"]`).val();
            let amount = $(`[name="MatchCourseTermFees[${index}].Amount"]`).val();

            var termFeeRow = {
                FeeItemId: feeItemId,
                Amount: amount
            }

            termFee[index] = termFeeRow;
        })

        courseRows.each(function (index) {
            let registrationCourseId = $(`[name="RegistrationMatchCourses[${index}].RegistrationCourseId"]`).val();
            let courseId = $(`[name="RegistrationMatchCourses[${index}].CourseId"]`).val();
            let sectionId = $(`[name="RegistrationMatchCourses[${index}].SectionId"]`).val();
            let isPaid = $(`[name="RegistrationMatchCourses[${index}].IsPaid"]`).val();

            var addingCourse = {
                RegistrationCourseId: registrationCourseId,
                CourseId: parseInt(courseId),
                IsPaid: isPaid,
                SectionId: sectionId
            }
            courses[index] = addingCourse;
        })

        let MatchCourse = {
            StudentId: studentId,
            CurrentTermId: currentTermId,
            MatchCourseTermFees: termFee,
            RegistrationMatchCourses: courses
        }

        var ajax = new AJAX_Helper(
            {
                url: RenderMatchCourseInvoice,
                data: JSON.stringify(MatchCourse),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );

        ajax.POST().done(function (response) {
            $('#modalWrapper-receipt-log').empty().append(response);

            RenderToggle.toggleHiddenButton('.hidden-row-toggle');
            $('.chosen-select').chosen();
            $('.tools').hide();
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})