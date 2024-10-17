let certificateType = '#js-certificate-type';
let receiptYear = '.js-academic-year-cascade';
let receiptTerm = '.js-academic-term-cascade';
let receipt = '.js-receipt-cascade';
let code = '.js-student-code';

$(certificateType).on('change', function() {
    var certificateTypeText = $(this).val();
    if (certificateTypeText === 'ExpensesOutlineCertificate') {
        $('#js-year-input').removeClass('d-none');
        $('#js-term-input').removeClass('d-none');
        $('#js-receipt-select').removeClass('d-none');
    } else {
        $('#js-year-input').addClass('d-none');
        $('#js-term-input').addClass('d-none');
        $('#js-receipt-select').addClass('d-none');
    }
})

function cascadeReceipt(code, year, term) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetReceiptNumberByStudentCodeAndTerm,
            data: {
                code: code,
                year: year,
                term: term
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(receipt).empty();

        response.forEach((item) => {
            $(receipt).append(getSelectOptions(item.value, item.text));
        });

        $(receipt).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', code, function() {
    let studentCode = $(this).val();
    let year = $(receiptYear).val();
    let term = $(receiptTerm).val();
    if (studentCode != '' && year != 0 && term != 0 ) {
        cascadeReceipt(studentCode, year, term);
    }
});

$(document).on('change', receiptYear, function() {
    let studentCode = $(code).val();
    let year = $(this).val();
    let term = $(receiptTerm).val();
    if (studentCode != '' && year != 0 && term != 0 ) {
        cascadeReceipt(studentCode, year, term);
    }
});

$(document).on('change', receiptTerm, function() {
    let studentCode = $(code).val();
    let year = $(receiptYear).val();
    let term = $(this).val();
    if (studentCode != '' && year != 0 && term != 0 ) {
        cascadeReceipt(studentCode, year, term);
    }
});