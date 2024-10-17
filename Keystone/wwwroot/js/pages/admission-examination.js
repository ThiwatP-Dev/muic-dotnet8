let examTableId = '#js-exam-schedule';
let facultyTableId = '#js-faculty-department';
let tableRowFaculty = '.js-cascade-faculty';
let tableRowDepartments = '.js-row-departments';
let academicLevelSelect = '.js-cascade-academic-level';

function cascadeFacultyDepartment(academicLevelSelect, facultySelect, departmentSelect) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDepartmentsByAcademicLevelIdAndFacultyId,
            data: {
                academicLevelId: $(academicLevelSelect).val(),
                facultyId: $(facultySelect).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(departmentSelect).empty();

        response.forEach((item) => {
            $(departmentSelect).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(departmentSelect).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
};

$(academicLevelSelect).on('change', function() {
    $(facultyTableId).find(tableRowFaculty).each( function() {
        let currentFaculty = $(this);
        let relatedDepartmentInput = currentFaculty.parents('tr').find(tableRowDepartments);
        cascadeFacultyDepartment(academicLevelSelect, currentFaculty, relatedDepartmentInput);
    })
})

$(facultyTableId).on('change', tableRowFaculty, function() {
    let currentFaculty = $(this);
    let relatedDepartmentInput = currentFaculty.parents('tr').find(tableRowDepartments);
    cascadeFacultyDepartment(academicLevelSelect, currentFaculty, relatedDepartmentInput);
});

$(document).ready( function() {
    var examTable = new RowAddAble({
        TargetTable: examTableId,
        TableTitle: 'Examination Schedule',
        ButtonTitle: 'Examination Type'
    })

    var faculyTable = new RowAddAble({
        TargetTable: facultyTableId,
        TableTitle: 'Choose Faculty & Department',
        ButtonTitle: 'Faculty'
    })

    examTable.RenderButton();
    faculyTable.RenderButton();
    InputMask.renderTimeMask();

    $("#admission-examination-modal").on('shown.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        let id = button.data('value');
        
        var ajax = new AJAX_Helper(
            {
                url: `${ AdmissionExaminationDetailsUrl }/?id=${ id }`,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );

        ajax.GET().done(function (response) {
            $('#modalWrapper-admission-examination-modal').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    });
});