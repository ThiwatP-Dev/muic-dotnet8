var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "หนังสือรับรองสถานภาพการศึกษา",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : ["สอบไล่ได้ครบทุกลักษณะวิชาตามหลักสูตร", "สาขาวิชา"],
    p5 : ["ประจำปีการศึกษา", "เมื่อวันที่"],
    p6 : ["โดยได้รับคะแนนเฉลี่ยสะสม", "และได้รับการอนุมัติจากสภามหาวิทยาลัยอัสสัมชัญ", "ให้เป็นผู้สำเร็จการศึกษาจริง"],
    p7 : ["ปัจจุบันบุคคลดังกล่าวได้", "ให้ไว้ ณ วันที่"],
    p8 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
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
    var language = dataBody.Language;
    var graduatedDate = getDateText(language, new Date(dataBody.GraduatedAt));

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.sansN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p4[0] }${ dataBody.FacultyName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p4[1] }${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p5[0] } ${ dataBody.GraduatedYear } ${ page.p5[1] } ${ graduatedDate }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[0] } ${ NumberFormat.renderDecimalTwoDigits(dataBody.GPA) }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p6[1], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p6[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    let p7NewName = report.getTextWidth(`${ page.p7[0] }${ dataBody.ChangeNameTypeText } ${ dataBody.Title } ${ dataBody.ChangedName } ${ dataBody.ChangedSurname }`);
    let p7NewNameCenter = p7NewName / 2;
    let p7AdmNo = report.getTextWidth(`${ page.p7[0] }${ dataBody.ChangeNameTypeText }`) + 2;

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p7[0] }${ dataBody.ChangeNameTypeText }`, centerX - p7NewNameCenter + 1, currentY += lineYS, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.ChangedName } ${ dataBody.ChangedSurname }`,  centerX - p7NewNameCenter + p7AdmNo, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p7[1] } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
          .text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.sansXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p8FullText = report.getTextWidth(`${ page.p8[0] } ${ verifyCode } ${ page.p8[1] }`);
    let p8FullTextCenter = p8FullText / 2;
    let p80Width = report.getTextWidth(page.p8[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p8FullTextCenter;
    report.text(page.p8[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(verifyCode, currentX += p80Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS)
    report.text(page.p8[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(page.p8[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})