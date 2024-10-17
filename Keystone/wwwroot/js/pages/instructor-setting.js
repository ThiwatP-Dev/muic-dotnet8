var universityLevelTable = '#js-roles-university-level'
var majorLevelTable = '#js-roles-major-level'

$('#js-add-role-university-level').click(function () {
    let roleId = $('#js-select-role-university-level').val();
    let tableBody = $(universityLevelTable).find('tbody');
    let clonedRow = tableBody.find('tr:last').clone(false);
    let oldRole = $('.js-get-roleId-university-level').val();

    let rowInput = $(clonedRow).find('input');
    let rowIndex = tableBody.find('tr').length;
    $(rowInput).attr("name", function (index, attr) {
        return attr.replace(RegularExpressions.AllDigitInBracket, `[${rowIndex}]`);
    });

    var ajax = new AJAX_Helper({
        url: GetRoleById,
        data: {
            roleId: roleId,
        },
        dataType: 'json'
    });

    ajax.GET().done(function (response) {
        if (response !== "") {
            if (oldRole === "") {
                tableBody.find('tr:last').find('.js-show-role-university-level').html(response.name);
                tableBody.find('tr:last').find('.js-get-roleId-university-level').val(roleId);
                tableBody.find('tr:last').find('.js-get-roleName-university-level').val(response.name);
            } else {
                clonedRow.find('.js-show-role-university-level').html(response.name);
                clonedRow.find('.js-get-roleId-university-level').val(roleId);
                clonedRow.find('.js-get-roleName-university-level').val(response.name);
                tableBody.append(clonedRow);
            }
        } 
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, jqXHR.responseText, errorThrown);
        });
})

$('#js-add-role-major-level').click(function () {
    let roleId = $('#js-select-role-major-level').val();
    let tableBody = $(majorLevelTable).find('tbody');
    let clonedRow = tableBody.find('tr:last').clone(false);
    let oldRole = $('.js-get-roleId-major-level').val();

    let rowInput = $(clonedRow).find('input');
    let rowIndex = tableBody.find('tr').length;
    $(rowInput).attr("name", function (index, attr) {
        return attr.replace(RegularExpressions.AllDigitInBracket, `[${rowIndex}]`);
    });

    var ajax = new AJAX_Helper({
        url: GetRoleById,
        data: {
            roleId: roleId,
        },
        dataType: 'json'
    });

    ajax.GET().done(function (response) {
        if (response !== "") {
            if (oldRole === "") {
                tableBody.find('tr:last').find('.js-show-role-major-level').html(response.name);
                tableBody.find('tr:last').find('.js-get-roleId-major-level').val(roleId);
                tableBody.find('tr:last').find('.js-get-roleName-major-level').val(response.name);
            } else {
                clonedRow.find('.js-show-role-major-level').html(response.name);
                clonedRow.find('.js-get-roleId-major-level').val(roleId);
                clonedRow.find('.js-get-roleName-major-level').val(response.name);
                tableBody.append(clonedRow);
            }
        } 
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, jqXHR.responseText, errorThrown);
        });
})