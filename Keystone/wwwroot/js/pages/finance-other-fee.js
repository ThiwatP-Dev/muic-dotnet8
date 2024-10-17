var financeStudentCode = '.js-cascade-student-code';
var financeTerm = '.js-cascade-term';
var financePaid = '.js-finance-paid';
var financeSumAmount = '.js-finance-sum-amount';
var financePaidAmount = '.js-finance-paid-amount';
var feeItem = '.js-cascade-fee';
var defaultPrice = '.js-cascade-default-price';
var invoiceAmount = '#Amount';

function cascadeTermByStudentCode(studentCode, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermsByStudentCode,
            data: {
                code: studentCode
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));
        
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('keyup', financeStudentCode, function() {
    let currentSection = $(this).closest('section');
    let currentStudentCode = $(this).val();

    let sectionFinanceTerm = currentSection.find(financeTerm);

    if (currentStudentCode.length >= 7) {
        cascadeTermByStudentCode(currentStudentCode, sectionFinanceTerm);
    } else {
        sectionFinanceTerm.append(getDefaultSelectOption(sectionFinanceTerm));
        sectionFinanceTerm.trigger("chosen:updated");
    }
});

$(document).on('keyup', financePaid, function() {
    var totalAmount = 0;
    var paymentMethods = $('#js-add-payment-method .js-finance-paid');
    paymentMethods.each((_, value) => {
        var paymentAmount = parseInt($(value).val());
        totalAmount += isNaN(paymentAmount) ? 0 : paymentAmount;
    });

    $(financePaidAmount).html(NumberFormat.renderDecimalTwoDigits(totalAmount));
    $(invoiceAmount).val(totalAmount);

    let receiptAmount = parseInt($(financeSumAmount).html().replace(/[^0-9.-]+/g,""));
    if (totalAmount >= receiptAmount) {
        $(summitPaid).prop('disabled', false);
    } else {
        $(summitPaid).prop('disabled', true);
    }
});

$(document).on('change', feeItem, function() {
    let currentRow = $(this).parents('tr');
    let priceCell = currentRow.find(defaultPrice);
    let feeItemId = $(this).val();

    getFeeItemDefaultPrice(feeItemId, priceCell);
});

function getFeeItemDefaultPrice(feeItemId, priceCell) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetFeeItemDefaultPrice,
            data: {
                feeItemId: feeItemId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        priceCell.empty().html(NumberFormat.renderDecimalTwoDigits(response))
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}