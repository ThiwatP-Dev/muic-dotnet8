$(document).ready(function() {
    CheckList.renderCheckbox('#js-batch-confirm-table');

    $('.js-check-all').on('change', function() {
        CheckList.renderCheckbox('#js-batch-confirm-table');
        setButtonApproval();
    })
    var table = $('#js-batch-confirm-table').DataTable({
        paging: false,
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0]
        }]
    });

   
});

function setButtonApproval() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-batch-confirm-table').find('.form-check-input:checked:not(:disabled)').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
};

$('.form-check-input').on('change', function () {
    setButtonApproval();
});