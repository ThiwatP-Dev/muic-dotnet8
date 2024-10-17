var courseIds = [];
var sectionIds = [];

var gradingFormId = '#grading-wizard';

var allocationTableId = '#js-allocation-table';
var scoringTableId = '#js-scoring-table';

var allocationAdded = '.js-allocation-type';
var allocationType = 'input[name="allocationType"]';

function getSectionsByCourses(courseIds) {

    var ajax = new AJAX_Helper(
        {
            url: GradeManagementGetSectionsUrl,
            data: {
                termId: $('#TermId').val(),
                courseIds: courseIds,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        let sectionTableBody = $('#js-section-table .js-items');
        sectionTableBody.empty();

        response.forEach((item, index) => {
            let checkedExisted = "";
            if (item.isSelected) {
                checkedExisted = "checked";
            }

            sectionTableBody.append(
                `<tr>
                    <td class="text-center">
                        <input id="section-${ index }" class="form-check-input js-selected-section" name="Sections" type="checkbox" data-section-id="${ item.sectionId }" data-children-section-id="${ item.childrenSectionIds }" ${ checkedExisted }>
                        <label class="form-table" for="section-${ index }"></label>
                    </td>
                    <td>${ item.courseCode }</td>
                    <td>${ item.courseName }</td>
                    <td class="text-center">${ item.sectionNumber }</td>
                    <td class="text-center">${ item.totalStudent }</td>
                    <td class="text-center">${ item.totalWithdrawal }</td>
                </tr>`
            )
        });

        CheckList.renderCheckbox('.js-checklist-sections');
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getStudentsBySections(sectionIds, childrenSectionIds) {
    $("#js-table-text-row").show();
    $("#js-table-text").text("Loading...");
    var ajax = new AJAX_Helper(
        {
            url: GradeManagementGetStudentsUrl,
            data: {
                ids: JSON.stringify(sectionIds),
                childrenids: '[' + childrenSectionIds + ']',
                gradingAllocationId: $('#js-grading-allocation-id').val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $('#js-get-students-score').empty().html(response);
        RenderTableStyle.columnAlign();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getGradeInfo(currentGrade, studentScoreId, allocationId) {
    var ajax = new AJAX_Helper(
        {
            url: GradeManagementGetGradingLog,
            data: {
                studentScoreId: studentScoreId,
                gradingAllocationId: allocationId
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        $('#select-grade').empty()
        response.grades.forEach(element => {
            if (element.name != currentGrade) {
                $('#select-grade').append(new Option(element.name, element.id));
            }
        });

        // $('#js-get-students-score').empty().html(response);
        // RenderTableStyle.columnAlign();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getCurrentAllocation() {
    var alreadyAddList = [];
        
    $(allocationAdded).each( function() {
        alreadyAddList.push($(this).val())
    })

    $(allocationType).each( function() {
        if (alreadyAddList.includes($(this).data('value'))) {
            $(this).prop('checked', true);
        }
    });

    return alreadyAddList;
}

function saveEditGrade(studentScoreId) {
    var ajax = new AJAX_Helper(
        {
            url: GradeManagementSaveStudentScore,
            data: {
                studentScoreId: studentScoreId,
                gradeId: $('#select-grade').val(),
                remark: $('#remark').val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $('#result-edit-grade-modal').modal('hide');
        calculateGradingResult($('#js-grading-allocation-id').val());
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).ready(function() {

    if ($('.js-get-course-ids').val() != "") {
    
        let currentCourses = $('.js-get-course-ids');
        currentCourses.each(function() {
            courseIds.push($(this).val());
        })
    }

    $('#allocation-template-modal').one('shown.bs.modal', function() {
        alreadyAddList = getCurrentAllocation()

        $('#js-confirm-selected').click(function() {
            alreadyAddList = getCurrentAllocation();

            let tableBody = $(`${ allocationTableId } tbody`);
            let lastestRow = tableBody.find('tr:last');

            $(allocationType).each( function() {
                let clonedRow = lastestRow.clone(false);

                if ($(this).prop('checked') === true) {
                    let type = $(this).data('value');
                    let abbr = $(this).data('abbr')

                    if (!alreadyAddList.includes(type)) {
                        if (lastestRow.find(allocationAdded).val() === "") {
                            lastestRow.remove();
                        }

                        let inputs = clonedRow.find('input');
                        let rowIndex = tableBody.find('tr').length
                        inputs.val("");
                        clonedRow.find(allocationAdded).val(type);
                        clonedRow.find('.js-type-abbr').val(abbr);

                        inputs.each(function() {
                            if (this.hasAttribute("name")) {
                                let nameValue = $(this).attr("name").replace(RegularExpressions.AllDigitInBracket, `[${ rowIndex }]`);
                                $(this).attr('name', nameValue);
                            }

                            if (this.hasAttribute("id")) {
                                let idValue = $(this).attr("id").replace(RegularExpressions.AllDigitInParenthesis, rowIndex);
                                $(this).attr('id', idValue);
                            }
                        })
                        
                        tableBody.append(clonedRow);
                    }
                }
            })
    
            $('#allocation-template-modal').modal('hide');
        })
    })

    $('#sections-modal').one('shown.bs.modal', function() {
        getSectionsByCourses(courseIds)

        $('#js-confirm-add').click(function() {
            sectionIds = [];
            childrenSectionIds = "";

            $('.js-selected-section').each(function() {

                if ($(this).prop('checked') === true) {
                    let currentId = $(this).data("section-id")
                    let currentChildrenSectionIds = $(this).data("children-section-id")
                    sectionIds.push(currentId)
                    childrenSectionIds += (childrenSectionIds !== "" ? "," : "") + currentChildrenSectionIds
                }
            })

            getStudentsBySections(sectionIds, childrenSectionIds)
            $('#sections-modal').modal('hide');
        })
    })

    $('#result-edit-grade-modal').on('shown.bs.modal', function(event) {
        let button = event.relatedTarget;
        let studentScoreId = $(button).data('studentscoreid');
        var allocationId = $('#js-grading-allocation-id').val();
        var prefix = $(button).data('value');
        var grade = $('#' + prefix + '_GradeId').html();
        $('#current-grade').val(grade);
        $('#student-score-id').val(studentScoreId);
        getGradeInfo(grade, studentScoreId, allocationId);
    });

    $('#js-save-edit-grade').click(function() {
        saveEditGrade($('#student-score-id').val());
    });

    // $('#result-edit-grade-modal').on('hidden.bs.modal', function(event) {
    //     $('#result-edit-grade-modal').modal('dispose');
    // });
})