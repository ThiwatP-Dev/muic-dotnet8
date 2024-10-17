var exceptionAcademicLevelSelect = '#js-exception-academic-level-select';
var exceptionCourseSelect = '#js-exception-course-select';
var exceptionFacultySelect = '#js-exception-faculty-select';
var exceptionDepartmentSelect = '#js-exception-department-select';

var courseContent = '#js-exception-course-content';
var facultyContent = '#js-exception-faculty-content';
var courseTabIndex = '#js-tab-index-course';
var facultyTabIndex = '#js-tab-index-faculty';

var responseStatusCourse = "#js-status-course";
var responseStatusFaculty = "#js-status-faculty";

$(document).on('click', '.js-add-exception', function() {
    $('#preloader').fadeIn();
    
    var tabIndex = $(this).data('tab-index');
    if (tabIndex == '0') {
        AddExceptionByCourse();
    } else {
        AddExceptionByFaculty();
    }

    $('#preloader').fadeOut();
})

function AddExceptionByCourse() {
    let model = {
        CourseId: $(exceptionCourseSelect).val(),
    }

    var ajax = new AJAX_Helper(
        {
            url: WithdrawalExceptionCreateByCourseUrl,
            data: JSON.stringify(model),
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
        }
    );

    ajax.POST().done(function (response) {
        $(courseContent).empty().append(response);
        let tabIndex = $(courseTabIndex).val();

        let status = $(responseStatusCourse).html();
        if (status === 'SaveSuccess') {
            FlashMessage.tab("Confirmation", ErrorMessage.SaveSuccess, tabIndex);
        } else if (status === 'DataDuplicate') {
            let message = `Course ${ ErrorMessage.DataDuplicate }`;
            FlashMessage.tab("Danger", message, tabIndex)
        } else {
            FlashMessage.tab("Danger", ErrorMessage.InvalidInput, tabIndex)
        }
        
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function AddExceptionByFaculty() {
    let model = {
        FacultyId: $(exceptionFacultySelect).val(),
        DepartmentId: $(exceptionDepartmentSelect).val(),
    }

    var ajax = new AJAX_Helper(
        {
            url: WithdrawalExceptionCreateByFacultyUrl,
            data: JSON.stringify(model),
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
        }
    );

    ajax.POST().done(function (response) { 
        $(facultyContent).empty().append(response);
        let tabIndex = $(facultyTabIndex).val();

        let status = $(responseStatusFaculty).html();
        if (status === 'SaveSuccess') {
            FlashMessage.tab("Confirmation", ErrorMessage.SaveSuccess, tabIndex);
        } else if (status === 'DataDuplicate') {
            let message = `Department ${ ErrorMessage.DataDuplicate }`;
            FlashMessage.tab("Danger", message, tabIndex)
        } else {
            FlashMessage.tab("Danger", ErrorMessage.InvalidInput, tabIndex)
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function Delete(id, tabIndex) {
    $('#delete-confirm-modal-tab').modal('hide');
    $('#preloader').fadeIn();

    var ajax = new AJAX_Helper(
        {
            url: WithdrawalExceptionDeleteUrl,
            data: {
                id: id, 
                tabIndex: tabIndex 
            },
            dataType: 'html',
        }
    );

    ajax.POST().done(function (response) { 
        if (tabIndex == '0') {
            $(courseContent).empty().append(response);
        } else {
            $(facultyContent).empty().append(response)
        }
        FlashMessage.tab("Confirmation", ErrorMessage.SaveSuccess, tabIndex);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
        FlashMessage.tab("Danger", ErrorMessage.ServiceUnavailable, tabIndex);
    });

    $('#preloader').fadeOut();
}

$(document).on('click', '#js-add-all-departments', function() {
    let facultyId = $(exceptionFacultySelect).val();
    let departments = $(`${ exceptionDepartmentSelect } option`);
    let departmentIds = [];
    departments.each(function() {
        var departmentId = $(this).val();
        if (departmentId != "") {
            let model = {
                FacultyId: facultyId,
                DepartmentId: parseInt(departmentId),
            }
            departmentIds.push(model);
        }
    });

    var ajax = new AJAX_Helper(
        {
            url: WithdrawalExceptionCreateByDepartmentsUrl,
            data: JSON.stringify(departmentIds),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
        }
    );

    ajax.POST().done(function (response) { 
        $(facultyContent).empty().append(response);
        let tabIndex = $(facultyTabIndex).val();

        let status = $(responseStatusFaculty).html();
        if (status === 'SaveSuccess') {
            FlashMessage.tab("Confirmation", ErrorMessage.SaveSuccess, tabIndex);
        } else {
            FlashMessage.tab("Danger", ErrorMessage.InvalidInput, tabIndex)
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$(document).on('change', exceptionAcademicLevelSelect, function() {
    var academicLevelId = $(this).val();

    if ($(exceptionFacultySelect).length > 0) {
        cascadeExceptionFaculties(academicLevelId);
    }

    if ($(exceptionFacultySelect).length > 0 && $(exceptionDepartmentSelect).length > 0) {
        var facultyId = $(exceptionFacultySelect).val();

        cascadeDepartmentsByAcademicLevelAndFaculty(academicLevelId, facultyId);
    }
});

function cascadeExceptionFaculties(academicLevelId) {
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
        $(exceptionFacultySelect).append(getDefaultSelectOption($(exceptionFacultySelect)));

        response.forEach((item) => {
            $(exceptionFacultySelect).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(exceptionFacultySelect).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', exceptionFacultySelect, function() {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetExceptionWithdrawalDepartments,
            data: {
                facultyId: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(exceptionDepartmentSelect).append(getDefaultSelectOption($(exceptionDepartmentSelect)));

        response.forEach((item) => {
            $(exceptionDepartmentSelect).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(exceptionDepartmentSelect).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});