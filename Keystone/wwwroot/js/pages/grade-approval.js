$(document).ready(function() {
    CheckList.renderCheckbox('#js-grade-approval-table');
    $(".js-render-nicescroll").niceScroll();
    $('.js-check-all').on('change', function() {
        CheckList.renderCheckbox('#js-grade-approval-table');
        setButtonApproval();
    })
    var table = $('#js-grade-approval-table').DataTable({
        paging: false,
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [1, 3, 8, 9, 10, 11,14]
        },
        {
            "targets": [9],
            "orderDataType": "dom-text"
        }]
    });

    $('#confirm-approve-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let controller = button.data('controller');
        let action = button.data('action');
        let barcodeId = button.data('barcode-id');
        let returnUrl = button.find('.js-return-url').val();
        let confirmButton = $(this).find('.js-confirm-btn');

        let fullRoute;
        fullRoute = `/${ controller }/${ action }?barcodeIds=${ barcodeId }&returnUrl=${ returnUrl }`;
        confirmButton.attr("href", `${ fullRoute }`);
    });

    $('#confirm-revert-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let controller = button.data('controller');
        let action = button.data('action');
        let barcodeId = button.data('barcode-id');
        let returnUrl = button.find('.js-return-url').val();
        let confirmButton = $(this).find('.js-confirm-btn');

        let fullRoute;
        fullRoute = `/${ controller }/${ action }?barcodeId=${ barcodeId }&returnUrl=${ returnUrl }`;
        confirmButton.attr("href", `${ fullRoute }`);
    });
});

function setButtonApproval() {
    var saveButton = $('.js-save-button');
    var previewButton = $('.js-preview-button');
    var atLeastOneIsChecked = $('#js-grade-approval-table').find('.form-check-input:checked:not(:disabled)').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
        previewButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
        previewButton.prop('disabled', true);
    }
};

$('.form-check-input').on('change', function() {
    setButtonApproval();
});

$('.js-save-button').on('click', function() {
    var form = '#js-grade-approval-form';
    $(form).attr('action', '/GradeApproval/Approves');
});

$('.js-preview-button').on('click', function() {
    var form = '#js-grade-approval-form';
    $(form).attr('action', '/GradeApproval/Report');
});