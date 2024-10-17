var addingTable = "#js-adding"
var disableRow = ".js-disable-row"
var refreshRow = ".js-refresh-row"
var creditClass = ".js-credit";
var registrationCreditClass = ".js-regist-credit";
var paidStatusClass = ".js-paid-status";
var availableCourses = "#js-items-list";
var selectedCourses = "#js-selected-course";

var setDisableBackground = "bg-danger-pastel"
var setDisableOpacity = "disable-item"
var setCrossXStatus = `<i class="color-danger la la-close"></i>`

var searchCourses = "#search-courses-modal"
var courseString = "";
var sectionString = "";
var allSelectedCourses = [];
var selectedScheduleCourses = [];
var deleteSelected = ".js-delete-selected"

var oldCourseClass = ".js-old-course"

var refundButton = ".js-refund-item"
var studentId = "#js-student-id";
var termId = "#js-term-id";
var refundCourses = [];

let creditInvalidHeader = "#js-credit-invalid-header";
let creditInvalidMessage = "#js-credit-invalid";

let examConflictHeader = "#js-exam-conflict-header";
let examConflictMessage = "#js-exam-conflict";

let classConflictHeader = "#js-class-conflict-header";
let classConflictMessage = "#js-class-conflict";

let prerequisiteHeader = "#js-prerequisite-conflict-header";
let prerequisiteMessage = "#js-prerequisite-conflict";

let corequisiteteHeader = "#js-corequisitete-conflict-header";
let corequisiteteMessage = "#js-corequisitete-conflict";

var modificationSchedule;

