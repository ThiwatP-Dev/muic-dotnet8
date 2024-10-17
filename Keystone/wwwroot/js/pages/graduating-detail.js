var specializationType = '.js-graduating-special-type';
var specializationGroup = '.js-graduating-special-group';

$(document).ready(function() {
    var inputTable = new RowAddAble({
        TargetTable: '#js-course-prediction-table',
        ButtonTitle: 'Course Prediction',
        TableTitle: 'Course Prediction'
    });
    inputTable.RenderButton();

    $('#change-course-group').on('shown.bs.modal', function(e) {
        let button = $(e.relatedTarget);
        let courseGroupId = $(e.relatedTarget).data('group-id');
        let courseId = $(e.relatedTarget).data('course-id');
        let versionId = $(e.relatedTarget).data('version-id');
        let requestId = $(e.relatedTarget).data('request-id');
        let moveGroupId = $(e.relatedTarget).data('move-id');
        let remark = $(e.relatedTarget).data('remark');
        let moveGroupString = button.parents('tr').find('.js-show-move-group');
        let remarkString = button.parents('tr').find('.js-show-course-group-remark');

        var ajax = new AJAX_Helper(
            {
                url: ChangeCourseGroupUrl,
                data: {
                    courseGroupId: courseGroupId,
                    courseId: courseId,
                    curriculumVersionId: versionId,
                    graduatingRequestId: requestId,
                    moveCourseGroupId: moveGroupId,
                    remark: remark
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-change-course-group').empty().append(response);
            $('.chosen-select').chosen();
            $('.js-submit-move-course-group').on('click', function(e) {
                submitChangeCourseGrouping(courseGroupId, courseId, requestId, moveGroupString, remarkString, button);
            })
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#add-grouping-course').on('shown.bs.modal', function(e) {
        let courseGroupId = $(e.relatedTarget).data('group-id');
        let requestId = $(e.relatedTarget).data('request-id');
        let tableId = $(e.relatedTarget).data('table-id');
        let table = $(e.relatedTarget).parents('.block__body').find(`#${tableId}`);
        var ajax = new AJAX_Helper(
            {
                url: AddGroupingCourseUrl,
                data: {
                    courseGroupId: courseGroupId,
                    graduatingRequestId: requestId
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-add-grouping-course').empty().append(response);
            $('.chosen-select').chosen();
            $('.js-submit-add-grouping-course').on('click', function(e) {
                submitAddCourseGrouping(courseGroupId, requestId, table);
            })
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#change-status-modal').on('shown.bs.modal', function(e) {
        let requestId = $(e.relatedTarget).data('request-id');
        var ajax = new AJAX_Helper(
            {
                url: GraduatingRequestChangeStatus,
                data: {
                    graduatingRequestId: requestId
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-graduating-change-status').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#confirm-delete-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let modificationId = button.data('modification-id');
        let requestId = button.data('request-id');
        var tableId = $(button).parents('table').attr('id');
        var eventRow = $(button).closest('tr');
        var rowIndex = eventRow[0].rowIndex;

        $('.js-confirm-btn').on('click', function(e) {
            submitDeleteCourseGrouping(modificationId, requestId, tableId, rowIndex);
        })

    });
});

$(document).on('change', specializationType, function() {
    let currentSection = $(this).closest('section');
    let target = currentSection.find(specializationGroup);
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSpecializationGroupByType,
            data: {
                type: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });
        
        target.trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$(document).on('click', '.js-graduating-button', function() {
    let currentSection = $(this).closest('section');
    var specializationTypeText = currentSection.find(specializationType).val();
    var specializationGroupId = currentSection.find(specializationGroup).val();
    var studentId = currentSection.find('.js-graduating-special-student').val();
    if (specializationTypeText == null || studentId == '') {
        Alert.renderAlert("Error", "Please select specialization type", "error");
        return;
    }

    var ajax = new AJAX_Helper(
        {
            url: GetSpecializationGroupsUrl,
            data: {
                studentId: studentId,
                type: specializationTypeText,
                specializationGroupId: specializationGroupId
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) { 
        if (response === null || response === "") {
            Alert.renderAlert("Error", "No specialization group matched", "error");
        } 
        
        $('#js-graduating-body').empty().append(response);
        RenderTableStyle.columnAlign();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$(document).on('click', '.js-honor-button', function() {
    let currentSection = $(this).closest('section');
    var studentId = currentSection.find('.js-graduating-student').val();
    var ajax = new AJAX_Helper(
        {
            url: GetAcademicHonorsUrl,
            data: {
                studentId: studentId
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) {         
        $('.js-graduation-honor').empty().append(response);

        RenderTableStyle.columnAlign();
        
        CheckList.renderCheckbox('#js-select-honor');
        $(".js-render-nicescroll").niceScroll();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

function submitChangeCourseGrouping(courseGroupId, courseId, requestId, moveGroupString, remarkString, button) {
    var moveCourseGroupId = $('.js-move-course-group').val();
    var remark = $('.js-change-course-group-remark').val();
    var ajax = new AJAX_Helper(
        {
            url: ChangeCourseGroupSubmit,
            data: {
                courseGroupId: courseGroupId,
                courseId: courseId,
                graduatingRequestId: requestId,
                moveCourseGroupId: moveCourseGroupId,
                remark: remark
            },
            dataType: 'json',
        }
    );

    ajax.GET().done(function (response) {
        $('#change-course-group').modal('hide');
        if (response.status == "duplicate") {
            Alert.renderAlert("Fail", "Course group is duplicate.", "error");
        } else if (response.status != "error") {
            moveGroupString.html(response.moveCourseGroup)
            remarkString.html(response.remark)
            $(button).data('move-id', response.moveCourseGroupId);
            $(button).data('remark', response.remark);
            SuccessCallback("Save Success");
        } else {
            Alert.renderAlert("Fail", "Unable to save", "error");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function submitAddCourseGrouping(courseGroupId, requestId, table) {
    var courseId = $('.js-add-course-id').val();
    var gradeId = $('.js-add-grade-id').val();
    var ajax = new AJAX_Helper(
        {
            url: SubmitAddGroupingCourseUrl,
            data: {
                courseGroupId: courseGroupId,
                courseId: courseId,
                gradeId: gradeId,
                graduatingRequestId: requestId
            },
            dataType: 'json',
        }
    );

    ajax.GET().done(function (response) {
        $('#add-grouping-course').modal('hide');
        if (response != null) {
            if (response == "error") {
                Alert.renderAlert("Fail", "Unable to save", "error");
            } else if (response == "duplicate") {
                Alert.renderAlert("Fail", "Course is duplicate.", "error");
            } else {
                var tBody = table.find('.body-course');
                tBody.append(
                    `<tr class="color-success">
                        <td>${ response.courseCode }</td>
                        <td>${ response.courseNameEn }</td>
                        <td>${ response.creditText }</td>
                        <td>${ response.requiredGradeName }</td>
                        <td></td>
                        <td class="text-center text-nowrap td-actions">
                            <a data-toggle="modal" 
                            data-target="#confirm-delete-modal" 
                            data-controller="GraduatingRequest"
                            data-action="DeleteManuallyCourse" 
                            data-modification-id="${ response.courseModificationId }"
                            data-request-id="${ response.graduatingRequestId }">
                                <i class="la la-trash delete"></i>
                            </a>
                        </td>
                    </tr>`
                )

                SuccessCallback("Save Success");
            }
        } else {
            Alert.renderAlert("Fail", "Unable to save", "error");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function submitDeleteCourseGrouping(modificationId, requestId, tableId, rowIndex) {
    var ajax = new AJAX_Helper(
        {
            url: DeleteGroupingCourse,
            data: {
                modificationId: modificationId,
                graduatingRequestId: requestId
            },
            dataType: 'json',
        }
    );

    ajax.GET().done(function (response) {
        $('#confirm-delete-modal').modal('hide');
        if (response === true) {
            SuccessCallback("Save Success");
            RenderTable.deleteRow('#' + tableId, rowIndex);
        } else {
            Alert.renderAlert("Fail", "Unable to save", "error");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('click', '.js-disable-course', function() {
    let courseGroupId = $(this).data('group-id');
    let courseId = $(this).data('course-id');
    let requestId = $(this).data('request-id');
    let moveGroupId = $(this).data('move-course-id');
    let button = $(this).find('i');
    let row = $(this).parents('tr');
    var ajax = new AJAX_Helper(
        {
            url: DisableGroupingCourse,
            data: {
                graduatingRequestId: requestId,
                courseGroupId: courseGroupId,
                courseId: courseId,
                moveCourseGroupId: moveGroupId
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) {
        if (response != "error") {
            if (response === "true") {
                row.removeClass();
                row.addClass('color-danger');
                button.removeClass();
                button.addClass('la la-check info');
            } else {
                row.removeClass();
                button.removeClass();
                button.addClass('la la-times info');
            }
            SuccessCallback("Save Success");
        } else {
            Alert.renderAlert("Fail", "Unable to save", "error");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});