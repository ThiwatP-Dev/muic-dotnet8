var selectedPlanId = 0;
var selectedPlanTitle = "";
var selectedPlanScheduleId = 0;
var selectedScheduleSections;
var selectedScheduleMinimumSeat = 0;
var students = [];
var selectedStudentIds = [];
var coursesWithGrade = [];
var courseGradeModal = '#courses-grade-modal';
var selectedTableBody = '#js-courses-grade';
var courseInput = '#js-filter-course';
var gradeInput = '#js-filter-grade';
var defaultRow = '.default-row';
var groupRegistrationFormId = "#group-registration-wizard";

let notReleasedText = "Not Released";

var GroupRegistration = (function() {
    function InitStep() {

        var groupRegistForm = $(groupRegistrationFormId);

        groupRegistForm.steps({
            headerTag: "h3",
            bodyTag: "section",
            transitionEffect: "slideLeft",
            onStepChanging: function (event, currentIndex, newIndex)
            {
                groupRegistForm.validate().settings.ignore = ":disabled,:hidden";

                $("#js-select-faculty").chosen().trigger("chosen:updated");
                $("#js-select-department").chosen().trigger("chosen:updated");

                return groupRegistForm.valid();
            },
            onStepChanged: function (event, currentIndex) {
                if (currentIndex == 2) {
                    renderScheduleSelection();
                }
            },
            onFinishing: function (event, currentIndex)
            {
                groupRegistForm.validate().settings.ignore = ":disabled";
                SubmitRegistration(ajaxCallBack, groupRegistForm);
                return groupRegistForm.valid();
            }
        });
    }

function renderScheduleSelection() {
    var ajax = new AJAX_Helper({
        url: GroupRegistrationURLDict.SelectPlan,
        data: {
            SelectedPlanId: selectedPlanId,
            SelectedStudentIds: selectedStudentIds
        },
        dataType: 'html'
    });
    
    ajax.POST().done(function(response) {
        var selectableSchedules = $('.js-select-schedule');
        selectableSchedules.empty().append(response);

        $('#js-group-registration-select-schedule-plan-title').html(`Plan: ${ selectedPlanTitle }`);
    })
    .fail(function(error) {
        console.log(error);
    });
}

function ajaxCallBack(response) {
    let responseType = typeof response

    if (responseType == 'string') {
        window.location.href = response
    }

    return response;
}

function InitMultiSelection() {
    MultiSelect.renderMultiSelectWithSearch('.js-multi-selectlist', afterSelectCallback, afterDeselectCallback)

    $('.ms-list li span').each(function() {
        let listText = $(this).html()
        let code = listText.substr(0,listText.indexOf(' ')); 
        let name = listText.substr(listText.indexOf(' ') + 1);
        $(this).html(`<b>${ code }</b> ${ name }`)
    })
}

var afterSelectCallback = function(values) {
    values.forEach(function(value) {
        selectedStudentIds.push(value);
    });
};

var afterDeselectCallback = function(values) {
    values.forEach(function(value) {
        var index = selectedStudentIds.findIndex(x => x === value);
        selectedStudentIds.splice(index, 1);
    });
};

function SubmitRegistration(callback, formData) {
    var currentFormData = formData.serialize();

    var ajax = new AJAX_Helper({
        url: GroupRegistrationURLDict.SubmitRegistration,
        data : currentFormData,
        dataType: 'html'
    });

    ajax.POST().done(function(response) {
        callback(response);
    })
    .fail(function(jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

return {
    initStep : InitStep,
    initMultiSelection : InitMultiSelection
}
})();

function removeSelected(selectedData) {
    let currentDataId = $(selectedData).data('id');

    let objIndex = coursesWithGrade.findIndex(x => x.CourseId == currentDataId);
    coursesWithGrade.splice(objIndex, 1);

    let currentRow = $(selectedData).parents('tr')
    $(currentRow).remove();
}

function addCourseAndGradeRow(currentCourse, currentGrade, currentDataCourseId) {
    return `<tr>
                <td>${ currentCourse[0].toUpperCase() }</td>
                <td>${ currentGrade }</td>
                <td class="td-actions">
                    <a class="js-delete-selected" href="#!" data-id="${ currentDataCourseId }">
                        <i class="la la-trash delete"></i>
                    </a>
                </td>
            </tr>`
}

$(document).on('keyup', '.js-suggestion', function(e) {
    let suggestBox = $(this).parent().find('.js-suggestion-list');
    let selectedItem = $(suggestBox).find('.active');
    if(e.keyCode === 13) {
        let courseId = $(selectedItem).data('course-id');

        if (courseId > 0) {
            $(this).attr('course-id', courseId);
        }
    }
}); 

$(document).on('click', '.js-suggestion-item', function() {
    let relateInput = $(this).parent().prev();
    let courseId = $(this).data('course-id');

    if (courseId > 0) {
        relateInput.attr('course-id', courseId);
    }
});

$('body').on('change', '.js-group-registration-plan-selection', function() {
    selectedPlanId = $(this).val();
    selectedPlanTitle = $(this).data('plan-title');
});

$('body').on('change', '.js-group-registration-schedule-selection', function() {
    selectedPlanScheduleId = $(this).val();``
    var selectedScheduleTarget = $(this).data('target');
    var selectedScheduleTitle = $(this).data('title');
    var selectedSchedule = $(selectedScheduleTarget);
    var selectedScheduleDetailTarget = $(this).data('schedule-detail-target');
    selectedScheduleSections = selectedSchedule.data('schedule-sections');
    selectedScheduleMinimumSeat = selectedSchedule.data('schedule-minimum-seat');

    var scheduleDetailElement = $(selectedScheduleDetailTarget).html();

    $('#js-group-registration-summary-plan-title').html(`Plan: ${ selectedPlanTitle }`);
    $('#js-group-registration-summary-schedule-title').html(selectedScheduleTitle);

    $('#js-group-registration-summary-schedule').html(selectedSchedule.html());
    $('.js-group-registration-summary-schedule-detail').html(scheduleDetailElement);
});

$('body').on('click', '#js-search-student', function() {
    let academicLevelId = $('#js-select-academic-level-id').val();
    let batchCodeStart = $('#js-input-batch-code-start').val();
    let batchCodeEnd = $('#js-input-batch-code-end').val();
    let faculty = $('#js-select-faculty').val();
    let department = $('#js-select-department').val();
    let admissionDateStart = $('#js-input-admission-date-start').val();
    let admissionDateEnd = $('#js-input-admission-date-end').val();
    let gradeReleaseDateStart = $('#js-input-grade-release-date-start').val();
    let gradeReleaseDateEnd = $('#js-input-grade-release-date-end').val();
    
    var ajax = new AJAX_Helper(
        {
            url: GroupRegistrationURLDict.SearchStudents,
            data : {
                AcademicLevelId: academicLevelId,
                BatchCodeStart: batchCodeStart,
                BatchCodeEnd: batchCodeEnd,
                FacultyId: faculty,
                DepartmentId: department,
                AdmissionStartedAt: admissionDateStart,
                AdmissionEndedAt: admissionDateEnd,
                GradePublishStartedAt: gradeReleaseDateStart,
                GradePublishEndedAt: gradeReleaseDateEnd,
                SelectedCourseGrades: coursesWithGrade,
                SelectedScheduleMinimumSeat: selectedScheduleMinimumSeat
            },
            dataType: 'html'
        }
    );

    ajax.POST().done(function(response) {
        $('#SelectStudents').empty().append(response);
        students = [];
        selectedStudentIds = [];
        $('#js-multi-select-callback option').each(function(index, value) {
            var option = $(value);
            students.push({
                id: option.val(),
                name: option.text()
            });
        });
        
        GroupRegistration.initMultiSelection();
    })
    .fail(function(error) {
        console.log(error);
    });
});

$(document).on('click', '#js-select-schedule-student', function() {
    let studentIds = $(this).data('student-ids');
    var ajax = new AJAX_Helper({
        url: '/GroupRegistration/ListScheduleStudents',
        data: {
            RegistrableScheduleStudentIds: studentIds
        },
        dataType: 'html'
    });

    ajax.POST().done(function(response) {
        $('.js-select-schedule-student-list').empty().append(response);
    })
    .fail(function(error) {
        console.log(error);
    });
});

GroupRegistration.initStep();

$(document).ready(function() {

    $('#js-select-faculty').chosen();
    $('#js-select-department').chosen();

    $(courseGradeModal).one('shown.bs.modal', function() {
        $(gradeInput).chosen({
            disable_search: "true",
        });

        $('#js-add-filter').click(function() {
            let currentDataCourseId = $(courseInput).attr('course-id')
            let currentCourse = $(courseInput).val().split(' ');
            let currentGrade = $(gradeInput).val();

            if ($(defaultRow).length) {
                $(defaultRow).remove()
            }

            if (coursesWithGrade.length) {
                let alreadySelectedCourses = coursesWithGrade.find( x => x.CourseId === currentDataCourseId )
                if (alreadySelectedCourses) {
                    Alert.renderAlert("Duplicated Course", "You select a course which already added", "error")
                    return;
                }
            }

            if (currentCourse != "") {
                if (currentGrade == notReleasedText) {
                    currentGrade = "";
                }

                let obj = {
                    CourseId : currentDataCourseId,
                    Grade : currentGrade
                }
        
                $(selectedTableBody).append(addCourseAndGradeRow(currentCourse, currentGrade, currentDataCourseId))
                coursesWithGrade.push(obj);
            }

            RenderTableStyle.columnAlign();
            $(courseInput).val('');
            $(gradeInput).val('Not Released').trigger('chosen:updated');
        })
        
        $(selectedTableBody).on('click', '.js-delete-selected', function() {
            removeSelected(this);
        })

        $('#js-confirm-add').click(function() {
            $(courseGradeModal).modal('hide');

            var selectedCourse = "";
            let selectedCourseList = $(selectedTableBody).find('td:not(.td-actions)');

            $(selectedCourseList).each(function(index) {
                
                selectedCourse += $(this).html();
                if (index % 2 == 0) {
                    selectedCourse += ": "
                }
                else if ((index + 1) < selectedCourseList.length) {
                    selectedCourse += ", "
                }
            })

            $('#js-filter-list').html(selectedCourse);
        })
    })
})