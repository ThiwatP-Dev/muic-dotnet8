var InputConfig = (function() {

    var SetDefaultZero = function() {
        $(document).on('keyup', '.js-default-zero', function() {
            let currentInput = $(this).val();
    
            if (currentInput === "" || currentInput === null) {
                $(this).val(0).select();
            }
        })
    }

    var SetDefaultMin = function() {
        $(document).on('keyup', '.js-default-min', function() {
            let currentInput = $(this).val();
            let minValue = $(this).attr('min');
    
            if (currentInput === "" || currentInput === null) {
                $(this).val(parseInt(minValue)).select();
            }
        })
    }

    var ClickHighlight = function() {
        $(document).on('click', '.js-click-highlight', function() {
            $(this).select();
        })
    }

    return {
        setDefaultZero : SetDefaultZero,
        setDefaultMin : SetDefaultMin,
        clickHighlight : ClickHighlight
    }
    
})();

$(document).ready( function() {
    InputConfig.setDefaultMin();
    InputConfig.setDefaultZero();
    InputConfig.clickHighlight();
})