
$(document).ready( function() {
    CheckList.renderCheckbox('#js-questionnaire-approval');
    $(".js-render-nicescroll").niceScroll();
    var table = $('#js-questionnaire-approval').DataTable({
        paging: false,
        dom: 'Bfrtip',
        buttons: {
            buttons: [{
                extend: 'excel',
                text: 'Excel',
                title: $('h1').text(),
                exportOptions: {
                    columns: ':not(.no-print)'
                },
                footer: true
            }],
            dom: {
                container: {
                    className: 'dt-buttons'
                },
                button: {
                    className: 'btn btn--primary'
                }
            }
        },
        language: {
            emptyTable: '<i class="text-danger">No Data</i>'
        },
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, -1]
        }],
        "order": [[ 1, 'asc' ]]
    });
    $('#questionnaire-approval-log').on('shown.bs.modal', function(e) {
        let questionnaireApprovalId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: QuestionnaireApprovalLogUrl,
                data: {
                    id: questionnaireApprovalId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-questionnaire-approval-log').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    setSaveButton();

    $('.form-check-input, .js-check-all').on('change', function() {
        setSaveButton();
    })

    
})

$('#form-search-questionnaire-approval').on('click','.js-search-btn' , function() {
    $('#preloader').fadeIn();
    $('#form-search-questionnaire-approval').submit()
})

function setSaveButton() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-questionnaire-approval').find('.form-check-input:checked').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}

function cascadeFilterCourseGroupsByFacultyId(facultyId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetFilterCourseGroupsByFacultyId,
            data: {
                facultyId: facultyId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', '.js-cascade-faculty', function() {
    let currentSection = $(this).closest('section');
    let facultyId = $(this).val();
    let sectionfilterCourseGroup = currentSection.find('.js-cascade-filter-course-group');

    if (sectionfilterCourseGroup.length > 0) {
        cascadeFilterCourseGroupsByFacultyId(facultyId, sectionfilterCourseGroup);
    }
});