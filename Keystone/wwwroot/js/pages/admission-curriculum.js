var faculty = '.js-cascade-faculty';
var term = '.js-cascade-admission-term';
var admissionRound = '.js-cascade-admission-round'
var curriculumsBlock = '#js-curriculums';

$(faculty).on('change', function() {
    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetCurriculumVersionForAdmissionCurriculum,
            data : { 
                facultyId: $(faculty).val(),
                admissionRoundId: $(admissionRound).val(),
                termId : $(term).val()
            },
            dataType : 'json',
        }
    );
    
    ajax.POST().done(function (response) {
        let itemsBlock = $(curriculumsBlock).find('.js-items');
        itemsBlock.empty();

        response.forEach((item, index) => {
            itemsBlock.append(
                `<div class="js-suggestion-parent">
                    <input id="check${ index }" class="form-check-input" name="curriculumVersionIds" type="checkbox" value="${ item.value }">
                    <label class="js-focus-item m-0" for="check${ index }">${ item.text }</label>
                    <hr class="w-100x">
                </div>`
            )
        });

        CheckList.renderCheckbox(curriculumsBlock);
        $(".js-render-nicescroll").niceScroll();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$('#js-search').on('keyup', function() {
    let keywords = $(this).val();
    InputSuggestion.elementSuggest(keywords, '.js-suggestion-parent');
})

$(document).ready( function() {
    let haveItems = $('.js-items').find('.js-suggestion-parent').length;

    if (haveItems > 0) {
        CheckList.renderCheckbox(curriculumsBlock);
        $(".js-render-nicescroll").niceScroll();
    }
})