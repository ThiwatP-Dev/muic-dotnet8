let tuitionFeeTable = '#js-printing-log-report';
let saveEdit = '.js-save-edit';

let referenceNumber = '#js-reference-number';
let gender = '#js-gender';
let startedDate = '#js-started-date';
let endedDate = '#js-ended-data';
let language = '#js-language';
let urgentStatus = '#js-urgent-status';
let printStatus = '#js-print-status';
let paidStatus = '#js-paid-status';


$(document).ready(function() {
    EditTable.renderEditTable(tuitionFeeTable);
});

$(saveEdit).click(function() {
    let tr = $(this).closest('tr');
    let id = tr.find('#js-printing-log-id').val();
    let trackingNumber = tr.find('#js-tracking-number').val();

    let model = {
        PrintingLogId: id,
        TrackingNumber: trackingNumber,
    }

    var ajax = new AJAX_Helper(
        {
            url: PrintingLogSaveTrackingNumberUrl,
            data: JSON.stringify(model),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8'
        }
    );

    ajax.POST().done(function (response) { 
        if (response) {
            FlashMessage.header("Confirmation", "Saved.");
        } else {
            FlashMessage.header("Danger", "Unable to save, invalid input in some fields.");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});