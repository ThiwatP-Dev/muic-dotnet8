var cascadeDepartment = function(facultyId) {

    var select = $('.js-department-select');
    var options = select.find('option');

    options.map(function(index, item) {

        if(facultyId != '') {
            if (String($(item).data('faculty-id')) != facultyId) {
                $(item).hide();
            } else {
                $(item).show();
            }
        } else {
            $(item).show();
        }
    });

    select.trigger("chosen:updated");
};

var cascadeCourse = function(facultyId) {

    var select = $('.js-course-select');
    var options = select.find('option');

    options.map(function(index, item) {

        if(facultyId != '') {
            if (String($(item).data('faculty-id')) != facultyId) {
                $(item).hide();
            } else {
                $(item).show();
            }
        } else {
            $(item).show();
        }
    });

    select.trigger("chosen:updated");
};

$(function(){
    $('.modal').modal('show');

    $('.js-faculty-select').on('change', function() {
        cascadeDepartment($(this).val());
        cascadeCourse($(this).val());
    });
});