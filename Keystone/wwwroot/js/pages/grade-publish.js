$(document).ready(function() {
    CheckList.renderCheckbox('#js-grade-approval-table');
    $(".js-render-nicescroll").niceScroll();
    $('.js-check-all').on('change', function() {
        CheckList.renderCheckbox('#js-grade-approval-table');
        setButtonPublishes();
    })

    $('#confirm-publish-modal').on('show.bs.modal', function (event) {
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

$('.form-check-input').on('change', function() {
    setButtonPublishes();
})

function setButtonPublishes() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-grade-approval-table').find('.form-check-input:checked:not(:disabled)').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}