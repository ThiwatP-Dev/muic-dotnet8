var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "This is to certify that",
    p4 : ["Adm. no.", "has completed", "credits with a grade point average of"],
    p5 : ["and thus has fulfilled all the requirements towards a", "majoring in", "from Mahidol University of Thailand.", "On", 
          "English", "I certify that English is the medium of instruction at Mahidol University.", "Date of issue :"],
    p6 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"],
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
    var fullName = `${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`;
    var issuedDate = new Date(dataBody.CreatedAt);
    let language = dataBody.Language;
    var gpaText = dataModel.Body.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataModel.Body.GPA);
    let possessive = toCapitalizationFormat(dataBody.Possessive);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    report.text(`${ page.p1 }${ dataBody.ReferenceNumber }`, paddingX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN);
    report.text(fullName, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    let p4StudentCode = report.getTextWidth(`${ page.p4[0] } ${ dataBody.StudentCode } ${ page.p4[1] } ${ dataBody.CreditComp } ${ page.p4[2] } ${ gpaText }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4AdmNo = report.getTextWidth(page.p4[0]) + 2;
    let p4StudentCodeText = report.getTextWidth(dataBody.StudentCode) + 1;

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4[0], centerX - p4StudentCodeCenter + 1, currentY += lineYN, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ GetStudentCodeFormat(dataBody.StudentCode) }`,  centerX - p4StudentCodeCenter + p4AdmNo, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifN);
    report.text(`${ page.p4[1] } ${ dataBody.CreditComp } ${ page.p4[2] } ${ gpaText }`, centerX - p4StudentCodeCenter + 1 + p4AdmNo + p4StudentCodeText, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.p5[0], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ dataBody.DegreeName },`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p5[1] } ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p5[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p5[3] } ${ getDateText('en', issuedDate) }.`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ possessive } ${ page.p5[4] } ${ dataBody.ChangeNameTypeText }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN);
    report.text(`"${ dataBody.Title } ${ dataBody.ChangedName } ${ dataBody.ChangedSurname }"`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    report.text(page.p5[5], centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p5[6] } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));;

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p6FullText = report.getTextWidth(`${ page.p6[0] } ${ verifyCode } ${ page.p6[1] }`);
    let p6FullTextCenter = p6FullText / 2;
    let p60Width = report.getTextWidth(page.p6[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p6FullTextCenter;
    report.text(page.p6[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p60Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p6[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p6[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));
    
    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})