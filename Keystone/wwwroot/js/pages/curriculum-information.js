let specializationStudent = '.js-cascade-student';
let specializationCurriculum = '.js-cascade-specialization-curriculum';
let specializationCurriculumVersion = '.js-cascade-specialization-curriculum-version';
let specializationGroup = '.js-cascade-specialization-group';

function specializationInformation() {
    var inputTable = new RowAddAble({
        TargetTable: `#js-specialization-group`,
        TableTitle: 'Specialization Information',
        ButtonTitle: 'Specialization'
    })
    inputTable.RenderButton();
};

function cascadesSpecializationCurriculumVersion(studentId, curriculumId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumVersionsByCurriculumIdAndStudentId,
            data: {
                studentId: studentId,
                curriculumId: curriculumId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });
        console.log(response);

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadesSpecializationGroup(curriculumVersionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSpecializationGroupByCurriculumVersionId,
            data: {
                curriculumVersionId: curriculumVersionId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', specializationCurriculum, function() {
    let currentSection = $(this).closest('section');
    let curriculumId = $(this).val();
    let studentId = $(specializationStudent).val();

    //require check fields & target
    let sectionSpecialization = currentSection.find(specializationCurriculumVersion);

    if (sectionSpecialization.length > 0) {
        cascadesSpecializationCurriculumVersion(studentId, curriculumId, sectionSpecialization);
    } 
});

$(document).on('change', specializationCurriculumVersion, function() {
    let currentSection = $(this).closest('section');
    let curriculumVersionId = $(this).val();

    //require check fields & target
    let sectionSpecialization = currentSection.find(specializationGroup);

    if (sectionSpecialization.length > 0) {
        cascadesSpecializationGroup(curriculumVersionId, sectionSpecialization);
    } 
});

$( function() {
    specializationInformation();
})