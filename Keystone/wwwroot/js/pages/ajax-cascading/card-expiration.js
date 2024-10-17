let cardAcademicLevel = '#student-academiclevel-id';
let cardFaculty = '#student-faculty-id';
let cardDepartment = '#student-department-id';

let createdCard = '#js-cascade-created-date';
let expiredCard = '#js-cascade-expired-date';

$(createdCard).on('apply.daterangepicker', function() {

    var ajax = new AJAX_Helper(
        {
            url: CardExpirationOptionUrl,
            data: {
                academicLevelId: $(cardAcademicLevel).val(),
                facultyId: $(cardFaculty).val(),
                departmentId: $(cardDepartment).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        let createdDate = $(createdCard).val();
        let yearEnd = parseInt(createdDate.substr(6, 4)) + response;
        let expiredDate = createdDate.substr(0,6) + yearEnd;
        $(expiredCard).val(expiredDate);
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})