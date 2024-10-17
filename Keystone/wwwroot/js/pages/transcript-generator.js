let disableTriggerArea = '.js-disable-trigger';
let transcriptFaculty = '.js-transcript-faculty';
let transcriptDepartment = '.js-transcript-department';
let transcriptDegree = '.js-transcript-degree';
let transcriptCurriculum = '.js-transcript-curriculum';
let transcriptCurriculumVersion = '.js-cascade-curriculum-version';
let transcriptLanguage = '.js-transcript-language';

function cascadeFacutyAndDepartmentByCurriculumVersionId(curriculumVersionId, language, curriculum, faculty, department, degree) {
    var ajax = new AJAX_Helper(
        {
            url: TranscriptGetFacutyAndDepartmentUrl,
            data: {
                curriculumVersionId: curriculumVersionId,
                language: language
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        curriculum.val(response.curriculum);
        faculty.val(response.faculty);
        department.val(response.department);
        degree.val(response.degree);
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', transcriptCurriculumVersion, function() {
    let currentSection = $(this).closest('section');
    let curriculumVersionId = $(this).val();
    let language = $(transcriptLanguage).val();

    let sectionCurriculum = currentSection.find(transcriptCurriculum);
    let sectionFaculty = currentSection.find(transcriptFaculty);
    let sectionDepartment = currentSection.find(transcriptDepartment);
    let sectionDegree = currentSection.find(transcriptDegree);

    if (sectionFaculty.length > 0 && sectionDepartment.length > 0) {
        cascadeFacutyAndDepartmentByCurriculumVersionId(curriculumVersionId, language, sectionCurriculum, sectionFaculty, sectionDepartment, sectionDegree);
    }
});

$(document).ready( function() {
    let focusElements = $(disableTriggerArea);

    let singleSearch = focusElements.find('.js-single-student');
    let multipleSearch = focusElements.find('.js-multiple-student');
    
    $(singleSearch).on('focusout', function() {

        if ($(this).val() != "") {
            $(multipleSearch).each( function() {
                $(this).parent().addClass('disable-item');
                $(this).prop('disabled', true)
                
                if ($(this).is('select')) {
                    $(this).trigger('chosen:updated')
                }
            })
        } else {            
            $(multipleSearch).each( function() {
                $(this).parent().removeClass('disable-item');
                $(this).prop('disabled', false)
                
                if ($(this).is('select')) {
                    $(this).trigger('chosen:updated')
                }
            })
        }
    })

    $(disableTriggerArea).on('focusout', '.js-multiple-student', function() {

        if ($(this).val() != "") {
            $(singleSearch).parent().addClass('disable-item');
            $(singleSearch).prop('disabled', true)
        } else {            
            $(singleSearch).parent().removeClass('disable-item');
            $(singleSearch).prop('disabled', false)
        }
    })

    let projectClass = '.js-transcript-project-class';
    var i = 0;

    $(projectClass).each(function() {
        i++;
        var newID = 'js-transcript-thesis' + i;
        $(this).attr('id', newID);

        var contractTable = new RowAddAble({
            TargetTable: '#' + newID,
            TableTitle: 'Project Thesis',
            ButtonTitle: 'Thesis'
        });

        contractTable.RenderButton();
    });
})