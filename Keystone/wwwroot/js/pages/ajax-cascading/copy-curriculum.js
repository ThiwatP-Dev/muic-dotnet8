var academicLevelCopyCurriculum = '.js-cascade-academic-level-copy-curriculum';
var termCopyCurriculum = '.js-cascade-term-copy-curriculum';
var facultyCopyCurriculum = '.js-cascade-faculty-copy-curriculum';
var departmentCopyCurriculum = '.js-cascade-department-copy-curriculum';

function cascadeTermsCopyCurriculum(academicLevelId) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermsByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(termCopyCurriculum).append(getDefaultSelectOption($(termCopyCurriculum)));

        response.forEach((item) => {
            $(termCopyCurriculum).append(getSelectOptions(item.id, item.termText));  
        });

        $(termCopyCurriculum).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeFacultiesCopyCurriculum(academicLevelId) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetFacultiesByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(facultyCopyCurriculum).append(getDefaultSelectOption($(facultyCopyCurriculum)));

        response.forEach((item) => {
            $(facultyCopyCurriculum).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(facultyCopyCurriculum).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeDepartmentsByAcademicLevelAndFacultyCopyCurriculum(academicLevelId, facultyId) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDepartmentsByAcademicLevelIdAndFacultyId,
            data: {
                academicLevelId: academicLevelId,
                facultyId: facultyId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(departmentCopyCurriculum).append(getDefaultSelectOption($(departmentCopyCurriculum)));

        response.forEach((item) => {
            $(departmentCopyCurriculum).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(departmentCopyCurriculum).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', facultyCopyCurriculum, function() {
    if ($(academicLevelCopyCurriculum).length > 0 && $(departmentCopyCurriculum).length > 0){
        let academicLevelId = $(academicLevelCopyCurriculum).val();
        let facultyId = $(this).val();
        cascadeDepartmentsByAcademicLevelAndFacultyCopyCurriculum(academicLevelId, facultyId);
    }
});

$(document).on('change', academicLevelCopyCurriculum, function() {
    let academicLevelId = $(this).val();

    if ($(facultyCopyCurriculum).length > 0) {
        cascadeFacultiesCopyCurriculum(academicLevelId);
    }

    if ($(termCopyCurriculum).length > 0) {
        cascadeTermsCopyCurriculum(academicLevelId);
    }
});