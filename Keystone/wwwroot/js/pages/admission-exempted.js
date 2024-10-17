let exemptedTableId = '#js-course-exemption';
let facultyTableId = '#js-faculty-department';
let academicLevelExempted = '.js-cascade-academic-level';

$(document).ready( function() {
    var exemptedTable = new RowAddAble({
        TargetTable: exemptedTableId,
        TableTitle: 'Preferred Course',
        ButtonTitle: 'Score'
    })

    var faculyTable = new RowAddAble({
        TargetTable: facultyTableId,
        TableTitle: 'Affected Faculty & Department',
        ButtonTitle: 'Faculty'
    })

    exemptedTable.RenderButton();
    faculyTable.RenderButton();
})

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

$(facultyTableId).on('change', '.js-cascade-faculty', function() {
    let currentFaculty = $(this);
    let relatedDepartmentInput = currentFaculty.parents('tr').find('.js-cascade-department');
    cascadeFacultyDepartment(academicLevelExempted, currentFaculty, relatedDepartmentInput);
});