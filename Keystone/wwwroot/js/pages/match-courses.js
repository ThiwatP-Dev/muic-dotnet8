let newCourse = '.js-new-course';
let currentCurriculumVersion = '.js-get-curriculum-version';
let newCourseGroup = '.js-cascade-course-group';

$(document).ready( function() {
    $('.js-check-all').prop('checked', true);
    CheckList.renderCheckbox('#js-match-course');
    $(".js-render-nicescroll").niceScroll();
})

$('#js-match-course').on('change', newCourse, function() {
    let currentRow = $(this).closest('tr');
    let rowCourse = currentRow.find(newCourse).val();
    let rowCurriculumVersion = $(currentCurriculumVersion).val();
    let target = currentRow.find(newCourseGroup);

    cascadeCourseGroupByCurriculumsVersionAndCourse(rowCurriculumVersion, rowCourse, target);
});

$(document).on('change', '.js-change-main-grade', function() {
    let mainGradeId = $(this).val();
    let currentSection = $(this).closest('section');
    var sectionChildGrade = currentSection.find('.js-cascade-grade');
    $(sectionChildGrade).val(mainGradeId).trigger('chosen:updated');
});