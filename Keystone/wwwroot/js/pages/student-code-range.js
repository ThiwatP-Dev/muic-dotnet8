let formId = '#js-is-existed';

$('.js-confirm-button').click( function() {
    $('#preloader').fadeIn();
    let id = $(formId).find('.js-student-code-range-id').val();
    let academicLevel = $(formId).find('.js-cascade-academic-level').val();
    let startedCode = $(formId).find('.js-started-code').val();
    let endedCode = $(formId).find('.js-ended-code').val();

    var ajax = new AJAX_Helper(
        {
            url: StudentCodeRangeIsExistCodeRangeUrl,
            data: {
                id: id,
                academicLevelId: academicLevel,
                startedCode: startedCode,
                endedCode: endedCode
            },
            dataType: 'json'
        }
    );

    ajax.POST().done( function(response) {

        if (response) {
            $('#alert-is-existed-modal').modal();
        }
        else {
            $(formId).submit();
        }

    })
    .fail( function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
});

$(document).ready( function () {
    $('#alert-is-existed-modal').on('shown.bs.modal', function() {
        $('.js-confirm-btn').click( function() {
            $(formId).submit();
        })
    })
})