$('body').on('keyup', '#js-search', function() {
    let keywords = $(this).val();
    InputSuggestion.elementSuggest(keywords, '.js-suggestion-parent');
})

$(document).ready( function() {
    CheckList.renderCheckbox('#js-delete-students');
    $(".js-render-nicescroll").niceScroll();
})