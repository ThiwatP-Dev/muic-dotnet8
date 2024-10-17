var titleEn = '.js-cascade-title-en';
var titleTh = '.js-cascade-title-th';

$(document).ready(function() {
    $('#change-student-status').on('shown.bs.modal', function(e) {
        let studentId = $(e.relatedTarget).data('value')
        let studentStatus = $(e.relatedTarget).data('status')

        var ajax = new AJAX_Helper(
            {
                url: ChangeStudentStatusUrl,
                data: {
                    id: studentId,
                    studentStatus: studentStatus
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-student-status').empty().append(response);
            $('.chosen-select').chosen();
            DateTimeInput.renderSingleDate($('.js-single-date'));
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})

function cascadeTitleThByTitleEn(titleId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTitleThByTitleEn,
            data: {
                id: titleId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
            target.find('option[selected]').prop('selected', false);
            target.find('option:last-child').prop('selected', true);
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', titleEn, function() {
    let currentSection = $(this).closest('section');
    let titleId = $(this).val();

    //require check fields & target
    let secTitleTh = currentSection.find(titleTh);

    if (secTitleTh.length > 0) {
        cascadeTitleThByTitleEn(titleId, secTitleTh);
    }
});
