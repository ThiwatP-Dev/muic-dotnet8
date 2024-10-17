var changeDateCheckbox = '.js-change-date';
var changeDateAll = '.js-check-all';
var dateInputArea = '.js-get-date';
var getDateInput = '.js-single-date';
var changeDateInputs = '.js-refund-date';
var refundBankTable = '#js-refund-bank';

function dateInputTrigger() {
    let count = 0;
    $(changeDateCheckbox).each( function(index) {
        if ($(this).prop('checked')) {
            count++;
        }
    })

    if (count > 0) {
        $(dateInputArea).removeClass('disable-item');
        $(dateInputArea).find(getDateInput).prop('disabled', false);
    } else {
        $(dateInputArea).addClass('disable-item');
        $(dateInputArea).find(getDateInput).prop('disabled', true);
    }
}

function changeDate(newDate, currentCheck, currentRefundDate) {
    if (newDate != "" && currentCheck.prop('checked')) {
        currentRefundDate.val(newDate);
    } else {
        let oldRefundDate = currentRefundDate.data('original-date');
        currentRefundDate.val(oldRefundDate);
    }
}

$(refundBankTable).on('change', changeDateCheckbox, function() {
    dateInputTrigger();

    let newDate = $(dateInputArea).find(getDateInput).val();
    let currentRefundDate = $(this).parents('tr').find(changeDateInputs);
    changeDate(newDate, $(this), currentRefundDate);
})

$(refundBankTable).on('change', changeDateAll, function() {
    dateInputTrigger();

    let newDate = $(dateInputArea).find(getDateInput).val();
    $(changeDateCheckbox).each( function() {
        let currentRefundDate = $(this).parents('tr').find(changeDateInputs);
        changeDate(newDate, $(this), currentRefundDate);
    })
})

$('body').on('apply.daterangepicker', `${ dateInputArea } > ${ getDateInput }`, function() {
    let newDate = $(this).val();
    $(changeDateCheckbox).each( function() {
        let currentRefundDate = $(this).parents('tr').find(changeDateInputs);
        changeDate(newDate, $(this), currentRefundDate);
    })
})

$('#js-submit-change-date').submit( function() {
    $('#preloader').fadeIn();
    let returnPath = window.location.pathname + window.location.search;
    let refundIds = [];

    $(changeDateCheckbox).each( function() {
        if ($(this).prop('checked')) {
            let currentRefundDate = $(this).parents('tr').find(changeDateInputs);
            refundIds.push(currentRefundDate.data('items'));
        }
    })

    var ajax = new AJAX_Helper(
        {
            url: RefundBankChangeRefundedAtUrl,
            data: {
                refundedAt : $(`${ dateInputArea } > ${ getDateInput }`).val(),
                refundIds : refundIds,
                redirectPath : returnPath
            },
            dataType: 'json',
        }
    );

    ajax.POST().done(function (response) {
        window.location.href = response;
        window.location.reload(true);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
    $('#preloader').fadeOut();
})

$(document).ready(function() {
    CheckList.renderCheckbox(refundBankTable);
})