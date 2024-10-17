let studentChecklist = '#js-choose-student';
let studentList = '#js-student-list';

var academicLevelId = '.js-cascade-academic-level';
var studentCodeFrom = '.js-value-code-from';
var studentCodeTo = '.js-value-code-to';
var schoolGroupId = '.js-cascade-school-group';
var previousSchoolId = '.js-cascade-previous-school';
var verificationLetter = '.js-value-verification-letter-id';
var batchFrom = '.js-batch-from';
var batchTo = '.js-batch-to';

$('body').on('click', '#js-get-student', function() {
    if ($(academicLevelId).val() === null) {
        FlashMessage.header("Danger", ErrorMessage.RequiredData);
    } else {
        $('#flash-message').empty();
        $('#preloader').fadeIn();
        GetStudent();
        $('#preloader').fadeOut();
    }
});

function GetStudent() {
    let model = {
        Id: $(verificationLetter).val(),
        academicLevelId: $(academicLevelId).val(),
        admissionTermId: $(admissionTerm).val(),
        admissionRoundId: $(admissionRound).val(),
        studentCodeFrom: $(studentCodeFrom).val(),
        studentCodeTo: $(studentCodeTo).val(),
        schoolGroupId: $(schoolGroupId).val(),
        previousSchoolId: $(previousSchoolId).val(),
        batchFrom: $(batchFrom).val(),
        batchTo: $(batchTo).val()
    };

    var ajax = new AJAX_Helper(
        {
            url: VerificationLetterGetStudentsUrl,
            data: JSON.stringify(model),
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
        }
    );

    ajax.POST().done(function (response) {
        $(studentList).html(response);

        CheckList.renderCheckbox(studentChecklist);
        $('.js-render-nicescroll').niceScroll();
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
};

$('#js-search').on('keyup', function() {
    let keywords = $(this).val();
    InputSuggestion.elementSuggest(keywords, '.js-suggestion-parent');
});

$(function() {
    let haveItems = $('.js-items').find('.js-suggestion-parent').length;

    if (haveItems > 0) {
        CheckList.renderCheckbox(studentChecklist);
        $('.js-render-nicescroll').niceScroll();
    }
});