var dataModel = $('#js-pdf-preview').data('model');
var universityNameEn = $('body').data('uname-en');
var universityNameTh = $('body').data('uname-th');

function toChars (text) {
    return text ? text.split('') : [''];
};

function charsRenderer(pdf, characters, xPos, yPos) {
    let boxWidth = 0;

    characters.forEach(char => {
        pdf.text(char, xPos + boxWidth, yPos, PdfSettings.style(textStyles.ReportLeft));
        boxWidth += 5.5;
    });
};

function generateApplicationData(data) {
    var application = new jsPDF('p', 'mm', 'a4');
    //PdfSettings.property(application, data);

    const leftStart = 20;
    const rightStart = 95;
    
    PdfSettings.format(application, textFormats.sansBoldS);

    // Personal section
    let currentY = 69;

    let birthData = data.BirthDateText.split('/');
    charsRenderer(application, toChars(birthData[0]), 113.5, currentY);
    charsRenderer(application, toChars(birthData[1]), 126.5, currentY);
    charsRenderer(application, toChars(birthData[2]), 139.5, currentY);
    
    application.text(data.FirstNameTh, leftStart + 4, currentY += 5)
               .text(data.LastNameTh, rightStart + 9, currentY);

    charsRenderer(application, toChars(data.FirstNameEn), leftStart, currentY += 7);
    charsRenderer(application, toChars(data.LastNameEn), rightStart, currentY);

    charsRenderer(application, toChars(data.Nationality), leftStart, currentY += 10.5);
    charsRenderer(application, toChars(data.Religion), rightStart, currentY);

    charsRenderer(application, toChars(data.BirthProvince), leftStart, currentY += 10);
    let idNumber = data.CitizenNumber == null ? data.Passport : data.CitizenNumber
    charsRenderer(application, toChars(idNumber), rightStart, currentY);

    charsRenderer(application, toChars(data.TelephoneNumber), leftStart, currentY += 9.5);
    application.text(data.PersonalEmail, rightStart, currentY);

    // Educational section
    charsRenderer(application, toChars(data.PreviousSchoolName), leftStart, currentY += 16);
    charsRenderer(application, toChars(data.PreviousSchoolCountry), leftStart, currentY += 10);

    // Address section
    application.text(data.HouseNumber, leftStart + 9, currentY += 16);
    application.text(data.Moo, 71, currentY);
    application.text(data.AddressTh, rightStart + 16, currentY);
    charsRenderer(application, toChars(data.HouseNumber), leftStart, currentY += 6);
    charsRenderer(application, toChars(data.Moo), 66, currentY);
    charsRenderer(application, toChars(data.AddressEn), rightStart, currentY);

    application.text(data.SoiTh, leftStart + 5, currentY += 8.5);
    application.text(data.RoadTh, rightStart + 5, currentY);
    charsRenderer(application, toChars(data.SoiEn), leftStart, currentY += 6);
    charsRenderer(application, toChars(data.RoadEn), rightStart, currentY);

    application.text(data.SubdistrictTh, leftStart + 13, currentY += 8.5);
    application.text(data.DistrictTh, rightStart + 11, currentY);
    charsRenderer(application, toChars(data.SubdistrictEn), leftStart, currentY += 6);
    charsRenderer(application, toChars(data.DistrictEn), rightStart, currentY);

    application.text(data.ProvinceTh, leftStart + 8, currentY += 8.5);
    application.text(data.ZipCode, rightStart + 14, currentY);
    application.text(data.AddressTelephoneNumber, rightStart + 39, currentY);
    charsRenderer(application, toChars(data.ProvinceEn), leftStart, currentY += 6);
    charsRenderer(application, toChars(data.ZipCode), rightStart, currentY);
    charsRenderer(application, toChars(data.AddressTelephoneNumber), rightStart + 30, currentY);

    // Parent section
    currentY += 16.5

    charsRenderer(application, toChars(data.FatherName), 70, currentY);
    charsRenderer(application, toChars(data.FatherTelephoneNumber), leftStart, currentY += 9);
    charsRenderer(application, toChars(data.FatherCitizenNumber), rightStart, currentY);
    currentY += 11;
    charsRenderer(application, toChars(data.MotherName), 70, currentY);
    charsRenderer(application, toChars(data.MotherTelephoneNumber), leftStart, currentY += 9);
    charsRenderer(application, toChars(data.MotherCitizenNumber), rightStart, currentY);
    currentY += 11;
    charsRenderer(application, toChars(data.GuardianName), 70, currentY);
    charsRenderer(application, toChars(data.GuardianTelephoneNumber), leftStart, currentY += 9);
    charsRenderer(application, toChars(data.GuardianCitizenNumber), rightStart, currentY);
    currentY += 11;
 

    // get preview
    $('#js-print-preview').attr('src', application.output('datauristring'));
};

$( function() {
    generateApplicationData(dataModel);
})