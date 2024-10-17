function hideColumnChild() {
    $('#js-receipt-info thead tr th').each(function(columnIndex) {
        let allBodyRow = $('#js-receipt-info').find('tbody tr.js-main-row')

        if ($(this).hasClass('d-none')) {
            allBodyRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).addClass('d-none');
            })
        } else {
            allBodyRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).removeClass('d-none');
            })
        }
    })
}

function showColumnByFilter(filterType) {
    let typeColumn = $(`.js-${filterType}`)
    let chosenFilter = $(`#${filterType}`)
    let currentColspan = $('.js-colspan').attr('colspan')

    if (chosenFilter[0].checked) {
        typeColumn.removeClass('d-none');
        currentColspan++;
    } else {
        typeColumn.addClass('d-none');
        currentColspan--;
    }
    hideColumnChild();

    $('.js-colspan').each(function() {
        $(this).attr('colspan', currentColspan)
    })
}

function resetTable() {
    $('#js-receipt-info thead tr th').each(function() {
        if (!$(this).hasClass('d-lock')) {
            $(this).addClass('d-none');
        }
    })
}

$(document).ready(function() {
    hideColumnChild();
});

$(document).on('click', '.js-filter-items', function() {
    showColumnByFilter(this.getAttribute('id'))
})