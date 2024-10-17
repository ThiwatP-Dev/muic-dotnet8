function getScholarshipBudgetDetails(scholarshipId) {

    var ajax = new AJAX_Helper(
        {
            url: ScholarshipBudgetTableUrl,
            data: {
                id: scholarshipId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $('#js-budget-details').empty().append(response);
        RenderTableStyle.columnAlign();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).ready(function() {
    $('#js-clone-budget-modal').one('shown.bs.modal', function() {
        $('.chosen-select').chosen();

        $('#js-scholarship').on('change', function() {
            getScholarshipBudgetDetails($(this).val());
        });
    });
});