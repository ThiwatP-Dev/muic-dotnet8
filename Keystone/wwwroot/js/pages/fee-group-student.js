$('#js-student-filter').on('click', function() {

    var ajax = new AJAX_Helper(
        {
            url: AddStudentFeeGroupUrl,
            data : {
                Nationalities : $('#Nationalities').val(),
                AcademicLevelId : $('#AcademicLevelId').val(),
                AdmissionTypeId : $('#AdmissionTypeId').val(),
                Faculties : $('#Faculties').val(),
                Departments : $('#Departments').val(),
                Curriculums : $('#Curriculums').val(),
                BatchStart : $('#BatchStart').val(),
                BatchEnd : $('#BatchEnd').val(),
                StudentCodeStart : $('#StudentCodeStart').val(),
                StudentCodeEnd : $('#StudentCodeEnd').val(),
                AdmissionYearStart : $('#AdmissionYearStart').val(),
                AdmissionYearEnd : $('#AdmissionYearEnd').val(),
                AdmissionTermStart : $('#AdmissionTermStart').val(),
                AdmissionTermEnd : $('#AdmissionTermEnd').val()
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) { 
        $('#preloader').fadeIn();
        $('.js-items').empty();
        let count = 0;
        
        $(response).each( function() {
            $('.js-items').append(
                `<div class="js-items-parent">
                    <input id="StudentIds[${ count }]" name="StudentIds[${ count }]" class="form-check-input" type="checkbox" value="${ this.id }"/>
                    <label class="js-focus-item m-0" for="StudentIds[${ count }]">${ this.codeAndName }</label>
                    <hr class="w-100x">
                </div>`
            )
            count ++;
        })

        CheckList.renderCheckbox(".js-checklist-course");
        $('#preloader').fadeOut();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        $('#preloader').fadeIn();
        ErrorCallback(jqXHR, textStatus, errorThrown);
        $('#preloader').fadeOut();
    });
})