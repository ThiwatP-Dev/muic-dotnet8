var financeStudent = '.js-cascade-student-code';
var financeTerm = '.js-cascade-term';
var invoiceDate = '.js-cascade-invoice-date';
var financeCheck = '.js-finance-check';
var financeAmount = '.js-finance-amount';
var financeSumAmount = '.js-finance-sum-amount';
var financeCheckAll = '.js-check-all';
var financePaid = '.js-finance-paid';
var summitPaid = '#js-summit-paid';
var financeType = '.js-finance-type';
var financePaidStatus = '.js-finance-paid-status';
var financePaidAmount = '.js-finance-paid-amount';
var sumCredit = 0;
var financeScholarship = '.js-finance-scholarship';

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

function checkFinanceType(financeType) {
    $(financeType).each( function() {
        let currentRow = $(this).closest('tr');
        if ($(this).val() === "Delete") {
            $(currentRow).addClass('disable-item');
            $(currentRow).find(financeCheck).remove();
        }
    })
}

function checkFinancePaidStatus(financePaidStatus) {
    $(financePaidStatus).each( function() {
        let currentRow = $(this).closest('tr');
        
        if ($(this).val() === "true") {
            $(currentRow).addClass('disable-item');
            $(currentRow).find(financeCheck).remove();
        }
    })
}

function checkSumAmount(checkValue, target) {
    //if ($(target).prop('checked')) {
    //    sumCredit += Number(checkValue);
    //} else {
    //    sumCredit -= Number(checkValue);
    //}

    //$(financeSumAmount).text(NumberFormat.renderDecimalTwoDigits(sumCredit))
}

$(document).ready( function() {
    CheckList.renderCheckbox('#js-finance-registration-fee');
    $(".js-render-nicescroll").niceScroll();
    $(summitPaid).prop('disabled', true);
    checkFinanceType(financeType);
    checkFinancePaidStatus(financePaidStatus);
});

$(document).on('change', financeCheck, function() {
    let currentFinanceCheck = $(this).parent('td').find(financeAmount).val();
    checkSumAmount(currentFinanceCheck, $(this));
});

$(document).on('change', financeCheckAll, function() {
    let currentFinanceCheck = $(financeCheck).closest('tr').find(financeAmount);
    if ($(financeCheckAll).prop('checked')) {
        sumCredit = 0;
    }

    $(currentFinanceCheck).each( function() {
        checkSumAmount($(this).val(), $(financeCheck));
    })
});

$(document).on('keyup', financePaid, function() {
    updateTotalAmount();
})

$(document).on('keyup', financeScholarship, function() {
    updateTotalAmount();
})

function updateTotalAmount() {
    var totalAmount = 0;
    var paymentMethods = $('#js-add-payment-method .js-finance-paid');
    paymentMethods.each((_, value) => {
        var paymentAmount = parseInt($(value).val());
        totalAmount += isNaN(paymentAmount) ? 0 : paymentAmount;
    });

    var scholarships = $('#js-add-financial-transaction .js-finance-scholarship');
    scholarships.each((_, value) => {
        var paymentAmount = parseInt($(value).val());
        totalAmount += isNaN(paymentAmount) ? 0 : paymentAmount;
    });

    $(financePaidAmount).html(NumberFormat.renderDecimalTwoDigits(totalAmount));

    let receiptAmount = parseInt($(financeSumAmount).html().replace(/[^0-9.-]+/g,""));
    if (totalAmount >= receiptAmount) {
        $(summitPaid).prop('disabled', false);
    } else {
        $(summitPaid).prop('disabled', true);
    }
}

$(document).on('keyup', financeStudent, function() {
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