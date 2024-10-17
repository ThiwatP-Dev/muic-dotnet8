var academicLevel = '.js-cascade-academic-level';
var faculty = '.js-cascade-faculty';
var term = '.js-cascade-admission-term';
var admissionRound = '.js-cascade-admission-round'
var curriculumsBlock = '#js-curriculums';

$( function() {
    $(academicLevel).prop('disabled', true).trigger('chosen:updated');
    $(term).prop('disabled', true).trigger('chosen:updated');
    $(admissionRound).prop('disabled', true).trigger('chosen:updated');
    $(faculty).prop('disabled', true).trigger('chosen:updated');

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
        let selectedItems = $(itemsBlock).find('input');
        let itemsValue = []

        selectedItems.each(function(){
            itemsValue.push(this.value);
        })
        $(".js-check-all").prop('checked', false);

        response.forEach((item, index) => {
            if (!itemsValue.includes(item.value)) {
                itemsBlock.append(
                    `<div class="js-suggestion-parent">
                        <input id="check${ item.value }" class="form-check-input" name="curriculumVersionIds" type="checkbox" value="${ item.value }">
                        <label class="js-focus-item m-0" for="check${ item.value }">${ item.text }</label>
                        <hr class="w-100x">
                    </div>`
                )
            }
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

$('#js-submit-form').click( function() {

    $(academicLevel).prop('disabled', false).trigger('chosen:updated');
    $(term).prop('disabled', false).trigger('chosen:updated');
    $(admissionRound).prop('disabled', false).trigger('chosen:updated');
    $(faculty).prop('disabled', false).trigger('chosen:updated');

    $('form').submit();
});