var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "This is to certify that",
    p4 : "Adm. No.",
    p5 : "is enrolled in the",
    p6 : "majoring in",
    p7 : ["and has completed", "credits out of the required", "credits"],
    p8 : "with a cumulative grade point average of",
    p9 : "The said student is expected to complete all the requirements",
    p10 : "for graduating by the year",
    p11 : ["result",  "still pending."],
    p12: "I also certify that English is the medium of instruction at Mahidol University",
    p13: "Date of issute :",
    p14 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
           "www.registrar.au.edu/certificationcheck.html"]
}

function renderCertificate(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        a4Width = defaultSetting.A4Width,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var informationConfig = {
        align: 'center',
        lineHeightFactor: defaultSetting.lineHeightLarge,
        maxWidth: a4Width - (paddingX * 2)
    }

    var dataBody = dataModel.Body;
    var fullName = `${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`;
    var issuedDate = new Date(dataBody.CreatedAt);
    var gpaText = dataModel.Body.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataModel.Body.GPA);
    var verbToBe = dataBody.CourseCodeAndNames.length > 1 ? "are" : "is"; 

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    report.text(`${ page.p1 }${ dataBody.ReferenceNumber }`, paddingX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN);
    report.text(fullName, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));
    let formatStudentCode = GetStudentCodeFormat(dataBody.StudentCode);

    PdfSettings.format(report, textFormats.serifN);
    let p4DataWidth = report.getTextWidth(`${ page.p4 } ${ formatStudentCode }`);
    let p4DataWidthCenter = p4DataWidth / 2;
    let p4Width = report.getTextWidth(page.p4);
    report.text(page.p4, centerX - p4DataWidthCenter - 1, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldN);
    report.text(formatStudentCode,  centerX - p4DataWidthCenter + p4Width, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifN);
    report.text(`${ page.p5 } ${ dataBody.DegreeName },`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6 } ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p7[0] } ${ dataBody.CreditEarned } ${ page.p7[1] } ${ dataBody.TotalCredit } ${ page.p7[2] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p8 } ${ gpaText }.`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p9, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p10 } ${ dataBody.Year }.`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    let CourseCodeAndNameWidth = report.getTextWidth(`${ dataBody.CourseCodeAndNames }`)
    let availableSpace = a4Width - (paddingX + CourseCodeAndNameWidth + paddingX);
    let CourseCodeAndNameEnding = `${ dataBody.CourseCodeAndNames }`
    let CourseCodeAndNameEndingLines = report.splitTextToSize(CourseCodeAndNameEnding, availableSpace);
    let newCourseCodeAndNameEnding = CourseCodeAndNameEndingLines.slice(1).join(',');
    let newCourseCodeAndNameEndingLength = report.splitTextToSize(newCourseCodeAndNameEnding, a4Width).length;

    let AllCourseAndNamePop = dataBody.CourseCodeAndNames.slice(0, -1).join(', ')
    let AllCourseAndNames = `The ${ AllCourseAndNamePop } and ${ dataBody.CourseCodeAndNames[dataBody.CourseCodeAndNames.length - 1] } ${ page.p11[0] } ${ verbToBe } ${ page.p11[1] }`;
    report.text(`${ AllCourseAndNames }`, centerX, currentY += lineYS, informationConfig)
          .text(page.p12, centerX, currentY += lineYS * newCourseCodeAndNameEndingLength, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p13 } ${ getDateText(dataBody.Language, issuedDate) }`, centerX, currentY += lineYS, informationConfig);

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS);
    let verifyCode = (dataBody.VerifyCode).toString();
    let p14FullText = report.getTextWidth(`${ page.p14[0] } ${ verifyCode } ${ page.p14[1] }`);
    let p14FullTextCenter = p14FullText / 2;
    let p140Width = report.getTextWidth(page.p14[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p14FullTextCenter;

    report.text(page.p14[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(verifyCode, currentX += p140Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS);
    report.text(page.p14[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(page.p14[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})