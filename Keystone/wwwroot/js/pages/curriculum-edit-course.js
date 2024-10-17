$(document).on('click', '.js-add-row', function() {
    let newRow = $(`#js-course-group tbody tr:last`);
    let curriculumVersionId = $('.js-curriculum-version-id').val();
    
    newRow.find(".js-course-child").val(0);
    newRow.find(".js-curriculum-version-id-course").val(curriculumVersionId);
});