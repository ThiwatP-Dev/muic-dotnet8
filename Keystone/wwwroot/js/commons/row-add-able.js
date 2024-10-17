var RowAddAble = (function(config) {
    
    function renderButton() {
        var tableId = config.TargetTable;
        var tableTitle = config.TableTitle;
        var buttonTitle = config.ButtonTitle;

        if (tableTitle === undefined) {
            tableTitle = "";
        }

        $(tableId).parent().before(
            `<div class="d-flex form-group justify-content-between m-b-15 p-r-15">
                <span class="w-100x section-title align-self-center">
                    <h4 class="mb-0">${ tableTitle }</h4>
                </span>
                <span class="w-100x text-right">
                    <button class="btn btn--primary mb-0 js-export-excel d-none" type="submit" name="type" value="export">
                        <i class="la la-download info mr-2 font-weight-800"></i>Export
                    </button>
                    <button class="btn btn--primary mb-0 js-add-row" type="button" data-target="${ tableId }">
                        <i class="la la-plus mr-2 font-weight-800"></i>Add ${ buttonTitle }
                    </button>
                </span>
            </div>`
        );
    }

    return {
        RenderButton : renderButton,
    };
})

var RenderTable = (function() {

    var AddRow = function(targetTableId) {
        let clonedRow = $(targetTableId).find('tbody > tr:last').clone(true);
        clonedRow.appendTo(targetTableId);
        var rowIndex = ($(targetTableId).find('tbody > tr:last'))[0].rowIndex;
        
        // chosen select Re-render
        let allColumn = $(targetTableId).find('tbody > tr:last > td');
        allColumn.each(function() {
            let isSelect = $(this).find('select');
            if (isSelect.length) {
                if (isSelect.hasClass("js-default-select")) {
                    let defaultValue = $(isSelect).val();
                    $(this).empty();
                    $(this).append(isSelect);
                    $(isSelect).chosen({search_contains: true});
                    $(this).find('select').val(defaultValue).trigger('chosen:updated');
                } else if (isSelect.hasClass("chosen-select")) {
                    $(this).empty();
                    $(this).append(isSelect);
                    $(isSelect).prop("disabled", false);
                    $(isSelect).chosen({search_contains: true});
                    $(this).find('select').val('Select').trigger('chosen:updated');
                } else if (isSelect.hasClass("editable-chosen-select")) {
                    $(this).empty();
                    $(this).append(`<span class="js-edit-value"></span>`);
                    $(this).append(isSelect);
                    $(isSelect).chosen({search_contains: true});
                    $(this).find('select').val('Select').trigger('chosen:updated');
                }
            } else {
                let isInput = $(this).find('input');
                let cellData = $(this).html();
                if (isInput.length){
                    $(this).empty();
                    $(this).append(cellData);
                    $(this).find('input').val('');
                }
            }
        }); 

        OrderRow(targetTableId, rowIndex)
        DateTimeInput.renderSingleDate($('.js-single-date'));
        InputMask.renderTimeMask();
    }

    var DeleteRow = function(targetTableId, rowIndex) {
        let currentBodyRows = $(targetTableId).find('tbody tr');
        let isCheckbox = $(currentBodyRows[rowIndex-1]).find('input[type="checkbox"]');
        if (isCheckbox.length > 0) {
            isCheckbox.each( function() {
                let currentName = $(this).attr('name');
                $('body').find(`input[name="${ currentName }"]`).remove();
            })
        }

        $(targetTableId)[0].deleteRow(rowIndex);
        OrderRow(targetTableId, rowIndex);
    }

    var OrderRow = function(targetTableId, rowIndex) {
        // Rearrange data row to make variables value correlation with table
        var newLength = $(targetTableId)[0].rows.length;
        while (rowIndex < newLength) {
            let focusRow = $(targetTableId)[0].rows[rowIndex]
            
            let firstColumn = $(targetTableId).find('thead tr th');
            let allCells = focusRow.cells;
            let columnOneHeader = $($(firstColumn)[0]).text()
            columnOneHeader = columnOneHeader.replace(RegularExpressions.AllWhitespace, '');
            if (columnOneHeader == "#" && !$(allCells[0]).hasClass('js-no-index')) {
                $(allCells[0]).text(rowIndex);
            }

            let allColumn = $(focusRow).find('select,input,div');
            allColumn.each(function() {
                if (this.hasAttribute("name")) {
                    if ($(this).hasClass('js-multiple-index')) {
                        let nameValue = $(this).attr("name").replace(RegularExpressions.LastDigitInBracket, `[${ rowIndex-1 }]`);
                        $(this).attr('name', nameValue);
                    } else {
                        let nameValue = $(this).attr("name").replace(RegularExpressions.AllDigitInBracket, `[${ rowIndex-1 }]`);
                        $(this).attr('name', nameValue);
                    }
                }

                if (this.hasAttribute("id")) {
                    let idValue = $(this).attr("id").replace(RegularExpressions.AllDigitInParenthesis, rowIndex-1);
                    $(this).attr('id', idValue);
                }
            });

            let inputLabel = $(focusRow).find('label');
            inputLabel.each(function() {
                if (this.hasAttribute("for")) {
                    let currentAttribute = $(this).attr("for")

                    if (currentAttribute.search(/\[/) != -1) {
                        var forValue = currentAttribute.replace(RegularExpressions.AllDigitInBracket, `[${ rowIndex-1 }]`);
                    } else {
                        var forValue = currentAttribute.replace(RegularExpressions.AllDigitInParenthesis, rowIndex-1);
                    }

                    $(this).attr('for', forValue);
                }
            });

            rowIndex++; //Because row start at index = 0 >> use for ending loop
        };
        RenderTableStyle.columnAlign();
    }

    return {
        addRow : AddRow,
        deleteRow : DeleteRow,
        orderRow : OrderRow
    };

})();

// delegate to always run this event function >> see old definition of .live, .delegate, .bind
$(document).on('click', '.js-add-row', function() {
    let tableId = $(this).data('target');
    RenderTable.addRow(tableId);

    if( $(tableId).hasClass('js-editable-table') ) {  
        var nextElementOfSelect = $(tableId).find('tbody > tr:last').find('select').next();

        if(nextElementOfSelect != undefined && nextElementOfSelect.length != 0) {
            $(nextElementOfSelect).addClass('d-none');
        }
    }
});

$(document).on('click', '.js-del-li', function() {
    var ulId = $(this).parents('ul').attr('id');
    var eventRow = $(this).closest('li');
    // tr[0].sectionRowIndex = start at 0 | tr[0].rowIndex = start at 1
    $('.js-delete-li-btn').data('id', `#${ ulId }|${ eventRow.index() }`); // Send table id from delete row button to delete confirm modal
    
    let btnType = $(this).data('type');
    if (btnType != "") {
        $('.js-delete-li-btn').data('type', btnType);
    }
});

$(document).on('click', '.js-del-row', function() {
    var tableId = $(this).parents('table').attr('id');
    var eventRow = $(this).closest('tr');
    // tr[0].sectionRowIndex = start at 0 | tr[0].rowIndex = start at 1
    $('.js-delete-row-btn').data('id', `#${ tableId }|${ eventRow[0].rowIndex }`); // Send table id from delete row button to delete confirm modal
    
    let btnType = $(this).data('type');
    if (btnType != "") {
        $('.js-delete-row-btn').data('type', btnType);
    }
});

$(document).on('click', '.js-disable-row', function() {
    var tableId = $(this).parents('table').attr('id');
    var eventRow = $(this).closest('tr');
    // tr[0].sectionRowIndex = start at 0 | tr[0].rowIndex = start at 1
    $('.js-delete-row-btn').data('id', `#${ tableId }|${ eventRow[0].rowIndex }|disable`); // Send table id from delete row button to delete confirm modal
});

$(document).on('click', '.js-delete-li-btn', function() {
    $('#delete-li-confirm-modal').modal('hide');
    var ulRow = $(this).data('id').split('|');
    var ulId = ulRow[0];
    var rowIndex = ulRow[1];
    var currentRow = $(ulId).children()[rowIndex];
    currentRow.remove();
    reorderIndex($(ulId));
})

$(document).on('click', '.js-del-row-now', function() {
    var tableId = $(this).parents('table').attr('id');
    var eventRow = $(this).closest('tr');
    var rowIndex = eventRow[0].rowIndex;
    RenderTable.deleteRow('#' + tableId, rowIndex);
})

$(document).on('click', '.js-delete-row-btn', function() {
    $('#delete-row-confirm-modal').modal('hide')

    // Get data group from delete confirm button in modal then split data apart to specify table and row to delete
    var tableRow = $(this).data('id').split('|');
    var tableId = tableRow[0];
    var rowIndex = tableRow[1];
    var currentRow = $(tableId).find('tbody tr');
    
    // check is disable or delete
    if (tableRow.length > 2) {
        $('#disable-confirm-modal').modal('hide')
        let allRows = $(tableId).find('tr');
        $(allRows[rowIndex]).addClass('bg-danger-pastel');
        $(allRows[rowIndex]).find('input').prop('disabled', true).addClass('disable-item');
        let actionColumn = $(allRows[rowIndex]).find('td:last')
        actionColumn.find('.js-refresh-row').removeClass('d-none');
        actionColumn.find('.js-disable-row').addClass('d-none');
    } else if (currentRow.length === 1) {
        let btnType = $(this).data('type');

        if (btnType === "delete-able") {
            RenderTable.deleteRow(tableId, rowIndex);
        } else {
            currentRow.each(function() {
                let formInput = $(this).find('input, select');
                $(formInput).val('');
                if (formInput.hasClass('chosen-select')) {
                    $(formInput).val(' ').trigger('chosen:updated');
                }
                
                if (formInput.hasClass('js-set-default-zero')) {
                    $(this).find('.js-set-default-zero').val(0);
                }

                $(this).find('span:first').text('');
            });
        }
    } else {
        RenderTable.deleteRow(tableId, rowIndex);
    }
})

$(document).on('click','.js-refresh-row', function() {
    let tableRow = $(this).data('id').split('|');
    let tableId = tableRow[0];
    let rowIndex = tableRow[1].slice(-1);
    
    let allRows = $(tableId).find('tr');
    $(allRows[rowIndex]).removeClass('bg-danger-pastel');
    $(allRows[rowIndex]).find('input').prop('disabled',false).removeClass('disable-item');
    let actionColumn = $(allRows[rowIndex]).find('td:last')
    actionColumn.find('.js-refresh-row').addClass('d-none');
    actionColumn.find('.js-disable-row').removeClass('d-none');
})