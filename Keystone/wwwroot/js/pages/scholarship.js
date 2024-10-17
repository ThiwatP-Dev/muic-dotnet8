var scholarshipPercentage = '.js-scholarship-percentage';
var scholarshipAmount = '.js-scholarship-amount';

$(document).ready( function() {
    var budgetDetailTable = new RowAddAble({
        title: "Budget Details",
        TargetTable: "#js-budget-detail-table",
        ButtonTitle: "Fee Group"
    })

    var paymentConditionTable = new RowAddAble({
        title: "Payment Conditions",
        TargetTable: "#js-payment-conditions-table",
        ButtonTitle: "Payment"
    })

    budgetDetailTable.RenderButton();
    paymentConditionTable.RenderButton();
    DisablePaymentPercentageAmount();
});

$(document).on('click', '.js-add-row', function() {
    DisablePaymentPercentageAmount();
});

function DisablePaymentPercentageAmount() {
    $(scholarshipPercentage).on('change', function() {
        let currentRow = $(this).parents('tr');
        let amount = currentRow.find(scholarshipAmount);
        let percentValue = currentRow.find(scholarshipPercentage).val();
        if (percentValue) {
            $(amount).prop('disabled', true)
            $(amount).val("")
        } else {
            $(amount).prop('disabled', false)
        }
    })
    
    $(scholarshipAmount).on('keyup', function() {
        let currentRow = $(this).parents('tr');
        let amountValue = currentRow.find(scholarshipAmount).val();
        let percent = currentRow.find(scholarshipPercentage);
        if (amountValue) {
            $(percent).prop('disabled', true).trigger('chosen:updated');
        } else {
            $(percent).prop('disabled', false).trigger('chosen:updated');
        }
    })
}