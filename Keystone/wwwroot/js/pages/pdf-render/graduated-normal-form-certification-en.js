var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "GRADUATION STATUS CERTIFICATION",
    p3 : "This is to certify that as of",
    p4 : "Adm. No.",
    p5 : ["has completed", "credits with a cumulative grade point average of"],
    p6 : "and thus has fulfilled all the requirements towards a ",
    p7 : "majoring in",
    p8 : "from Mahidol University of Thailand.",
    p9 : "I also certify that English is the medium of instuition at Mahidol University.",
    p10 : "Date of issue :",
    p11 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
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
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var dataBody = dataModel.Body;
    var issuedDate = new Date(dataBody.CreatedAt);
    var graduatedDate = new Date(dataBody.GraduatedAt);
    var gpaText = dataBody.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataBody.GPA);
    var language = dataBody.Language.toLowerCase();

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    let p1Width = report.getTextWidth(page.p1);
    report.text(`${ page.p1 } ${ dataBody.ReferenceNumber }`, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    report.text(`${ page.p3 } ${ getDateText(language, graduatedDate) }.`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    let p4StudentCode = report.getTextWidth(`${ page.p4 } ${ dataBody.StudentCode }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4 = report.getTextWidth(page.p4);

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4, centerX - p4StudentCodeCenter - 1, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(GetStudentCodeFormat(dataBody.StudentCode),  centerX - p4StudentCodeCenter + p4, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p5[0] } ${ dataBody.CreditEarned } ${ page.p5[1] } ${ gpaText }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p6, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ dataBody.DegreeName },`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p7 } ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p8, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p9, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p10 } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS);
    let verifyCode = (dataBody.VerifyCode).toString();
    let p11FullText = report.getTextWidth(`${ page.p11[0] } ${ verifyCode } ${ page.p11[1] }`);
    let p11FullTextCenter = p11FullText / 2;
    let p110Width = report.getTextWidth(page.p11[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p11FullTextCenter;

    report.text(page.p11[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(verifyCode, currentX += p110Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS);
    report.text(page.p11[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(page.p11[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})