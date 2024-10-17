var FlashMessage = (function() {
    var HeaderMessage = function(type, message) {
        if (type == "Confirmation") {
            $('#flash-message').html(`<div class='alert alert-success alert-dismissible'role='alert'>
                                        <button type='button' class='close' data-dismiss='alert' aria-label='Close'>
                                            <span aria-hidden='true'>×</span>
                                        </button> ${ message }
                                    </div>`)
        } else if (type == "Danger") {
            $('#flash-message').html(`<div class="alert alert-danger alert-dismissible" role="alert">
                                        <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                                            <span aria-hidden="true">×</span>
                                        </button> ${ message }
                                    </div>`)
        } else if (type == "Warning") {
            $('#flash-message').html(`<div class="alert alert-warning alert-dismissible" role="alert">
                                        <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                                            <span aria-hidden="true">×</span>
                                        </button> ${ message }
                                    </div>`)
        }
    }

    var TabMessage = function(type, message, tabIndex) {
        if (type == "Confirmation") {
            $(`#flash-message-${ tabIndex }`).html(`<div class='alert alert-success alert-dismissible'role='alert'>
                                                     <button type='button' class='close' data-dismiss='alert' aria-label='Close'>
                                                         <span aria-hidden='true'>×</span>
                                                     </button> ${ message }
                                                </div>`)
        } else if (type == "Danger") {
            $(`#flash-message-${ tabIndex }`).html(`<div class="alert alert-danger alert-dismissible" role="alert">
                                                     <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                                                         <span aria-hidden="true">×</span>
                                                     </button> ${ message }
                                                </div>`)
        }
    }

    return {
        tab : TabMessage,
        header : HeaderMessage
    }

})();