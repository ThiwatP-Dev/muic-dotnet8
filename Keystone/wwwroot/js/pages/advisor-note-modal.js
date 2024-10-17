$(document).ready( function() {
    var courseTable = new RowAddAble({
        TargetTable: "#js-advisor-course",
        ButtonTitle: 'Course'
    });
    courseTable.RenderButton();
});

$(document).on('change', '.js-cascade-advisor-course', function() {
    let currentRow = $(this).parents('tr');
    let sectionSection = currentRow.find('.js-cascade-advisor-section');
    let sectionTermId = currentRow.find('.js-cascade-advisor-term').val();

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: sectionTermId,
                courseId: $(this).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        sectionSection.append(getDefaultSelectOption(sectionSection));

        response.forEach((item) => {
            $(sectionSection).append(getSelectOptions(item.value, item.text));
        });

        $(sectionSection).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})