var dataModel = $('#id-card-preview').data('model');

var page = {
    header : ["PERSONAL INFORMATION", "EDUCATION INFORMATION", "ADDRESS DETAIL", "PARENT INFORMATION",
              "APPLICATION", "SUMMITTED DOCUMENT"],
    submitCheck : "O YES    O NO    ________________________",
    p1 : ["STUDENT ID :", "NAME :", "GENDER :", "NATIONALITY :", "RELIGION :", "BIRTH DAY :",
          "BIRTH COUNTRY :", "BIRTH PROVINCE :", "AGE :", "CITIZEN/PASSPORT :", "MOBILE PHONE :", "E-MAIL :"],
    p2 : ["BACHELOR'S DEGREE :", "MASTER'S DEGREE :", "GRADUATED YEAR :", "TOEFL IBT :", "TOEFL PBL :", "IELTS :"],
    p3 : ["ADDRESS :", "HOUSE NUMBER :", "MOO :", "SOI :", "STREET :", "SUB-DISTRICT :", "DISTRICT :", "PROVINCE :", "POSTAL CODE :"],
    p4 : ["NAME :", "TELEPHONE :", "EMAIL :", "RELATIONSHIP :"],
    p5 : ["ACADEMIC YEAR :", "PROGRAM :", "CAMPUS :", "ACADEMIC LEVEL :", "PROGRAM OF STUDY :"],
    p6 : ["BACHELOR'S DEGREE CERTIFICATE / DIPLOMA CERTIFICATE (2 COPIES)", "MASTER'S DEGREE CERTIFICATE (2 COPIES) - PHD APPLICANTS ONLY",
          "OFFICIAL TRANSCRIPT (2 COPIES) - BACHELORS", "OFFICIAL TRANSCRIPT (2 COPIES) - MASTERS - PHD APPLICANTS ONLY",
          "PASSPORT COPY (INTL.) / CITIZEN ID(THAI) (2 COPIES)", "HOUSE REGISTRATION (2 COPIES) - FOR THAI APPLICANTS ONLY",
          "4 PHOTOGRAPHS IN FORMAL ATTIRE (1 X 1 INCH)", "ENGLISH PROFICIENCY SCORE: IELTS______   TOEFL______", "NON-CRIMINAL RECORD CERTIFICATE",
          "NAME CHANGE CERTIFICATE - IF ANY", "POLICE CLEARANCE REPORT - (NON THAI STUDENT)",],
    p7 : ["I CERTIFY THAT THE INFORMATION AND DOCUMENTS I HAVE SUBMITTED ALONG WITH THIS APPLICATION IS",
          "COMPLETE, UP TO DATE AND ACCURATE. I AGREE TO ALLOW THE UNIVERSITY TO INQUIRE INFORMATION ON",
          "THE STATUS OF MY APPLICATION AND SUBMITTED DOCUMENTS FROM MY PREVIOUS INSTITUTION ATTENDED.",
          "I HEREBY ACKNOWLEDGE THAT MAHIDOL UNIVERSITY HAS THE RIGHT TO TERMINATE MY APPLICATION IF",
          "ANY OF THE ABOVE MENTIONED DOCUMENTS ARE PROVEN TO BE INCOMPLETE, INVALID, AND INACCURATE.",
          "AS I AM STUDENT OF MAHIDOL UNIVERSITY, I MUST FOLLOW THE REGISTRATION AND TUITION FEES OF",
          "THE PROGRAM I HAVE CHOSEN THROUGH MY OVER ALL DEGREE."],
    p8 : ["(____________________)", "ADMISSION OFFICER", "DATE__________________", "APPLICANT'S SIGNATURE"]
};

