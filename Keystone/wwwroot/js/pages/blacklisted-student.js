$('#js-student-search').click(function() {
    $('#preloader').fadeIn();
    var ajax = new AJAX_Helper(
        {
            url : BlacklistedStudentInfoUrl,
            data : {
                studentCode: $('#js-student-code').val()
            },
            dataType : 'html',
        }
    );

    ajax.POST().done(function (response) {
        $('#js-form-detail').empty().append(response);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    })
    $('#preloader').fadeOut();
})