var tableId = '#js-refund-report'

function hideColumnChild() {
    $(`${ tableId } thead tr th`).each(function(columnIndex) {
        let allBodyRow = $(tableId).find('tbody tr')
        let allFooterRow = $(tableId).find('tfoot tr')

        if ($(this).hasClass('d-none')) {
            allBodyRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).addClass('d-none');
            })

            allFooterRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).addClass('d-none');
            })
        } else {
            allBodyRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).removeClass('d-none');
            })

            allFooterRow.each(function() {
                let cellRow = $(this).find('td')
                $(cellRow[columnIndex]).removeClass('d-none');
            })
        }
    })
}

function showColumnByFilter(filterType) {
    let typeColumn = $(`.js-${filterType}`)
    let chosenFilter = $(`#${filterType}`)

    if (chosenFilter[0].checked) {
        typeColumn.removeClass('d-none');
    } else {
        typeColumn.addClass('d-none');
    }
    hideColumnChild();
}

function resetTable() {
    $(`${ tableId } thead tr th`).each(function() {
        if (!$(this).hasClass('d-lock')) {
            $(this).addClass('d-none');
        }
    })
}

function columnTotal() {
    var bodyRows = $(tableId).find('tbody tr')
    var footerColumn = $(tableId).find('tfoot tr td')

    $(footerColumn).each(function(columnIndex) {

        if($(this).hasClass('js-col-total')) {
            var currentAmount = 0;

            $(bodyRows).each(function() {
                $(this).find('td').each(function(index) {
                    if (index == columnIndex) {
                        let cellAmount = $(this).html();
                        cellAmount = Number(cellAmount.replace(/[^0-9.-]+/g,""));
                        currentAmount += cellAmount;
                    }
                })
            })

            $(this).html(NumberFormat.renderDecimalTwoDigits(currentAmount));
        }
    })
}

function rowTotal() {
    $('.js-row-total').each(function() {
        var currentAmount = 0;
        let cellsAmount = $(this).parent().find('td.js-two-digits')

        cellsAmount.each(function() {
            if (!$(this).hasClass('d-none')) {
                let cellAmount = $(this).html();
                cellAmount = Number(cellAmount.replace(/[^0-9.-]+/g,""));
                currentAmount += cellAmount;
            }
        })

        $(this).html(NumberFormat.renderDecimalTwoDigits(currentAmount));
    })
}

$(document).ready(function() {
    $('.js-two-digits').each(function() {
        let value = parseInt($(this).html())
        $(this).html(NumberFormat.renderDecimalTwoDigits(value));
    })

    RenderTableStyle.columnAlign();
    RenderTableStyle.footerColumnAlign();
    hideColumnChild();
    columnTotal();
    rowTotal();

    $('#js-invoice-details-modal').on('shown.bs.modal', function (e) {
        let invoiceId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: InvoiceDetailUrl,
                data: {
                    id: invoiceId,
                },
                dataType: 'html',
                contentType: "application/json; charset=utf-8"
            }
        );

        ajax.GET().done(function (response) {
            $('#js-invoice-details-modal .modal-content').html(response);
        })
            .fail(function (jqXHR, textStatus, errorThrown) {
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
    })
});

$(document).on('click', '.js-filter-items', function() {
    showColumnByFilter(this.getAttribute('id'));
    rowTotal();
})