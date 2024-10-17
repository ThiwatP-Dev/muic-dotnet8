let formArea = '#js-get-form';
let certificateStudentCode = '#StudentCode';
let referenceNumber = '#ReferenceNumber';
let studentFirstName = '#StudentFirstName';
let studentLastName = '#StudentLastName';
let studentTitle = '#TitleId';
let language = '#Language';
let academicTerm = '#AcademicTerm';
let academicYear = '#AcademicYear';
let receiptId = '#ReceiptId';
let previewButton = "#js-preview-button";
let studentCertificateId = "#StudentCertificateId";

function generateCertificateForm(formType, formArea) {
    let ajaxUrl = "";

    switch (formType) {
        case CertificateType.AcademicYearWithGPA:
            ajaxUrl = "/Certificate/AcademicYearWithGPA";
            break;

        case CertificateType.CertifyNewStudent:
            ajaxUrl = "/Certificate/CertifyNewStudent";
            break;

        case CertificateType.ChangingStudentName:
            ajaxUrl = "/Certificate/ChangingStudentName";
            break;

        case CertificateType.ConfirmAcademicYear:
            ajaxUrl = "/Certificate/ConfirmAcademicYear";
            break;

        case CertificateType.GoingAbroadCertification:
            ajaxUrl = "/Certificate/GoingAbroadCertification";
            break;

        case CertificateType.GraduatedEnglishInstruction:
            ajaxUrl = "/Certificate/GraduatedEnglishInstruction";
            break;
            
        case CertificateType.GraduatedNormalForm:
            ajaxUrl = "/Certificate/GraduatedNormalForm";
            break;

        case CertificateType.GraduatedWithAdmissionDateAndGraduatedDate:
            ajaxUrl = "/Certificate/GraduatedWithAdmissionDateAndGraduatedDate";
            break;

        case CertificateType.GraduatedWithCeremonyAt:
            ajaxUrl = "/Certificate/GraduatedWithCeremonyAt";
            break;

        case CertificateType.GraduatedWithEnglishAssessment:
            ajaxUrl = "/Certificate/GraduatedWithEnglishAssessment";
            break;

        case CertificateType.GraduatingStatusCertification:
            ajaxUrl = "/Certificate/GraduatingStatusCertification";
            break;

        case CertificateType.GraduatingWaitingFinalResult:
            ajaxUrl = "/Certificate/GraduatingWaitingFinalResult";
            break;

        case CertificateType.GraduatingWaitingGrade:
            ajaxUrl = "/Certificate/GraduatingWaitingGrade";
            break;

        case CertificateType.GraduationRankingCertification:
            ajaxUrl = "/Certificate/GraduationRankingCertification";
            break;

        case CertificateType.GraduationWithIELTS:
            ajaxUrl = "/Certificate/GraduationWithIELTS";
            break;

        case CertificateType.StatusCertificateWithAssessment:
            ajaxUrl = "/Certificate/StatusCertificateWithAssessment";
            break;

        case CertificateType.StudentDraftDeferment:
            ajaxUrl = "/Certificate/StudentDraftDeferment";
            break;
            
        case CertificateType.StudentStatus:
            ajaxUrl = "/Certificate/StudentStatus";
            break;

        case CertificateType.StudentWithAdmissionFromTo:
            ajaxUrl = "/Certificate/StudentWithAdmissionFromTo";
            break;

        case CertificateType.ThaiAirwayCertification:
            ajaxUrl = "/Certificate/ThaiAirwayCertification";
            break;

        case CertificateType.ExpensesOutlineCertificate:
            ajaxUrl = "/Certificate/ExpensesOutlineCertificate";
            break;
            
        case CertificateType.TranslationGraduatedCertificate:
            ajaxUrl = "/Certificate/TranslationGraduatedCertificate";
            break;

        default:
            break;
    }

    var ajax = new AJAX_Helper(
        {
            url: ajaxUrl,
            data: {
                Id: formType,
                StudentCode: $(certificateStudentCode).val(),
                Language: $(language).val(),
                AcademicTerm: $(academicTerm).val(),
                AcademicYear: $(academicYear).val(),
                ReceiptId: $(receiptId).val(),
                StudentCertificateId: $(studentCertificateId).val()
            },
            dataType: 'html'
        }
    );

    ajax.POST().done(function (response) {
        formArea.empty().append(response);
        
        var referenceNumber = $('#ReferenceNumber').val();
        var message = $('#ErrorMessage').val();
        
        if (referenceNumber == "") {
            FlashMessage.header("Danger", message)
            $(previewButton).prop('disabled',true);
        } else {
            $('#flash-message').empty();
            $(previewButton).prop('disabled',false);
        }
        
        $('.chosen-select').chosen();
        DateTimeInput.renderSingleDate($('.js-single-date'));
        DateTimeInput.renderYear($('.js-single-year'));

        if (formType == CertificateType.ExpensesOutlineCertificate) {
            var inputTable = new RowAddAble({
                TargetTable: '#js-receipt-items',
                ButtonTitle: 'Receipt Item'
            });
            inputTable.RenderButton();

            RenderTableStyle.columnAlign();
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$('#js-get-details').on('click', function() {
    let typeId = $('#js-certificate-type').val();

    if (typeId != null) {
        $('#preloader').fadeIn();
        generateCertificateForm(typeId, $(formArea))
        $('#preloader').fadeOut();
    } else {
        Alert.renderAlert("Error", "Please Select Certificate Type.", "error")
    }
})