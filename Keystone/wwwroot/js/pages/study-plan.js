var creditSum = 0; // for Calculate Credit
var coursesPlan = []; // for Suggestion
var studyPlanId = 0; // for Edit reference

var RenderStudyPlanInput = (function() {

    var TotalCredit = function() {
        creditSum = 0;
        $('.js-get-credit').each(function() {
            if (this.value) {
                $(this).val(parseInt(this.value))
                creditSum += parseInt(this.value);
            }
        });
        $('#js-total-credit').html(creditSum);
    }

    var CloseSuggestion = function() {
        $('.js-course-suggestion-list').removeClass('d-block');
    }

    var IsNoCourseId = function() {
        let allCourseInputs = $('.js-course-suggestion');
    
        allCourseInputs.each(function() {
            let courseId = $(this).attr('course-id');
    
            if (courseId == 0) {
                let inputIndex = $(this).attr('name').match(/\d/g);
                let courseCredit = $(`[name="StudyPlan[${ inputIndex }].Credit"]`).val();
                
                CoursePlan = {
                    Id: 0,
                    NameEN: this.value,
                    Credit: courseCredit
                }
                coursesPlan[inputIndex[0]] = CoursePlan;
            }
        })  
    }
    
    var UpdateCredits = function() {
        let allCreditInputs = $('.js-get-credit');
        allCreditInputs.each(function() {
            let parentRow = $(this).closest('tr').find('.js-course-suggestion');
            let courseId = $(parentRow).attr('course-id');
            let selectSuggest = $(parentRow).val();

            let inputIndex = $(this).attr('name').match(/\d/g);
            let courseCredit = $(`[name="StudyPlan[${ inputIndex }].Credit"]`).val();

            CoursePlan={
                Id: parseInt(courseId),
                NameEN: selectSuggest,
                Credit: parseInt(courseCredit)
            }
            coursesPlan[inputIndex[0]] = CoursePlan;
        })
    }

    return {
        totalCredit : TotalCredit,
        closeSuggestionBox : CloseSuggestion,
        isNoCourseId : IsNoCourseId,
        updateCredits : UpdateCredits
    };

})();

/* Calculate Credit Event */
// calculate when input field change or not focus
$(document).on('keyup change blur', '.js-get-credit', function() {
    RenderStudyPlanInput.totalCredit();
});

// calculate when row has delete
$(document).on('click','.js-delete-row-btn', function() {
    RenderStudyPlanInput.totalCredit();
});

/* Suggestion Event */
// show suggestion
$(document).on('keyup focus', '.js-course-suggestion', function() {
    $(this).keydown(function(e) {
        if(e.which == 9) {
            RenderStudyPlanInput.closeSuggestionBox();
        }
    });

    let suggestList = $(this).next('.js-course-suggestion-list')
    suggestList.addClass('d-block');

    let nowInput = this.value.toUpperCase();
    let allSuggest = suggestList.find('.js-course-suggestion-item');

    // compare input with each data from loadSuggestion
    $(allSuggest).each(function() {
        let suggestData = $(this).html();
        suggestData = suggestData.toUpperCase();

        if (suggestData.search(nowInput) >= 0) {
            $(this).addClass('d-block');
        } else {
            $(this).removeClass('d-block');
        }
    });
});

// select suggestion
$(document).on('click', '.js-course-suggestion-item', function() {
    let selectSuggest = $(this).html().trim();
    let relateInput = $(this).parent().prev();
    let inputIndex = relateInput.attr('name').match(/\d/g);
    let courseId = $(this).data('course-id');
    let courseCredit = $(this).data('credit');

    relateInput.val(selectSuggest);
    relateInput.attr('course-id', courseId);
    $(`[name="StudyPlan[${ inputIndex }].Credit"]`).val(courseCredit);

    CoursePlan = {
        Id: courseId,
        NameEN: selectSuggest,
        Credit: courseCredit
    }
    
    coursesPlan[inputIndex[0]] = CoursePlan;
    RenderStudyPlanInput.closeSuggestionBox();
    RenderStudyPlanInput.totalCredit();
});

// Set Default value to new row
$(document).on('click', '.js-add-row', function() {
    let newRow = $('#js-plans-1-1 tbody tr:last');
    let inputIndex = (newRow[0].rowIndex) - 1;
    let courseInput = newRow.find(`[name="StudyPlan[${ inputIndex }].CoursePlan"]`)
    let creditInput = newRow.find(`[name="StudyPlan[${ inputIndex }].Credit"]`)

    courseInput.attr('course-id', 0);
    creditInput.val(0);

    $('.js-del-row').prop( "disabled", false );
});

/* Submit Form event */
$(document).on('submit', '#js-study-plan-form', function(e) {
    e.preventDefault();
    $('#preloader').fadeIn();
    let actionType = $(this).attr('action').split('/');
    let returnUrl = $('input[name="returnUrl"]').val();

    RenderStudyPlanInput.isNoCourseId();
    RenderStudyPlanInput.updateCredits();
    SubmitStudyPlanModel = {
        Id: studyPlanId,
        CurriculumVersionId: $('#js-curriculum-id').data('curriculum-id'),
        Year: parseInt($('input[name="Year"]').val()),
        Term: parseInt($('input[name="Term"]').val()),
        CoursesPlan: coursesPlan,
        TotalCredit: parseInt($('#js-total-credit').html().trim()),
    }

    if (actionType[2] === "Create") {
        var ajax = new AJAX_Helper(
            {
                url: `${ StudyPlanCreateUrl }?returnUrl=${ returnUrl }`,
                data: JSON.stringify(SubmitStudyPlanModel),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );
    } else {
        var ajax = new AJAX_Helper(
            {
                url: `${ StudyPlanEditUrl }?returnUrl=${ returnUrl }`,
                data: JSON.stringify(SubmitStudyPlanModel),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );
    }

    ajax.POST().done(function (data) { 
        location.href = data;
        $('#preloader').fadeOut();
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        $('#preloader').fadeOut(); 
        ErrorCallback(jqXHR, jqXHR.responseText, errorThrown);
    });
});

/* Render Row Add Able */
$(document).ready(function() {
    var inputTable = new RowAddAble({
        TargetTable: '#js-plans-1-1',
        ButtonTitle: 'Course'
    })
    inputTable.RenderButton();
    RenderStudyPlanInput.totalCredit();

    let isEdit = window.location.pathname.toLowerCase().search('edit');
    if(isEdit >= 0) {
        RenderStudyPlanInput.updateCredits()
        studyPlanId = $('#js-get-plan-id').data('studyplan-id');
    }

    let tableRow = $('#js-plans-1-1 tbody').find('tr').length;
    if(tableRow <=1) {
        $('.js-del-row').prop( "disabled", true );
    }
});