$(document).ready(function() {
    // confirm with submit form
    $('#confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        
        let form = button.data('form-id');
        let confirmButton = $('.js-confirm-btn');

        confirmButton.click( function() {
            $(form).submit();
        })
    });
    
    $('#change-status-confirm-modal').on('show.bs.modal', function (event) {
        let fullRoute;

        let button = $(event.relatedTarget); // Button that triggered the modal
        let buttonHref = button.attr('href');

        $('#change-status-confirm-modal-text').text(button.data('message'));

        if (buttonHref !== "" && buttonHref !== undefined) {
            fullRoute = buttonHref;
        } else {
            let controller = button.data('controller');
            let action = button.data('action');
            let value = button.data('value');
            let returnUrl = button.data('return-url');
            

            if (returnUrl != 'undefined') {
                fullRoute = `/${ controller }/${ action }?id=${ value }&returnUrl=${ returnUrl }`;
                console.log(value);
            } else {
                fullRoute = `/${ controller }/${ action }/${ value }`;
            }

            $('input[name=id]').val(value);
        }

        let confirmButton = $(this).find('.js-delete-confirm-btn,.js-confirm-btn');
        confirmButton.attr("href", `${ fullRoute }`);
    });

    // confirm with form inside modal to redirect with criteria
    $('#confirm-redirection-criteria-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        
        let controller = button.data('controller');
        let action = button.data('action');
        let data = button.data('id');
        let redirection = button.data('return-url');
        
        let form = $(this).find('form');
        let confirmButton = $('.js-confirm-btn');

        $(form).attr('action', `${ controller }/${ action }`);
        $('#js-stash-id').val(data);
        $('#js-stash-redirection').val(redirection);

        confirmButton.click( function() {
            $(form).submit();
        })
    });

    // confirm with link redirection
    $('#confirm-redirection-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        
        let controller = button.data('controller');
        let action = button.data('action');
        let routeData = button.data('route');
    
        let fullRoute = `/${ controller }/${ action }?${ routeData }`;
    
        let confirmButton = $('.js-confirm-btn');
        confirmButton.attr("href", `${ fullRoute }`)
    });

    // confirm or delete with single query data
    $('#delete-confirm-modal,#confirm-action-modal,#cancel-confirm-modal,#update-confirm-modal').on('show.bs.modal', function (event) {
        let fullRoute;

        let button = $(event.relatedTarget); // Button that triggered the modal
        let buttonHref = button.attr('href');

        if (buttonHref !== "" && buttonHref !== undefined) {
            fullRoute = buttonHref;
        } else {
            let controller = button.data('controller');
            let action = button.data('action');
            let value = button.data('value');
            let returnUrl = button.data('return-url');
            

            if (returnUrl != 'undefined') {
                fullRoute = `/${ controller }/${ action }?id=${ value }&returnUrl=${ returnUrl }`;
            } else {
                fullRoute = `/${ controller }/${ action }/${ value }`;
            }

            $('input[name=id]').val(value);
        }

        let confirmButton = $(this).find('.js-delete-confirm-btn,.js-confirm-btn,.js-cancel-confirm-btn,.js-update-confirm-modal');
        confirmButton.attr("href", `${ fullRoute }`);
    });

    // delete confirm then back to tab by index
    $('#delete-confirm-modal-tab').on('show.bs.modal', function(event) {
        let button = $(event.relatedTarget);
        let value = button.data('value');
        let tabIndex = button.data('tab-index');
        let onclick = button.data('onclick');
        let deleteConfirmBtn = $('.js-delete-confirm-tab-btn');
        deleteConfirmBtn.attr("onclick", `${ onclick }(${ value },${ tabIndex })`)
    });
    
});

$('.block__action-row').on('click', '.js-delete-confirm-btn', function() {
    $('#delete-confirm-form').submit();
});