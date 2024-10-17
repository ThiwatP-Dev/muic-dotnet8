var dataModel = $('#certificate-preview').data('model');

var page = {
    p1: "ที่ ทบ. ร.",
    p2: "หนังสือรับรองสถานภาพการศึกษา",
    p3: "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4: "สอบไล่ได้ครบทุกลักษณะวิชาตามหลักสูตร",
    p5: "สาขา",
    p6: ["ประจำปีการศึกษา", "เมื่อวันที่"],
    p7: "โดยได้รับคะแนนเฉลี่ยสะสม",
    p8: "และได้รับการอนุมัติจากสภามหาวิทยาลัยอัสสัมชัญ",
    p9: "ให้เป็นผู้สำเร็จการศึกษาจริง",
    p10: "ให้ไว้ ณ วันที่",
    p11 : ["สามารถตรวจสอบได้ว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
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

    let dataBody = dataModel.Body;
    let language = dataBody.Language;
    let fullName = `${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`;
    let graduatedDate = getDateText(language, new Date(dataBody.GraduatedAt));
    var gpaText = dataBody.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataBody.GPA);
    let createdAt = getDateText(language, new Date(dataBody.CreatedAt));

    /* -------------------------------------------------- page 1 -------------------------------------------------- */
    PdfSettings.format(report, textFormats.sansN);
    report.text(`${ page.p1 } ${ dataBody.ReferenceNumber }`, paddingX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN);
    report.text(fullName, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    report.text(`${ page.p4 }${ dataBody.DegreeName }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN);
    report.text(`${ page.p5 }${ dataBody.DepartmentName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    report.text(`${ page.p6[0] } ${ dataBody.GraduatedYear } ${ page.p6[1] } ${ graduatedDate }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    report.text(`${ page.p7 } ${ gpaText }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p8, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p9, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));
    
    report.text(`${ page.p10 } ${ createdAt }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.sansXS);
    let verifyCode = (dataBody.VerifyCode).toString();
    let p11FullText = report.getTextWidth(`${ page.p11[0] } ${ verifyCode } ${ page.p11[1] }`);
    let p11FullTextCenter = p11FullText / 2;
    let p110Width = report.getTextWidth(page.p11[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p11FullTextCenter;

    report.text(page.p11[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS);
    report.text(verifyCode, currentX += p110Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS);
    report.text(page.p11[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS);
    report.text(page.p11[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel)
})