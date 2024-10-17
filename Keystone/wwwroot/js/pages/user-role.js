var userRoleTable = '#js-roles'

$('#js-add-role').click(function () {
    let userId = $('#UserId').val();
    let roleId = $('#js-select-role').val();
    let tableBody = $(userRoleTable).find('tbody');
    let clonedRow = tableBody.find('tr:last').clone(false);
    let oldRole = $('.js-get-roleId').val();

    let rowInput = $(clonedRow).find('input');
    let rowIndex = tableBody.find('tr').length;
    $(rowInput).attr("name", function (index, attr) {
        return attr.replace(RegularExpressions.AllDigitInBracket, `[${rowIndex}]`);
    });

    var ajax = new AJAX_Helper({
        url: GetUserRoleById,
        data: {
            userId: userId,
            roleId: roleId,
        },
        dataType: 'json'
    });

    ajax.GET().done(function (response) {
        if (response !== "") {
            if (oldRole === "") {
                tableBody.find('tr:last').find('.js-show-role').html(response.name);
                tableBody.find('tr:last').find('.js-get-roleId').val(roleId);
                tableBody.find('tr:last').find('.js-get-roleName').val(response.name);
            } else {
                clonedRow.find('.js-show-role').html(response.name);
                clonedRow.find('.js-get-roleId').val(roleId);
                clonedRow.find('.js-get-roleName').val(response.name);
                tableBody.append(clonedRow);
            }
        } 
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, jqXHR.responseText, errorThrown);
        });
})