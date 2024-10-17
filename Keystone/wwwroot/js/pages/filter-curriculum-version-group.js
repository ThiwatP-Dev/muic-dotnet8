$(document).ready(function() {
    CheckList.renderCheckbox('#js-select-curriculum-version');
    $(".js-render-nicescroll").niceScroll();
    
    $('#add-filter-curriculum-version-modal').on('shown.bs.modal', function() {
        $('.chosen-select').chosen();
        $("#filter-curriculum-version-select").empty();
        $(".filter-curriculum-version-group-none").addClass('d-none');

        $('.js-button-search').on('click', function(){
            $('#preloader').fadeIn();
            var formData = $('#filter-curriculum-version-group').serialize();
            
            var ajax = new AJAX_Helper(
                {
                    url: FilterGroupGetCurriculumVersions,
                    data: formData,
                    dataType: 'application/x-www-form-urlencoded; charset=utf-8'
                }
            );
        
            return ajax.POST().done(function (response) {
                if (response != null) {
                    $("#filter-curriculum-version-select").empty().html(response);
                    $(".filter-curriculum-version-group-none").removeClass('d-none');
                    CheckList.renderCheckbox('#js-select-curriculum-version');
                    $(".js-render-nicescroll").niceScroll();
                }

                setAddCurriculumVersionSubmitButton();
                $('.form-check-input, .js-check-all').on('change', function() {
                    setAddCurriculumVersionSubmitButton();
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

function setAddCurriculumVersionSubmitButton() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-select-curriculum-version').find('.form-check-input:checked').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}