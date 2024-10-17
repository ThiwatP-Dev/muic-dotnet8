var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "คำแปลใบปริญญาบัตร",
    p3 : ["มหาวิทยาลัยอัสสัมชัญ", "มูลนิธิคณะเซนต์คาเบรียลแห่งประเทศไทย",
          "โดยอนุมัติสภามหาวิทยาลัย ให้ปริญญาบัตรฉบับนี้ไว้แก่"],
    p4 : ["เพื่อแสดงความว่า สอบไล่ได้ตามหลักสูตร", "มีศักดิ์ และสิทธิ์ แห่งปริญญานี้ทุกประการ"],
    p5 : ["ให้ไว้ ณ วันที่", "เดือน", "พุทธศักราช"],
    p6 : "ลงนามโดย",
    p7 : "ขอรับรองว่าเป็นคำแปลที่ถูกต้อง",
    p8 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
          "www.registrar.au.edu/certificationcheck.html"]
};

function renderCertificate() {
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
          language = dataBody.Language,
          issuedFullTime = dataBody.CreatedAt.split('-'),
          issuedDate = parseInt(issuedFullTime[2].substring(0,2)),
          issuedMonth = parseInt(issuedFullTime[1])-1;

      /* -------------------------------------------------- page 1 -------------------------------------------------- */
      PdfSettings.format(report, textFormats.sansBoldN)
      let p1Width = report.getTextWidth(page.p1);
      report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
            .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

      PdfSettings.format(report, textFormats.sansBoldN)
      report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

      PdfSettings.format(report, textFormats.sansN)
      report.text(page.p3[0], centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
            .text(page.p3[1], centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
            .text(page.p3[2], centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

      PdfSettings.format(report, textFormats.sansBoldN)
      report.text(`${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

      PdfSettings.format(report, textFormats.sansN)
      report.text(page.p4[0], centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
            .text(dataBody.DegreeName, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

      if ( dataBody.AcademicHonor != null) {
          PdfSettings.format(report, textFormats.sansN)
          report.text(dataBody.AcademicHonor, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));
      }

      PdfSettings.format(report, textFormats.sansN)
      report.text(page.p4[1], centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[0] } ${ issuedDate } ${ page.p5[1] } ${ getMonthText(language, issuedMonth) } ${ page.p5[2] } ${ dataBody.Year }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

      PdfSettings.format(report, textFormats.sansN);
      report.text(`${ page.p6 } ${ dataBody.Signs[0] }`, centerX - 40, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p6 } ${ dataBody.Signs[1] }`, centerX + 40, currentY, PdfSettings.style(textStyles.CertificateCenter))
            .text(dataBody.Positions[0], centerX - 40, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
            .text(dataBody.Positions[1], centerX + 40, currentY, PdfSettings.style(textStyles.CertificateCenter));

      PdfSettings.format(report, textFormats.sansN);
      report.text(page.p7, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
            .text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
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
    renderCertificate();
})