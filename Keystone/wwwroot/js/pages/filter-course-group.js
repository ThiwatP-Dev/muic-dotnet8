$(document).ready(function() {
    CheckList.renderCheckbox('#js-select-course');
    $(".js-render-nicescroll").niceScroll();
    
    $('#add-filter-course-modal').on('shown.bs.modal', function() {
        $('.chosen-select').chosen();
        $("#filter-course-select").empty();
        $(".filter-course-group-none").addClass('d-none');

        $('.js-button-search').on('click', function(){
            $('#preloader').fadeIn();
            var formData = $('#filter-course-group-course').serialize();
            
            var ajax = new AJAX_Helper(
                {
                    url: FilterCourseGroupGetCourses,
                    data: formData,
                    dataType: 'application/x-www-form-urlencoded; charset=utf-8'
                }
            );
        
            return ajax.POST().done(function (response) {
                if (response != null) {
                    $("#filter-course-select").empty().html(response);
                    $(".filter-course-group-none").removeClass('d-none');
                    CheckList.renderCheckbox('#js-select-course');
                    $(".js-render-nicescroll").niceScroll();
                }

                setAddCourseSubmitButton();
                $('.form-check-input, .js-check-all').on('change', function() {
                    setAddCourseSubmitButton();
                })
                $('#preloader').fadeOut();                
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                $('#preloader').fadeOut();
                Alert.renderAlert("Warning", "Please fill all required data.", "warning");
            });
        })
    })
});

function setAddCourseSubmitButton() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-select-course').find('.form-check-input:checked').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}