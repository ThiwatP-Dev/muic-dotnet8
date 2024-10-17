let termCoordinator = '#js-get-term-coordinator';
let courseSelect = '#js-cascade-course';
let instructorsSelect = '.js-cascade-instructors';

$(courseSelect).on('change', function() {
    var ajax = new AJAX_Helper(
        {
            url: CoordinatorSearchByCourseUrl,
            data: {
                termId: $(termCoordinator).data('id'),
                courseId: $(this).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done( function(response) { 
        $(instructorsSelect).each( function(index) {
            let currentSelect = $(this)
            currentSelect.empty();

            $(response[index]).each( function(index, item) {
                currentSelect.append(getSelectOptions(item.value, item.text, item.selected))
            });

            currentSelect.trigger("chosen:updated")
        })
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(document).ready(function() {
    $('.js-render-nicescroll').niceScroll();
    $('.chosen-container-multi').addClass('minimal-primary')
})