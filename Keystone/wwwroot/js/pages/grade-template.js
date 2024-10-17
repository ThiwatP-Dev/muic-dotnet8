var gradesTable = '#js-grades'

$(gradesTable).on('click', '.js-move-up, .js-move-down', function () {
    RowSwitcher.switchRow(this);
})

$('#js-add-grade').click(function () {
    let gradeId = $('#js-select-grade').val();
    let tableBody = $(gradesTable).find('tbody');
    let clonedRow = tableBody.find('tr:last').clone(false);
    let oldGrade = $('.js-get-gradeId').val();

    let rowInput = $(clonedRow).find('input');
    let rowIndex = tableBody.find('tr').length;
    $(rowInput).attr("name", function (index, attr) {
        return attr.replace(RegularExpressions.AllDigitInBracket, `[${rowIndex}]`);
    });

    var ajax = new AJAX_Helper({
        url: GetGradeById,
        data: {
            id: gradeId,
        },
        dataType: 'json'
    });

    ajax.GET().done(function (response) {
        if (response !== "") {
            if (oldGrade === "") {
                tableBody.find('tr:last').find('.js-show-grade').html(response.name);
                tableBody.find('tr:last').find('.js-get-gradeId').val(gradeId);
                tableBody.find('tr:last').find('.js-get-gradeName').val(response.name);
            } else {
                clonedRow.find('.js-show-grade').html(response.name);
                clonedRow.find('.js-get-gradeId').val(gradeId);
                clonedRow.find('.js-get-gradeName').val(response.name);
                tableBody.append(clonedRow);
            }
        }
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
})