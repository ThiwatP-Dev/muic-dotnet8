var dataModel = $('#js-pdf-preview').data('model');

var page = {
    header : ["บัญชีคุมยอดจ่ายเงินทุนการศึกษา", "ประเภททุน :", "ประจำ"],
    p1 : ["ชื่อผู้ได้รับทุน", "รหัสประจำตัว", "สาขาวิชา", "เงื่อนไข:"],
    p2 : ["สัญญามีผลบังคับใช้:", "ระยะเวลา", "ปี ตั้งแต่วันที่", "ครั้งที่", "ภาคการศึกษาที่", "จำนวนเงิน", "บาท", "รวมเงินที่รับไปแล้ว"]
};

function switchFormat(pdf, format) {
    switch (format) {
        case "bold":
            PdfSettings.format(pdf, textFormats.sansBoldXS)
            break;

        case "normal":
            PdfSettings.format(pdf, textFormats.sansXS)
            break;
    }
}

function financialTransaction(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);
    let centerX = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX
        halfCenterX = (centerX - paddingX)/2,
        scholarshipType = "";
        currentY = 20;
        headerLine = 7;
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;
        boldText = "bold";
        simpleText = "normal";
    
    var informationConfig = {
        align: 'left',
        lineHeightFactor: 5,
        maxWidth: defaultSetting.A4MaxWidth - paddingX
    }

    switchFormat(report, boldText);
    report.setFontSize(16)
          .text(page.header[0], centerX, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.header[1] } ${ dataModel.Scholarship }`, centerX, currentY += 10, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.header[2] } ${ dataModel.CurrentTerm }`, centerX, currentY += 10, PdfSettings.style(textStyles.CertificateCenter));

    report.text(page.p1[0], paddingX, currentY += 20, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.p1[1], paddingX, currentY += 10, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.p1[2], paddingX + 50, currentY, PdfSettings.style(textStyles.CertificateLeft));
    if (dataModel.Condition != null) {
        report.text(page.p1[3], paddingX, currentY += 10, PdfSettings.style(textStyles.CertificateLeft));
    }

    currentY = 60
    switchFormat(report, simpleText);
    report.setFontSize(16)
          .text(dataModel.StudentName, paddingX + 25, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.StudentCode, paddingX + 25, currentY += 10, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.Major, paddingX + 70, currentY, PdfSettings.style(textStyles.CertificateLeft))

    if (dataModel.Condition != null) {
        let extractedCondition = report.splitTextToSize(dataModel.Condition, maxWidth - 10);
        currentY = PdfSettings.justifyEn(report, extractedCondition, currentY += 4, paddingX + 25, 6, informationConfig, 'sarabun');
    }

    switchFormat(report, boldText);
    report.setFontSize(16)
          .text(page.p2[0], paddingX, currentY += 10, PdfSettings.style(textStyles.CertificateLeft))

    var approvedDate = dataModel.ApprovedDate != null ? getDateText("en", new Date(dataModel.ApprovedDate)) : getDateText("en", new Date());
    var expiredDate = dataModel.ExpiredDate != null ? getDateText("en", new Date(dataModel.ExpiredDate)) : getDateText("en", new Date());
    switchFormat(report, simpleText);
    report.setFontSize(16)
          .text(`${ page.p2[1] } ${ dataModel.TotalYear } ${ page.p2[2] } ${ approvedDate } - ${ expiredDate }`, paddingX + 35, currentY, PdfSettings.style(textStyles.CertificateLeft));

    var currentReceiptLine = currentY + 10;
    if (dataModel.FinancialTransactions != null) {
        var index = 1;
        dataModel.FinancialTransactions.forEach((item) => {
            report.text(`${ page.p2[3] } ${ index }`, paddingX + 15, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                  .text(`${ page.p2[4] } ${ item.Term } ${ item.Type != "" ? "(" + item.Type + ")" : "" }`, paddingX + 35, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                  .text(page.p2[5], centerX + 10, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                  .text(item.AmountText, centerX + 70, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight))
                  .text(page.p2[6], centerX + 80, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));
            currentReceiptLine = currentReceiptLine + 10;
            index++;
        })
    };

    currentY = currentReceiptLine;
    switchFormat(report, boldText);
    report.setFontSize(16)
          .text(page.p2[7], paddingX + 35, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.TotalAmount, centerX + 70, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight))
          .text(page.p2[6], centerX + 80, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));

    switchFormat(report, simpleText);
    report.setFontSize(16)
          .text(dataModel.SignatoryName1, centerX - halfCenterX, currentY += 20, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.SignatoryName2, centerX + halfCenterX, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.SignatoryPosition1, centerX - halfCenterX, currentY += 10, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.SignatoryPosition2, centerX + halfCenterX, currentY, PdfSettings.style(textStyles.CertificateCenter))

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(document).ready( function() {
    let data = dataModel.Body;
    financialTransaction(data);
})