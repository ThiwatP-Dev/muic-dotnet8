var MultiSelect = (function() {

    // require "input-suggestion.js"
    var RenderMultiSelectWithSearch = function(selectClass, afterSelectCallback, afterDeselectCallback) {
        $(selectClass).multiSelect({
            selectableHeader: "<input type='text' class='ms-search-input js-search-selectable' autocomplete='off'>",
            selectionHeader: "<input type='text' class='ms-search-input js-search-selection' autocomplete='off'>",

            afterSelect: function(values){
                afterSelectCallback(values);
            },
            afterDeselect: function(values){
                afterDeselectCallback(values);
            }
        });

        $('#js-select-all').click(function(){
            $(selectClass).multiSelect('select_all');
            return false;
        });
        
        $('#js-deselect-all').click(function(){
            $(selectClass).multiSelect('deselect_all');
            return false;
        });

        $('.ms-list li span').each(function() {
            $(this).addClass('js-focus-item')
        })

        $('.ms-selectable').on('keyup blur', '.js-search-selectable', function() {
            let searchValue = $(this).val()
            InputSuggestion.elementSuggest(searchValue, '.ms-selectable .ms-list li')
        })
        
        $('.ms-selection').on('keyup blur', '.js-search-selection', function() {
            let searchValue = $(this).val()
            InputSuggestion.elementSuggest(searchValue, '.ms-selection .ms-list li')
        })

        $('#js-all-switch').click(function() {
            let isCheckAll = $(this).hasClass("unchecked")
            
            if (isCheckAll) {
                $('.js-multi-selectlist').multiSelect('select_all');
                $(this).html(`<i class="far fa-square mr-1"></i> Deselect All`)
                $(this).addClass("checked")
                $(this).removeClass("unchecked")
                return false;
            } else {
                $('.js-multi-selectlist').multiSelect('deselect_all');
                $(this).html(`<i class="far fa-check-square mr-1"></i> Select All`)
                $(this).addClass("unchecked")
                $(this).removeClass("checked")
                return false;
            }
        })
    }

    return {
        renderMultiSelectWithSearch : RenderMultiSelectWithSearch
    }

})();