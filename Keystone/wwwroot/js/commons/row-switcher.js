var RowSwitcher = (function() {

    var ReorderRow = function(currentTableBody) { // require tbody in jquery type ex. $('tbody')
        let rows = currentTableBody.find('tr')
        let rowLength = rows.length;
        let rowIndex = 0;

        while (rowIndex < rowLength) {
            let currentRow = rows[rowIndex]
            let allCells = currentRow.cells;
            
            let headerCells = currentTableBody.prev().find('tr th');
            let columnOneHeader = $(headerCells[0]).text().replace(RegularExpressions.AllWhitespace, '');
            if (columnOneHeader === "#") {
                $(allCells[0]).text(rowIndex+1);
            }

            let formFields = $(currentRow).find('select,input,div');
            formFields.each(function() {
                if (this.hasAttribute("name")) {
                    let nameValue = $(this).attr("name").replace(RegularExpressions.AllDigitInBracket, `[${ rowIndex }]`);
                    $(this).attr('name', nameValue);
                }

                if (this.hasAttribute("id")) {
                    let idValue = $(this).attr("id").replace(RegularExpressions.AllDigitInParenthesis, rowIndex);
                    $(this).attr('id', idValue);
                }
            });

            let fieldsLabel = $(currentRow).find('label');
            fieldsLabel.each(function() {
                if (this.hasAttribute("for")) {
                    let currentAttribute = $(this).attr("for")

                    if (currentAttribute.search(/\[/) != -1) {
                        var forValue = currentAttribute.replace(RegularExpressions.AllDigitInBracket, `[${ rowIndex }]`);
                    } else {
                        var forValue = currentAttribute.replace(RegularExpressions.AllDigitInParenthesis, rowIndex);
                    }

                    $(this).attr('for', forValue);
                }
            });

            rowIndex++; //Because row start at index = 0 >> use for ending loop
        };
        RenderTableStyle.columnAlign();
    };
    
    var SwitchRow = function(eventButton) {
        var row = eventButton.parentNode.parentNode,
            sibling = row.previousElementSibling,
            anchor = row.nextElementSibling,
            parent = row.parentNode;

        if ($(eventButton).hasClass('js-move-up')) {
            parent.insertBefore(row, sibling);
        } else if ($(eventButton).hasClass('js-move-down')) {
            anchor.insertAdjacentElement("afterend", row)
        }
    };

    return {
        orderRow : ReorderRow,
        switchRow : SwitchRow,
    };

})();