function masterApplication(dataModel) {
    var card = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(card, dataModel);
    let center = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX,
        paddingX2 = defaultSetting.paddingX + 70,
        paddingX3 = defaultSetting.paddingX + 140,
        currentY = 45;
        headerLine = 7;
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;

    var informationConfig = {
        align: 'left',
        lineHeightFactor: 5,
        maxWidth: defaultSetting.A4MaxWidth - paddingX
    }

    var sizes = [12,9]
    let logoSize = 35;
    card.addImage(PdfSettings.img($('body').data('logo-url')), 'png', center - (logoSize / 2), 5, logoSize, logoSize);
    card.addImage($('#js-default-student')[0], 'png', 155, 5, 35, 35)

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[0], 10, currentY, 'left')

    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(`${ page.p1[0] } ${ dataModel.Code }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p1[1] } ${ dataModel.FullName.toUpperCase() }`, paddingX2, currentY, 'left')
        .text(`${ page.p1[2] } ${ dataModel.Gender.toUpperCase() }`, paddingX3, currentY, 'left')
        .text(`${ page.p1[3] } ${ dataModel.Nationality.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p1[4] } ${ dataModel.Religion.toUpperCase() }`, paddingX2, currentY, 'left')
        .text(`${ page.p1[9] } ${ dataModel.CitizenNumber }`, paddingX3, currentY, 'left')
        .text(`${ page.p1[5] } ${ dataModel.Birthday }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p1[6] } ${ dataModel.BirthCountry.toUpperCase() }`, paddingX2, currentY, 'left')
        .text(`${ page.p1[8] } ${ dataModel.Age }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p1[7] } ${ dataModel.BirthProvince }`, paddingX2, currentY, 'left')
        .text(`${ page.p1[10] } ${ dataModel.TelephoneNumber }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p1[11] } ${ dataModel.Email.toUpperCase() }`, paddingX2, currentY, 'left')

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[1], paddingX, currentY += headerLine, 'left')

    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(`${ page.p2[0] } ${ dataModel.PreviousBachelorSchool.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p2[2] } ${ dataModel.BachelorGraduatedYear }`, paddingX3, currentY, 'left')
        .text(`${ page.p2[1] } ${ dataModel.PreviousMasterSchool.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p2[2] } ${ dataModel.MasterGraduatedYear }`, paddingX3, currentY, 'left')
        .text(`${ page.p2[3] } ${ dataModel.TOEFLIBT }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p2[4] } ${ dataModel.TOEFLPBL }`, paddingX2, currentY, 'left')
        .text(`${ page.p2[5] } ${ dataModel.IELTS }`, paddingX3, currentY, 'left')

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[2], paddingX, currentY += headerLine, 'left')

    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(`${ page.p3[0] } ${ dataModel.Address.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p3[1] } ${ dataModel.HouseNumber }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p3[2] } ${ dataModel.Moo }`, paddingX2, currentY, 'left')
        .text(`${ page.p3[3] } ${ dataModel.Soi.toUpperCase() }`, paddingX3, currentY, 'left')
        .text(`${ page.p3[4] } ${ dataModel.Road.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p3[5] } ${ dataModel.SubDistrictName.toUpperCase() }`, paddingX2, currentY, 'left')
        .text(`${ page.p3[6] } ${ dataModel.DistrictName.toUpperCase() }`, paddingX3, currentY, 'left')
        .text(`${ page.p3[7] } ${ dataModel.ProvinceName.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p3[8] } ${ dataModel.ZipCode }`, paddingX2, currentY, 'left')

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[3], paddingX, currentY += headerLine, 'left')

    $(dataModel.EmergencyDetails).each( function() {
        card.setFont('helvetica', 'normal')
            .setFontSize(sizes[1])
            .text(`${ page.p4[0] } ${ this.Name.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
            .text(`${ page.p4[1] } ${ this.PhoneNumber }`, center, currentY, 'left')
            .text(`${ page.p4[2] } ${ this.Email.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
            .text(`${ page.p4[3] } ${ this.Relationship.toUpperCase() }`, center, currentY, 'left')
    })
        
    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[4], paddingX, currentY += headerLine, 'left')

    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(`${ page.p5[0] } ${ dataModel.AcademicYear }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p5[1] } ${ dataModel.Program.toUpperCase() }`, paddingX2, currentY, 'left')
        .text(`${ page.p5[2] } ${ dataModel.Campus.toUpperCase() }`, paddingX3, currentY, 'left')
        .text(`${ page.p5[3] } ${ dataModel.AcademicLevelName.toUpperCase() }`, paddingX, currentY += contentLine, 'left')
        .text(`${ page.p5[4] } ${ dataModel.ProgramOfStudy.toUpperCase() }`, center, currentY, 'left')

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[0])
        .text(page.header[5], paddingX, currentY += headerLine, 'left')

    let check = String.fromCharCode(10003)
    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(`(PLEASE CHECK     THE APPROPRIATE BOX)`, 63, currentY, 'left')

    PdfSettings.format(card, "unicode");
    card.text(check, 89.5, currentY, 'left');
    
    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(page.p6[0], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[1], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[2], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[3], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[4], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[5], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[6], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[7], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[8], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[9], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')
        .text(page.p6[10], paddingX, currentY += contentLine, 'left')
        .text(page.submitCheck, paddingX + 120, currentY, 'left')

    let extractedPage7 = card.splitTextToSize(page.p7, maxWidth);
    currentY = PdfSettings.justifyEn(card, extractedPage7, currentY += 2, paddingX, 5, informationConfig, 'helvetica');

    card.setFont('helvetica', 'normal')
        .setFontSize(sizes[1])
        .text(page.p8[0], 33, currentY += 10, 'left')
        .text(page.p8[0], 134, currentY, 'left')
        .text(page.p8[1], 35, currentY += contentLine, 'left')
        .text(page.p8[3], 133, currentY, 'left')
        .text(page.p8[2], 32, currentY += contentLine, 'left')
        .text(page.p8[2], 133, currentY, 'left')

    $('#js-print-preview').attr('src', card.output('datauristring'));
}

$(document).ready( function() {
    let data = dataModel.Body;
    masterApplication(data);
})