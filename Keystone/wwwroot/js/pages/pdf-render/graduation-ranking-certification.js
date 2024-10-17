var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C.",
    p2 : "GRADUATION STATUS CERTIFICATION",
    p3 : ["This is to certify that as of", ","],
    p4 : ["Adm. No.", ","],
    p5 : ["has completed", "credits with a cumulative grade point average of", 
          "and thus has fulfilled all the requirements towards a", ",", "majoring in", 
          "from Mahidol University of Thailand", ", at the time of", "graduation (", "),",
          "achieved the following rankings."],
    p6 : ["In the department of majoring in", "In the",
          "As an Mahidol University Undergraduate enrollee", "out of",
          "Date of issue : "],
    p7 : ["(", ")"],
    p8 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"]
};

function formatNumber(number) {
      return new Intl.NumberFormat().format(number);
}

function renderCertificate() {
      var report = new jsPDF('p', 'mm', 'a4');
      PdfSettings.property(report, dataModel);
  
      let paddingX = defaultSetting.paddingC,
          currentY = defaultSetting.paddingC,
          centerX = defaultSetting.centerX,
          lineYL = defaultSetting.LargeLine,
          lineYS = defaultSetting.SmallLine,
          fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);
      
      var dataBody = dataModel.Body;
      var language = dataBody.Language;
      var issuedDate = new Date(dataBody.CreatedAt);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style("CertificateCenter"));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p3[0] } ${ getDateText(language, new Date(dataBody.GraduatedAt)) }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }${ page.p3[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    let p4StudentCode = report.getTextWidth(`${ page.p4 } ${ dataBody.StudentCode }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4 = report.getTextWidth(page.p4[0]);

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4[0], centerX - p4StudentCodeCenter - 1, currentY += lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ GetStudentCodeFormat(dataBody.StudentCode) }${ page.p4[1] }`,  centerX - p4StudentCodeCenter + p4, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p5[0] } ${ dataBody.CreditComp } ${ page.p5[1] } ${ dataBody.GPA }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p5[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ dataBody.DegreeName }${ page.p5[3] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.ReportCenter))
          .text(`${ page.p5[4] } ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p5[5], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }${ page.p5[6] } ${ dataBody.Possessive } ${ page.p5[7] }${ dataBody.GraduatedYear }${ page.p5[8] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p5[9], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p6[0] } ${ dataBody.DepartmentName }`, centerX - 60, currentY += lineYS, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ formatNumber(dataBody.MajorRank) } ${ page.p6[3] }`, centerX + 50, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(formatNumber(dataBody.AllMajorRank), centerX + 55, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(`${ page.p6[1] } ${ dataBody.FacultyName }`, centerX - 60, currentY += lineYS, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ formatNumber(dataBody.FacultyRank) } ${ page.p6[3] }`, centerX + 50, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(formatNumber(dataBody.AllFacultyRank), centerX + 55, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(page.p6[2], centerX - 60, currentY += lineYS, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ formatNumber(dataBody.EnrolleeRank) } ${ page.p6[3] }`, centerX + 50, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(formatNumber(dataBody.AllEnrolleeRank), centerX + 55, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(`${ page.p6[4] } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p7[0] } ${ dataBody.ApprovedByName } ${ page.p7[1] }`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS);
    let verifyCode = dataBody.VerifyCode.toString();
    let p8FullText = report.getTextWidth(`${ page.p8[0] } ${ verifyCode } ${ page.p8[1] }`);
    let p8FullTextCenter = p8FullText / 2;
    let p80Width = report.getTextWidth(page.p8[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p8FullTextCenter;
    
    report.text(page.p8[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(verifyCode, currentX += p80Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS);
    report.text(page.p8[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS);
    report.text(page.p8[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
      renderCertificate();
})