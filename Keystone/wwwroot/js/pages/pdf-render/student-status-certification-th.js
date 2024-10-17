var dataModel = $('#certificate-preview').data('model');
var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "หนังสือรับรอง",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : "รหัสประจำตัวนักศึกษา",
    p5 : "เคยเป็นนักศึกษาระดับ",
    p6 : "คณะ",
    p7 : "ของมหาวิทยาลัยอัสสัมชัญ",
    p8 : "ให้ไว้ ณ วันที่",
    p9 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
           "www.registrar.au.edu/certificationcheck.html"]
}

function generateCertificate(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var dataBody = dataModel.Body,
        issuedDate = new Date(dataBody.CreatedAt),
        language = dataBody.Language.toLowerCase();

    /* -------------------------------------------------- page 1 -------------------------------------------------- */
    PdfSettings.format(report, textFormats.sansN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    let studentCode = GetStudentCodeFormat(dataBody.StudentCode),
        p4LineWidth = report.getTextWidth(`${ page.p4 } ${ studentCode }`),
        p4WidthCenter = p4LineWidth / 2,
        p4Width = report.getTextWidth(page.p4),
        currentX = centerX - p4WidthCenter;

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p4, currentX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(studentCode,  currentX += p4Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p5 }${ dataBody.AcademicLevelName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p6 }${ dataBody.FacultyName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p7, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p8 } ${ getDateText(language, issuedDate) }`, centerX, currentY += 18, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansXS)
    let verifyCode = (dataBody.VerifyCode).toString(),
        p9FullText = report.getTextWidth(`${ page.p9[0] } ${ verifyCode } ${ page.p9[1] }`),
        p9FullTextCenter = p9FullText / 2,
        p9_0Width = report.getTextWidth(page.p9[0]),
        verifyCodeWidth = report.getTextWidth(verifyCode);

    currentX = centerX - p9FullTextCenter;
    report.text(page.p9[0], currentX, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(verifyCode, currentX += p9_0Width + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS)
    report.text(page.p9[1], currentX += verifyCodeWidth + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(page.p9[2], centerX, fromBottom, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
};

$(function() {
    generateCertificate(dataModel);
})