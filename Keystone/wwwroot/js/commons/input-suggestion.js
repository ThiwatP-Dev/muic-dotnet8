var InputSuggestion = (function() {

    var TimeSuggest = function(suggestBox) {
        var timeSlots = [
            "08:00", "08:20", "08:30", "08:50", 
            "09:00", "09:20", "09:30", "09:50", 
            "10:00", "10:20", "10:30", "10:50", 
            "11:00", "11:20", "11:30", "11:50", 
            "12:00", "12:20", "12:30", "12:50",
            "13:00", "13:20", "13:30", "13:50", 
            "14:00", "14:20", "14:30", "14:50", 
            "15:00", "15:20", "15:30", "15:50", 
            "16:00", "16:20", "16:30", "16:50", 
            "17:00", "17:20", "17:30", "17:50",
            "18:00", "18:20", "18:30", "18:50", 
            "19:00", "19:20", "19:30", "19:50", 
            "20:00", "20:20", "20:30", "20:50"
        ];
        for (let i = 0; i < timeSlots.length; i++) {
            $(suggestBox).append(
                `<li class="suggestion-result js-suggestion-item">${ timeSlots[i] }</li>`
            )
        }
    }

    var FocusSuggest = function(eventKey, suggestBox, inputParent) {
        let activeItem = $(suggestBox).find('.active');
        if (activeItem.length != 0) {
            if (eventKey == 40 || eventKey == 38) {
                //arrow Up & arrow Down
                let focusItem = activeItem;
                activeItem.removeClass('active');
                switch (eventKey) {
                    case 40 :
                    //arrow Down
                    if (focusItem.next().length > 0) {
                        while (!focusItem.next().hasClass('d-block') && $(focusItem.next()).html()) {
                            focusItem = focusItem.next();

                        }

                        if($(focusItem).next().html())
                        {
                            $(focusItem).next().addClass('active');
                        } else {
                            $(suggestBox).find('.d-block:first').addClass('active');
                        }

                    } else {
                        $(suggestBox).find('.d-block:first').addClass('active');
                    }
                    break;

                    case 38 :
                    //arrow Up
                    if (focusItem.prev().length > 0) {
                        while (!focusItem.prev().hasClass('d-block') && $(focusItem.prev()).html()) {
                            focusItem = focusItem.prev();
                        }
                        if($(focusItem).prev().html())
                        {
                            $(focusItem).prev().addClass('active');
                        } else {
                            $(suggestBox).find('.d-block:last').addClass('active');
                        }
                    } else {
                        $(suggestBox).find('.d-block:last').addClass('active');
                    }
                    break;
                }
            } else if (eventKey == 9) {
                //tab
                InputSuggestion.closeSuggest(suggestBox);
            } else if (eventKey == 13) {
                //enter
                $(inputParent).val($(activeItem).html());
                InputSuggestion.closeSuggest(suggestBox);
            }
        }
    }

    var CloseSuggest = function(suggestBox) {
        $(suggestBox).removeClass('d-block');
    }

    var ElementSuggest = function(searchInput, parentClass) {
        searchInput = searchInput.toUpperCase();

        $(parentClass).each(function() {
            let content = ""
            let allContents = $(this).find('.js-focus-item')
            $(allContents).each( function() {
                content += $(this).html() + " ";
            });
            content = content.toUpperCase();

            if (content.search(searchInput) >= 0) {
                $(this).removeClass('d-none');
            } else {
                $(this).addClass('d-none');
            }
        })
    }

    return {
        renderTimeSuggest : TimeSuggest,
        focusSuggest : FocusSuggest,
        closeSuggest : CloseSuggest,
        elementSuggest : ElementSuggest
    }

})();
/* Create Hidden Suggestion */
$(document).ready(function() {
    $('.js-time-mask').parent().append('<ul class="suggestion-box js-time-list w-90x"></ul>');
    InputSuggestion.renderTimeSuggest('.js-time-list');
})

/* Display Time Suggestion */
$(document).on('keyup','.js-time-mask', function(e) {
    let suggestBox = $(this).parent().find('.js-time-list');
    suggestBox.addClass('d-block');

    let nowInput = this.value.toUpperCase();
    let allSuggest = suggestBox.find('.js-suggestion-item');

    // compare input with each data from loadSuggestion
    $(allSuggest).each(function() {
        let suggestData = $(this).html();
        
        suggestData = suggestData.toUpperCase();

        if (suggestData.replace(/:/g, "").search(nowInput.replace(/(:|_)/g, "")) >= 0) {
            $(this).addClass('d-block');
        } else {
            $(this).removeClass('d-block');
        }
    });

    // trigger focusSuggestion by some keyboard event
    if (e.keyCode === 40 || e.keyCode === 38 || e.keyCode === 13 || e.keyCode === 9) {
        //arrow Down | arrow up | enter | tab
        let eventKey = e.keyCode;
        InputSuggestion.focusSuggest(eventKey, suggestBox, this);
    } else {
        $('.js-suggestion-list li').removeClass('active');
        $(allSuggest).removeClass('active');

        $(suggestBox).find('.d-block:first').addClass('active');
    }
});

/* Display Suggestion */
$(document).on('keyup','.js-suggestion', function(e) {
    let suggestBox = $(this).parent().find('.js-suggestion-list');
    suggestBox.addClass('d-block');

    let nowInput = this.value.toUpperCase();
    let allSuggest = suggestBox.find('.js-suggestion-item');

    // compare input with each data from loadSuggestion
    $(allSuggest).each(function() {
        let suggestData = $(this).html();
        
        suggestData = suggestData.toUpperCase();

        if (suggestData.search(nowInput) >= 0) {
            $(this).addClass('d-block');
        } else {
            $(this).removeClass('d-block');
        }
    });

    // trigger focusSuggestion by some keyboard event
    if (e.keyCode === 40 || e.keyCode === 38 || e.keyCode === 13 || e.keyCode === 9) {
        //arrow Down | arrow up | enter | tab
        let eventKey = e.keyCode;
        InputSuggestion.focusSuggest(eventKey, suggestBox, this);
    } else {
        $('.js-suggestion-list li').removeClass('active');
        $(suggestBox).find('.d-block:first').addClass('active');
    }
});

$(document).on('click', '.js-suggestion-item', function() {
    let selectSuggest = $(this).html();
    let relateInput = $($(this).parents()[1]).find('input');
    relateInput.val(selectSuggest);
    InputSuggestion.closeSuggest($(this).parent());
});

$(document).on('click', function() {
    InputSuggestion.closeSuggest('.js-time-list');
    InputSuggestion.closeSuggest('.js-suggestion-list');
});

/* Mouse Event Re-render Focus Suggestion */
$('.js-suggestion-list li').hover(function() {
    $('.js-suggestion-list li').removeClass('active')
    $(this).addClass('active');
});

$('.js-time-list li').hover(function() {
    $('.js-suggestion-list li').removeClass('active')
    $(this).addClass('active');
});

$(document).on('keypress', '.js-time-mask, .js-suggestion', function(e) {
    if (e.keyCode === 13) {
        //enter
        e.preventDefault();
    }
})