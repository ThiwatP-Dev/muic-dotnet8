var dataModel = $('#js-pdf-preview').data('model');

var page = {
    header : ["วิทยาลัยนานาชาติ มหาวิทยาลัยมหิดล", "MAHIDOL UNIVERSITY INTERNATIONAL COLLEGE",
              "999 ถ.พุทธมณฑล 4 ต.ศาลายา อ.พุทธมณฑล จ.นครปฐม 73170 โทรศัพท์ 0-2441-5090 โทรสาร 0-2441-9745",
              "999 Phutthamonthon 4 Rd., Salaya, Phutthamonthon, Nakhon Pathom 73170, THAILAND Tel.0-2441-5090 Fax.0-2441-9745",
              "ใบเสร็จรับเงิน ต้นฉบับ (Original Receipt)", "ใบเสร็จรับเงิน สำเนา (Copy Receipt)", "เลขที่", "Number", "วันที่", "Date"],
    tableHeader : ["ได้รับเงินจาก", "สถานที่ Issued at :", "Received From :", "โดย By", "เลขที่อ้างอิง", "Reference No. :"],
    tableHeader2 : ["รายการ", "จำนวน", "จำนวนเงิน", "DESCRIPTION", "QUANTITY", "AMOUNT", "รวม TOTAL"],
    footer : ["1. ชำระโดย / Paid By :", "CASH =", "THB", "2. ใบเสร็จฉบับนี้จะสมบูรณ์ เมื่อเช็คผ่านบัญชีเรียบร้อยแล้ว",
              "THIS RECEIPT IS NOT VALID UNTIL CHEQUE IS CLEARED", "พนักงานรับเงิน / COLLECTOR"]
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

function registrationReceipt(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);
    let centerX = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX
        currentY = 10;
        headerLine = 76
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;
        boldText = "bold";
        simpleText = "normal";

    var sizes = [12, 14, 18]
    switchFormat(report, boldText);
    report.addImage($('#js-muic-logo')[0], 'png', 10, 10, 25, 25)

    switchFormat(report, boldText);
    report.setFontSize(sizes[2])
          .text(page.header[0], paddingX + 30, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.header[1], paddingX + 30, currentY += 7, PdfSettings.style(textStyles.CertificateLeft));

    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[2], paddingX + 30, currentY += 5)
          .text(page.header[3], paddingX + 30, currentY += 5);

    switchFormat(report, boldText);
	if (!dataModel.IsPrinted) {
		report.setFontSize(sizes[2])
			  .text(page.header[4], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
	} else {
		report.setFontSize(sizes[2])
			  .text(page.header[5], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
    }

    currentY = 10;
    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[6], centerX + 60, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.ReceiptNumber, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[7], centerX + 60, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[8], centerX + 75, currentY += 5, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.CreatedAt, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[9], centerX + 75, currentY += 2, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, 45, 194, 25, 'DF')
          .rect(8, 70, 194, 15, 'DF')
          .rect(8, 85, 194, 70, 'DF')
          .rect(125, 155, 30, 10, 'DF')
          .rect(155, 155, 47, 10, 'DF')
          .line(125, 70, 125, 155)
          .line(155, 45, 155, 155);

    currentY = 50;
    report.setFontSize(sizes[1])
          .text(page.tableHeader[0], paddingX + 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[1], centerX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[2], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.StudentName, paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[3], centerX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PrintedBy ? dataModel.PrintedBy : "N/A", centerX + 73, currentY + 10, PdfSettings.style(textStyles.CertificateCenter));

    report.text(page.tableHeader[4], paddingX + 5, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[5], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.InvoiceNumber, paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader2[0], paddingX + 55, currentY += 9, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[1], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[2], centerX + 73, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[3], paddingX + 55, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[4], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[5], centerX + 73, currentY, PdfSettings.style(textStyles.CertificateCenter));

    var currentReceiptLine = 90;
    if (dataModel.ReceiptItems != null) {
        dataModel.ReceiptItems.forEach((item) => {
            report.text(`- ${ item.Name }`, paddingX + 5, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft));
            report.text(item.Quantity, centerX + 35, currentReceiptLine, PdfSettings.style(textStyles.CertificateCenter));
            report.text(item.TotalAmount, centerX + 90, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));
            currentReceiptLine = currentReceiptLine + 5;
        })
    };

    currentY = 161;
    report.text(`( ${ dataModel.TotalAmountTextTh } )`, paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[6], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.TotalAmount, centerX + 90, currentY, PdfSettings.style(textStyles.CertificateRight));

    report.text(page.footer[0], paddingX, currentY += 25, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.footer[1] } ${ dataModel.TotalAmount } ${ page.footer[2] }`, paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.footer[3], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.footer[4], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PrintedBy != null ? `( ${ dataModel.PrintedBy } )` : "( )", centerX + 70, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))

    switchFormat(report, boldText);
    report.text(page.footer[5], centerX + 70, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(document).ready( function() {
    let data = dataModel.Body;
    registrationReceipt(data);
})