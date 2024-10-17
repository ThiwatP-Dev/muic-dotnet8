$(document).on('dblclick', 'td > p', function() {
    editCell(this);
});

function editCell(cellParagraph) {
    if (cellParagraph != undefined) {
        let cellParagraphIndex = $(cellParagraph).index();
        let parentColumnIndex = $($(cellParagraph).parent('td')).index();
        let parentTable = $(cellParagraph).parents('table')[0];
        var parentTableId = $(parentTable).attr('id');

        let cellData = $(cellParagraph).text();
        let cellName = `${ parentTableId }_col-${ parentColumnIndex }_row-${ cellParagraphIndex }`;
        $(cellParagraph).html(`<input class="w-75 text-right" name="${ cellName }" value="${ cellData }"/>`);
        
        $('.content-changed').removeClass('d-none');
    } else {
        alert("Selected paragraph is invalid!")
    }
    
}