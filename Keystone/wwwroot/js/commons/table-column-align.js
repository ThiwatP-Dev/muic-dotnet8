var RenderTableStyle = (function() {

    var ColumnAlign = function() {
        $('table').each(function() {
            let allHeader = $(this).find('th')
            let allBodyRow = $(this).find('tbody tr')
            
            allHeader.each(function(columnIndex) {
        
                if ($(this).hasClass('text-center')) {
    
                    allBodyRow.each(function() {
    
                        let rowCells = $(this).find('td')
                        $(rowCells[columnIndex]).addClass('text-center');
    
                        let cellInput = $(rowCells[columnIndex]).find('input')
                        $(cellInput).addClass('text-center');
                    })
                    
                } else if ($(this).hasClass('text-right')) {
                    allBodyRow.each(function() {
                        let rowCells = $(this).find('td')
                        $(rowCells[columnIndex]).addClass('text-right');
    
                        let cellInput = $(rowCells[columnIndex]).find('input')
                        $(cellInput).addClass('text-right');
                    })
                }
            }); 
        })
    }

    var FooterColumnAlign = function() {
        $('table').each(function() {
            let allHeader = $(this).find('th')
            let allFooterRow = $(this).find('tfoot tr')
            
            allHeader.each(function(columnIndex) {
        
                if($(this).hasClass('text-center')) {
    
                    allFooterRow.each(function() {
    
                        let rowCells = $(this).find('td')
                        $(rowCells[columnIndex]).addClass('text-center');
    
                        let cellInput = $(rowCells[columnIndex]).find('input')
                        $(cellInput).addClass('text-center');
                    })
                    
                } else if($(this).hasClass('text-right')) {
                    allFooterRow.each(function() {
                        let rowCells = $(this).find('td')
                        $(rowCells[columnIndex]).addClass('text-right');
    
                        let cellInput = $(rowCells[columnIndex]).find('input')
                        $(cellInput).addClass('text-right');
                    })
                }
            }); 
        })
    }

    return {
        columnAlign : ColumnAlign,
        footerColumnAlign : FooterColumnAlign
    };

})();

$(document).ready(function() {
    RenderTableStyle.columnAlign();
});