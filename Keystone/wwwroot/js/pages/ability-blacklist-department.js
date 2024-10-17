$(function () {
    $('#ability-blacklist-modal-create').on('shown.bs.modal', function (event) {
        $('.chosen-select').chosen();
    })

    $('#ability-blacklist-modal-edit').on('shown.bs.modal', function (event) {
        var button = event.relatedTarget;
        var specializationGroupBlacklistId = $(button).data('specialization-group-blacklist-id');

        if (specializationGroupBlacklistId != 0) {
            var ajax = new AJAX_Helper(
                {
                    url: GetAbilityBlacklistDepartmentUrl,
                    data: {
                        specializationGroupBlackListId: specializationGroupBlacklistId
                    },
                    dataType: 'html',
                    contentType: "application/json; charset=utf-8"
                }
            );

            ajax.GET().done(function (response) {
                $('#modalWrapper-ability-blacklist-edit').empty().append(response);
                $('.chosen-select').chosen();
            })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    ErrorCallback(jqXHR, textStatus, errorThrown);
                });
        }
    })
})