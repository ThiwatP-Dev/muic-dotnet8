var planTable = $('#js-plan-courses');
var totalCredit = 0;
var totalRegistCredit = 0;

function calculateCredit() {

    totalCredit = 0;
    $('.js-credit').each(function() {
        if ($(this).html()) {
            totalCredit += parseInt($(this).html());
        }
    });
    $('#js-total-credit').html(totalCredit);

    totalRegistCredit = 0;
    $('.js-registration-credit').each(function() {
        if ($(this).html()) {
            totalRegistCredit += parseInt($(this).html());
        } 
    });
    $('#js-total-registration-credit').html(totalRegistCredit);
}

$('#js-add-course').on('click', function() {
    $('#default-row').remove();

    var ajax = new AJAX_Helper({
        url : PlanCourseDetailsUrl,
        data : { 
            id: $('#ajax-course').val(), 
        },
        dataType : 'html',
    });

    ajax.POST().done(function (response) { 
        planTable.find('tbody').append(response)
        RenderTableStyle.columnAlign();
        calculateCredit();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(document).on('click', '.js-delete-row-btn', function() {
    calculateCredit();
})

$(document).ready(function() {
    calculateCredit();

    $('#selectable-sections-modal').on('shown.bs.modal', function() {
        CheckList.renderCheckAllBtn('.js-selectable-section', '#js-check-all')

        $('#js-section-search').on('keyup', function() {
            let keywords = $(this).val();
            InputSuggestion.elementSuggest(keywords, '.js-suggestion-parent');
        })

        $('#js-select-section').on('click', function() {
            $('#selectable-sections-modal').modal('hide');

            var sectionCount = 0;
            $('.js-selectable-section').each(function() {

                if(this.checked) {
                    sectionCount++
                }
            })
        })

        $('#js-seat-available').on('change', function() {
            var sectionSeat = $('.seat-count');

            if (this.checked) {
                sectionSeat.each(function() {
                    let focusSectionBlock = $(this).parents('.js-suggestion-parent')
    
                    if($(this).html() < 1) {
                        focusSectionBlock.addClass('d-none')
                    }
                })
            } else {
                sectionSeat.each(function() {
                    $(this).parents('.js-suggestion-parent').removeClass('d-none')
                })
            }
        })
    })
})