function getCurrentCourses() {
    var currentCourses = [];

    if (oldCourseClass.length > 0) {
        $(oldCourseClass).each(function () {
            let oldCourse = $(this).attr('course-id')
            currentCourses.push(oldCourse);
        })
    }

    return currentCourses;
}
/* [Start] ajax function area */
function getSectionsByCourse(termId, courseId) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        let sectionSuggestion = $('.js-get-sections');
        sectionSuggestion.empty();
        response.forEach((item) => {
            sectionSuggestion.append(
                `<li data-section-id="${ item.value }" data-section-number="${ item.text }" class="suggestion-result js-suggestion-item">${ item.text }</li>`
            )
        });
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function checkExamConflicts() {
    var ajax = new AJAX_Helper(
        {
            url: RegistrationCheckExamConflicts,
            data: {
                RegistrationTermId: $(termId).val(),
                AddingResults: selectedScheduleCourses
            },
            dataType: 'string'
        }
    )

    ajax.POST().done(function (response) {
        var examCheckingResponse = JSON.parse(response);

        if (examCheckingResponse.Message.length > 0) {
            $(examConflictHeader).show();
            $(examConflictMessage).html(examCheckingResponse.Message.replace(/\n/g, "<br>"));
        } else {
            $(examConflictHeader).hide();
            $(examConflictMessage).html("");
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function checkCredit() {
    var ajax = new AJAX_Helper(
        {
            url: RegistrationCheckCredit,
            data: {
                StudentId: $(studentId).val(),
                Code: $('#js-student-code').html(),
                AddingResults: selectedScheduleCourses
            },
            dataType: 'string'
        }
    )

    ajax.POST().done(function (response) {
        var creditCheckingResponse = JSON.parse(response);

        if (creditCheckingResponse.Message.length > 0) {
            $(creditInvalidHeader).show();
            $(creditInvalidMessage).html(creditCheckingResponse.Message.replace(/\n/g, "<br>"));
        } else {
            $(creditInvalidHeader).hide();
            $(creditInvalidMessage).html("");
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}
function checkScheduleConflicts() {
    var ajax = new AJAX_Helper(
        {
            url: RegistrationCheckScheduleConflicts,
            data: {
                RegistrationTermId: $(termId).val(),
                AddingResults: selectedScheduleCourses
            },
            dataType: 'string'
        }
    )

    ajax.POST().done(function (response) {
        var scheduleCheckingResponse = JSON.parse(response);

        if (scheduleCheckingResponse.Message.length > 0) {
            $(classConflictHeader).show();
            $(classConflictMessage).html(scheduleCheckingResponse.Message.replace(/\n/g, "<br>"));
        } else {
            $(classConflictHeader).hide();
            $(classConflictMessage).html("");
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function checkPrerequisite() {
    var ajax = new AJAX_Helper(
        {
            url: RegistrationCheckPrerequisite,
            data: {
                StudentId: $(studentId).val(),
                RegistrationTermId: $(termId).val(),
                AddingResults: selectedScheduleCourses
            },
            dataType: 'string'
        }
    )

    ajax.POST().done(function (response) {
        var prerequisiteResponse = JSON.parse(response);

        if (prerequisiteResponse.Message.length > 0) {
            $(prerequisiteHeader).show();
            $(prerequisiteMessage).html(prerequisiteResponse.Message.replace(/\n/g, "<br>"));
        } else {
            $(prerequisiteHeader).hide();
            $(prerequisiteMessage).html("");
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function checkCorequisitete() {
    var ajax = new AJAX_Helper(
        {
            url: RegistrationCheckCourseCorequisite,
            data: {
                StudentId: $(studentId).val(),
                RegistrationTermId: $(termId).val(),
                AddingResults: selectedScheduleCourses
            },
            dataType: 'string'
        }
    )

    ajax.POST().done(function (response) {
        var corequisiteteResponse = JSON.parse(response);

        if (corequisiteteResponse.Message.length > 0) {
            $(corequisiteteHeader).show();
            $(corequisiteteMessage).html(corequisiteteResponse.Message.replace(/\n/g, "<br>"));
        } else {
            $(corequisiteteHeader).hide();
            $(corequisiteteMessage).html("");
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getAvailableItems(termId, courseCode, courseName, sectionString) {

    var ajax = new AJAX_Helper(
        {
            url: RegistrationGetSectionsUrl,
            data: {
                termId: termId,
                courseCode: courseCode,
                courseName: courseName,
                sectionNumber: sectionString
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {

        $(availableCourses).empty().append(response);
        isDuplicate();
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeSectionByRegistrationCourse(courseId, termId, sectionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: courseId,
                sectionId: sectionId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        response.forEach((item) => {
            if (item.value == sectionId) {
                item.selected = true;
            }
            $(target).append(getSelectOptions(item.value, item.text, item.selected));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}
/* [End] ajax function area */

/* [Start] Search courses modal's function area */
function isDuplicate() {
    currentCourses = getCurrentCourses();

    $('.js-checkbox-course').each(function () {
        let currentData = $(this).data('course-id');
        let currentId = $(this).attr('id');

        for (var i = 0; i <= currentCourses.length; i++) {
            let currentOldData = parseInt(currentCourses[i])
            if (currentData == currentOldData) {
                $(`#${ currentId }`).prop('disabled', true);
                $(`#${ currentId }`).prop('checked', true);
                $(`#${ currentId }`).parents('tr').addClass(`${ setDisableBackground} ${ setDisableOpacity }`);
            }
        }
    })
}

function removeSelected(selectedData) {
    let currentDataId = $(selectedData).data('id');
    let currentDataCourseId = $(selectedData).data('course-id');
    let currentDataSectionId = $(selectedData).data('section-id');

    let objIndex = allSelectedCourses.findIndex(x => x.CourseId == currentDataCourseId &&
        x.SectionId == currentDataSectionId);
    allSelectedCourses.splice(objIndex, 1);

    $(`#${ currentDataId }`).prop('disabled', false);
    $(`#${ currentDataId }`).prop('checked', false);

    $(selectedData).remove();
}
/* [End] Search courses modal's function area */

/* [Start] Adding table function area */
function renderCreditInTable(tableRow, credit, regCredit) {
    $(tableRow).find(creditClass).html(credit)
    $(tableRow).find(registrationCreditClass).html(regCredit)

    calculateCredit(oldCredit) // this function come from registration.js (must already declare before call this file)
}

function isDuplicateSelectSuggest(courseId) {
    currentCourses = getCurrentCourses();

    for (var i = 0; i < currentCourses.length; i++) {
        let currentCourseId = parseInt(currentCourses[i])
        if (courseId === currentCourseId) {
            Alert.renderAlert("Duplicated Course", "You select a course which already added", "error")
            return true;
        }
    }

    return false;
}
/* [End] Adding table function area */

function generateAddingModel() {
    let addingRow = $(addingTable).find('tbody tr');

    selectedScheduleCourses = [];
    addingRow.each(function () {
        let course = $(this).find('.js-cascade-registration-course');
        let section = $(this).find('.js-cascade-registration-section');
        let courseCode = course.find(':selected').text().split(' ');
        let sectionNumber = section.find(':selected').text();
        let paidStatus = $(this).find('.js-input-paid-status').val();
        let regisCourseId = $(this).find('.js-registration-course-id').val();
        let courseIndex = refundCourses.findIndex(obj => obj.courseId == courseId);
        
        var addingCourse = {
            CourseId: parseInt(course.val() == null ? 0 : course.val()),
            CourseCode: courseCode[0],
            SectionId: parseInt(section.val() == null ? 0 : section.val()),
            SectionNumber: sectionNumber,
            RegistrationCourseId: regisCourseId == ' ' ? 0 : regisCourseId,
            IsPaid: paidStatus == ' ' ? 'false' : paidStatus,
            RefundItems: courseIndex != -1 ? refundCourses[courseIndex].refundList : null
        }
        selectedScheduleCourses.push(addingCourse);
    })
} // revised for require data again

/* [Start] Adding table event area */
$(document).on('click', '.js-suggestion-item', function () {
    let relateInput = $(this).parent().prev();
    let termId = $(this).data('term-id');
    let courseId = $(this).data('course-id');
    let sectionId = $(this).data('section-id');
    let courseCredit = $(this).data('credit');
    let courseRegCredit = $(this).data('reg-credit');

    if (courseId > 0) {
        if (isDuplicateSelectSuggest(courseId)) {
            $(relateInput).val('');
        } else {
            let courseCode = $(this).data('course-code');
            let courseName = $(this).data('course-name');

            relateInput.attr('course-id', courseId);
            relateInput.attr('course-code', courseCode);
            relateInput.attr('value', courseName);
            renderCreditInTable($(relateInput).closest('tr'), courseCredit, courseRegCredit)
            getSectionsByCourse(termId, courseId);
        }
    } else if (sectionId > 0) {
        let sectionNumber = $(this).data('section-number');

        relateInput.attr('section-id', sectionId);
        relateInput.attr('section-number', sectionNumber);
        relateInput.attr('value', sectionNumber);
    }
});

$(document).on('keyup', '.js-suggestion', function (e) {
    let suggestBox = $(this).parent().find('.js-suggestion-list');

    // trigger focusSuggestion by some keyboard event
    if (e.keyCode === 13) {
        //enter
        let selectedItem = $(suggestBox).find('.active');
        let termId = $(this).data('term-id');
        let courseId = $(selectedItem).data('course-id');
        let sectionId = $(selectedItem).data('section-id');
        let courseCredit = $(selectedItem).data('credit');
        let courseRegCredit = $(selectedItem).data('reg-credit');

        if ($(selectedItem).data('course-id') > 0) {
            if (isDuplicateSelectSuggest(courseId)) {
                $(this).val('');
            } else {
                let courseCode = $(selectedItem).data('course-code');
                let courseName = $(selectedItem).data('course-name');

                $(this).attr('course-id', courseId);
                $(this).attr('course-code', courseCode);
                $(this).attr('value', courseName);
                renderCreditInTable($(suggestBox).closest('tr'), courseCredit, courseRegCredit)
                getSectionsByCourse(termId, courseId);
            }
        } else if ($(selectedItem).data('section-id') > 0) {
            let sectionNumber = $(selectedItem).data('section-number');

            $(this).attr('section-id', sectionId);
            $(this).attr('section-number', sectionNumber);
            $(this).attr('value', sectionNumber);
        }
    }
});

// Set Default value to new row
$(document).on('click', '.js-add-row', function () {
    let newRow = $(`${ addingTable} tbody tr:last`);
    let isPaid = newRow.find(paidStatusClass);

    let actionButton = newRow.find(disableRow);
    let refreshButton = newRow.find(refreshRow);

    isPaid.empty().html(setCrossXStatus);

    actionButton.addClass('js-del-row').removeClass('js-disable-row d-none');
    actionButton.attr("data-target", "#delete-row-confirm-modal");
    actionButton.attr("data-toggle", "modal");
    refreshButton.remove();
});

// Set Default value to new row
$(document).on('click', '.js-del-row', function () {
    var course =  $(this).closest('tr').find('.js-cascade-registration-course option:selected').text();
    $("#delete-row-confirm-modal-content").empty().html("Are you sure to delete this " + (course == "Select" ? "item" : "<br>" + course) + " ?");
});

$(addingTable).on('click', refreshRow, function () {
    let parentRow = $(this).parent();
    parentRow.find(refundButton).addClass('d-none');
})

$(addingTable).on('click', disableRow, function () {
    var eventRow = $(this).closest('tr');

    if ($(this).hasClass('js-unregister')) {
        let allInputs = $(eventRow).find('input')

        allInputs.each(function () {
            $(this).val("");
            $(this).attr("course-id", 0);
            $(this).attr("section-id", 0);
        })

        $(eventRow).find(creditClass).html(0)
        $(eventRow).find(registrationCreditClass).html(0)
    } else {
        $(eventRow).addClass(setDisableBackground);
        $(eventRow).find('input').prop('disabled', true).addClass(setDisableOpacity);
        $(eventRow).find(`${ creditClass}, ${ registrationCreditClass }`).addClass(setDisableOpacity);

        let actionColumn = $(eventRow).find('td:last')
        actionColumn.find(refreshRow).removeClass('d-none');
        actionColumn.find(refundButton).removeClass('d-none');
        actionColumn.find(disableRow).addClass('d-none');

        let refund = $(eventRow).find('.chosen-select');
        $(refund).prop('disabled', false).trigger("chosen:updated");
    }

    calculateCredit(oldCredit)
});

/* [End] Adding table event area */

$(document).on('click', '.js-render-schedule', function () {
    $('.js-preview-schedule').removeClass('d-none');
    generateAddingModel();
    var ajax = new AJAX_Helper({
        url: RegistrationShowSchedule,
        data: {
            RegistrationTermId: $(termId).val(),
            AddingResults: selectedScheduleCourses
        },
        dataType: 'json'
    });

    ajax.POST().done(function (response) {
        if (modificationSchedule != null) {
            modificationSchedule.clearClassSchedule();
        }

        modificationSchedule = new ClassScheduleSlot({
            table: $('table', $('.js-modification-schedule')),
            dataSets: JSON.parse(response)
        });

        modificationSchedule.renderClassScheduleSlots();
        generateAddingModel();

        checkExamConflicts();
        checkScheduleConflicts();
        checkCredit();
        checkPrerequisite();
        checkCorequisitete();
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

/* Submit Form event */
$(document).on('submit', '#js-adding-submit', function(e) {
    e.preventDefault();
    $('#preloader').fadeIn();

    generateAddingModel();
    SubmitAddingModel = {
        StudentId: $(studentId).val(),
        Code: $('#js-student-code').html(),
        RegistrationTermId: $(termId).val(),
        AddingResults: selectedScheduleCourses,
    }

    var ajax = new AJAX_Helper(
        {
            url: RegistrationAddingUrl,
            data: JSON.stringify(SubmitAddingModel),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
        }
    );

    ajax.POST().done(function (data) {
        
        if (data.isSucceeded) {
            if (data.result == null) {
                location.reload();
            } else {
                location.href = '/Waive?id=' + data.result + '&returnUrl=' + location.href;
            }
        } else {
            $('#preloader').fadeOut();
            Alert.renderAlert('Error', data.errorResponse.description, 'warning')
        }
    })

    .fail(function (jqXHR, textStatus, errorThrown) {
        $('#preloader').fadeOut();
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$( function() {
    // Search courses modal's event
    $(searchCourses).one('shown.bs.modal', function () {
        var currentTermId = $(termId).val();
        $(`#${ this.id }`).on('click', '#js-search-input', function () {
            courseCode = $('#js-course-code').val();
            courseName = $('#js-course-name').val();
            sectionString = $('#js-section').val();

            getAvailableItems(currentTermId, courseCode, courseName, sectionString);
            $(".js-render-nicescroll").niceScroll();
        })

        $(`#${ this.id }`).on('change', '.js-checkbox-course', function () {
            let currentDataId = $(this).attr('id');
            let currentDataCourseId = $(this).data('course-id');
            let currentDataSectionId = $(this).data('section-id');
            let currentCourse = $(this).parent().find('.js-get-course-name').html();
            let currentSection = $(this).parent().find('.js-get-section-number').html();
            let currentCourseCode = $(this).parent().find('.js-get-course-code').html();
            let currentCredit = $(this).parent().find('.js-get-course-credit').html();
            let currentRegistrationCredit = $(this).parent().find('.js-get-registration-credit').html();
            let currentInstructor = $(this).parent().find('.js-get-course-instructor').html();

            if (allSelectedCourses.length) {
                let alreadySelectedCourses = allSelectedCourses.find(x => x.CourseId === currentDataCourseId)
                if (alreadySelectedCourses) {
                    $(`#${ currentDataId }`).prop('checked', false);
                    Alert.renderAlert("Duplicated Course", "You select a course which already added", "error")
                    return;
                }
            }

            let obj = {
                CourseId: currentDataCourseId,
                SectionId: currentDataSectionId,
                Course: currentCourse,
                Section: currentSection,
                Credit: currentCredit,
                RegistrationCredit: currentRegistrationCredit,
                Instructor: currentInstructor
            }

            $(selectedCourses).append(`
                <li data-id="${ currentDataId}" class="btn btn-rounded btn--success js-delete-selected"
                    data-course-id="${ currentDataCourseId}" data-section-id="${ currentDataSectionId}">
                    ${ currentCourseCode.toUpperCase()} - ${ currentSection} <i class="la la-close"></i>
                </li>
            `)
            allSelectedCourses.push(obj);
            $(this).prop('disabled', true);
        })

        $(`#${ this.id }`).on('click', deleteSelected, function () {
            removeSelected(this);
        })

        $('#js-confirm-add').on('click', function () {
            $(searchCourses).modal('hide')

            var allRow = $(addingTable).find('tbody tr');
            var currentRowIndex = allRow.length - 1;
            var currentRow = allRow[currentRowIndex];
            var isCourseId = $(currentRow).find(`select[name="AddingResults[${ currentRowIndex }].CourseId"]`)
            if ($(isCourseId).val() != null) {
                RenderTable.addRow(addingTable)
            }

            for (var i = 0; i < allSelectedCourses.length; i++) {
                allRow = $(addingTable).find('tbody tr');
                currentRowIndex = allRow.length - 1;
                currentRow = allRow[currentRowIndex]
                let actionButton = $(currentRow).find('.td-actions .js-disable-row');
                let courseInput = $(currentRow).find(`select[name="AddingResults[${ currentRowIndex }].CourseId"]`)
                let sectionInput = $(currentRow).find(`select[name="AddingResults[${ currentRowIndex }].SectionId"]`)
                let credit = $(currentRow).find('.js-credit')
                let registrationCredit = $(currentRow).find('.js-regist-credit')
                let mainInstructor = $(currentRow).find('.js-main-instructor')
                cascadeSectionByRegistrationCourse(allSelectedCourses[i].CourseId, currentTermId, allSelectedCourses[i].SectionId, sectionInput);
                // let refund = $(currentRow).find('.chosen-select'); // may not use
                
                $(courseInput).val(allSelectedCourses[i].CourseId).trigger("chosen:updated");
                $(mainInstructor).html(allSelectedCourses[i].Instructor)
                $(credit).html(allSelectedCourses[i].Credit)
                $(registrationCredit).html(allSelectedCourses[i].RegistrationCredit)

                $(currentRow).find(paidStatusClass).empty().html(setCrossXStatus)

                actionButton.addClass('js-del-row');
                actionButton.removeClass('js-disable-row');
                $(actionButton).attr("data-target", "#delete-row-confirm-modal");
                $(actionButton).attr("data-toggle", "modal");
                
                // $(refund).val(-1).trigger("chosen:updated");
                isCourseId = $(courseInput).attr('course-id');
                if (i < allSelectedCourses.length -1) {
                    RenderTable.addRow(addingTable)
                }
            }

            $(availableCourses).empty()
            $(deleteSelected).each(function () {
                removeSelected(this);
            })

            calculateCredit(oldCredit);
        })

        $('#js-reset-data').on('click', function () {
            $(deleteSelected).each(function () {
                removeSelected(this);
            })

            allSelectedCourses = []
            $(selectedCourses).empty();
            $(availableCourses).empty()
        })
    })

    $("#refund-modal").on('shown.bs.modal', function(event) {
        let getCourseId = $(event.relatedTarget).data('course-id');
        let getRegistrationId = $(event.relatedTarget).data('regist-course-id');
        let refundSelect = $(event.relatedTarget).prev('select');

        var ajax = new AJAX_Helper(
            {
                url: RefundCoursesGetInvoiceCourseFeeItemUrl,
                data: {
                    studentId: $(studentId).val(),
                    termId: $(termId).val(),
                    courseId: getCourseId,
                    registrationId: getRegistrationId
                },
                dataType: 'json',
            }
        );

        ajax.POST().done(function (response) {
            let courseIndex = 0;
            
            if (!refundCourses.some(obj => obj.courseId === getCourseId)) {
                refundCourses.push({
                    courseId: getCourseId,
                    refundList: []
                })
            }

            courseIndex = refundCourses.findIndex(obj => obj.courseId == getCourseId)

            $('.js-get-refund').empty();

            $(response).each(function(index) {
                let refundRow = `.js-add-refund-select-${ index }`
                refundCourses[courseIndex].refundList.push(this);
                var htmlRefundSelect = $(refundSelect).clone(false)
                
                $('.js-get-refund').append(`
                    <tr>
                        <td>${ this.feeItemName }</td>
                        <td>${ NumberFormat.renderDecimalTwoDigits(this.amount) }</td>
                        <td class="js-add-refund-select-${ index }"></td>
                    </tr>
                `)

                $(refundRow).append(htmlRefundSelect);
                let newRefundSelect = $(refundRow).find('select')
                newRefundSelect.chosen()
                               .val(this.refundPercent.toFixed(2))
                               .trigger('chosen:updated');
            })

            RenderTableStyle.columnAlign();
        })
        
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });

        $(this).on('click', '#js-confirm-refund', function() {
            let courseIndex = refundCourses.findIndex(obj => obj.courseId == getCourseId)
    
            $('.js-add-refund-select').each( function(index) {
                let refundPercent = $(this).find('select').val();
                refundCourses[courseIndex].refundList[index].refundPercent = refundPercent;
            })
    
            $("#refund-modal").modal('hide');
        })
    })
})