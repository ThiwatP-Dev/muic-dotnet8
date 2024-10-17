var dataModel = $('#js-pdf-preview').data('model');

var page = {
	header: ["MAHIDOL UNIVERSITY", "INTERNATIONAL COLLEGE", "Trimester :", "Registration Invoice",
		"STUDENT :", "Invoice No.", "PROGRAM :", "Tuition & Fee confirmed date :"],
	fee: ["Fee Code", "Item", "Amount (Baht)", "Discount",  "Total Payment of"],
	signature: ["...........................MUIC Finance Officer...........................", "..................MUIC Academic Service Officer..................",
		"Signature.......................................................................",
		"Date................../........................../........................."],
	p1: ["** Payment due date for normal enrollment is", "Any late payment will be charged 100 baht per day maximum of 1,000 baht",
		"Please return this invoice slip to the MUIC Finance Office after your payment is completed at the bank.",
		"Failure to return this invoice slip will result in incomplete registration.",
		"This receipt is valid only if the college has cleared the payment"],
	p1add_drop: ["** Payment due date for add/drop/late enrollment is"],
	p2: ["MAHIDOL UNIVERSITY", "DEPOSIT SLIP", "For Bank", "INTERNATIONAL COLLEGE", "Pay in slip at counter of any branch of Siam Commercial Bank",
		"เพื่อเข้าบัญชีวิทยาลัยนานาชาติ มหาวิทยาลัยมหิดล", "บมจ.ธนาคารไทยพาณิชย์ เลขที่บัญชี 333-3-00119-7"],
	student: ["Student Name :", "Identification No. :", "Debit Invoice No. :", "Date :"],
	payment: ["Service Code : MUIC", "เงินสด", "เช็ค", "เลขที่เช็ค", "ชื่อธนาคาร/สาขา", "จำนวนเงิน", "In Figures :"]
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

function registrationInvoice(dataModel) {
	var report = new jsPDF('p', 'mm', 'a4');
	PdfSettings.property(report, dataModel);
	let centerX = defaultSetting.centerX;
	paddingX = defaultSetting.paddingX;
	let endX = defaultSetting.centerX + 95;
	currentY = 20;
	headerLine = 7;
	contentLine = 5;
	maxWidth = defaultSetting.A4MaxWidth - paddingX;
	boldText = "bold";
	simpleText = "normal";

	var sizes = [10, 14, 25]
	switchFormat(report, boldText);
	report.addImage($('#js-muic-logo')[0], 'png', 10, 10, 20, 20)
		  .setFontSize(sizes[2])
		  .text(page.header[0], centerX, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.header[1], centerX, currentY += 5, PdfSettings.style(textStyles.CertificateCenter));

	switchFormat(report, simpleText);

	report.setFontSize(sizes[1])
		  .text(page.header[2], paddingX, currentY += 15, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${ dataModel.Term } - ${ dataModel.AcademicYear }`, paddingX + 20, currentY, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.header[3], centerX + 95, currentY, PdfSettings.style(textStyles.CertificateRight))
		  .text(page.header[4], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${ dataModel.StudentCode } ${ dataModel.FullName }`, paddingX + 20, currentY, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${ page.header[5] } ${ dataModel.InvoiceNumber }`, centerX + 95, currentY, PdfSettings.style(textStyles.CertificateRight))
		  .text(page.header[6], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(dataModel.Program, paddingX + 20, currentY, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${page.header[7]} ${ dataModel.PrintedAt }`, centerX + 95, currentY, PdfSettings.style(textStyles.CertificateRight));

	report.text(page.fee[0], paddingX + 5, currentY += 11, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.fee[1], centerX, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.fee[2], centerX + 92, currentY, PdfSettings.style(textStyles.CertificateRight));

	currentY = 65;
	if (dataModel.InvoiceItems != null) {
		dataModel.InvoiceItems.forEach((item) => {
			currentY = currentY + 5;
			report.text(item.Code, paddingX + 10, currentY, PdfSettings.style(textStyles.CertificateLeft))
				  .text(item.Name, centerX - 25, currentY, PdfSettings.style(textStyles.CertificateLeft))
				  .text(item.Amount, centerX + 92, currentY , PdfSettings.style(textStyles.CertificateRight));
		})

		//fix total and discount footer table case invoice item less than 7 row
		console.log(dataModel.InvoiceItems.length);
		if(dataModel.InvoiceItems.length < 7)
		{
			currentY = 100;
		}

		currentY = currentY + 3;
	};

	//Header Fee Table
	report.line(10, 55, 200, 55)
		  .line(10, 65, 200, 65)
		  .line(70, 55, 70, 65)
		  .line(140, 55, 140, 65);

	//line under discount or total payment
	report.line(10, currentY, 200, currentY);
	
	report.text(page.fee[3], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(dataModel.AllDiscountAmountText, centerX + 92, currentY, PdfSettings.style(textStyles.CertificateRight))
		  .text(page.fee[4], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(dataModel.TotalAmount, centerX + 92, currentY, PdfSettings.style(textStyles.CertificateRight))

	//Rectangle Signature
	currentY = currentY + 5;
	report.setFillColor(255, 255, 255)
		  .rect(20, currentY, 80, 20, 'DF')
		  .rect(110, currentY, 80, 20, 'DF');


	report.text(page.signature[0], paddingX + 50, currentY += 6, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.signature[1], centerX + 45, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.signature[2], paddingX + 50, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.signature[2], centerX + 45, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.signature[3], paddingX + 50, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.signature[3], centerX + 45, currentY, PdfSettings.style(textStyles.CertificateCenter))

	switchFormat(report, simpleText);
	//report.setFontSize(sizes[1])
	//	  .text(`${ dataModel.StartedDate } - ${ dataModel.EndedDate }`, centerX + 95, currentY += 10, PdfSettings.style(textStyles.CertificateRight))

	//var fullDate = report.getTextWidth(`${dataModel.StartedDate} - ${dataModel.EndedDate}`);
	//var regOrAddDropText = dataModel.IsAddDrop == '1' ? page.p1add_drop[0] : page.p1[0];

	report.setFontSize(sizes[1])
		.text(`${dataModel.EndedDate}`, centerX + 95, currentY += 10, PdfSettings.style(textStyles.CertificateRight))

	var fullDate = report.getTextWidth(`${dataModel.EndedDate}`);
	var regOrAddDropText = (dataModel.IsAddDrop == '1' || dataModel.IsLateRegis == '1') ? page.p1add_drop[0] : page.p1[0];
	switchFormat(report, boldText);
	report.setFont('arial', 'italic')
		  .setFontSize(sizes[0])
		  .text(regOrAddDropText, endX - (fullDate + 1), currentY, PdfSettings.style(textStyles.CertificateRight))

	switchFormat(report, boldText);
	report.setFont('arial', 'italic')
		  .setFontSize(sizes[0])
		  .text(page.p1[1], centerX + 95, currentY += 5, PdfSettings.style(textStyles.CertificateRight))

	switchFormat(report, simpleText);
	report.setFontSize(sizes[1])
		  .text(page.p1[2], paddingX, currentY += 7, 'left')
		  .text(page.p1[3], paddingX, currentY += 5, 'left')
		  .text(page.p1[4], paddingX, currentY += 5, 'left')

	//dotted line
	report.setLineDash([1, 1])
		  .line(10, currentY += 4 , 200, currentY);

	report.text(page.p2[0], paddingX, currentY += 6, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.p2[1], centerX, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .setFont('arial', 'italic')
		  .setFontSize(sizes[0])
		  .text(page.p2[2], centerX + 95, currentY, PdfSettings.style(textStyles.CertificateRight))

	//Rectangle student information
	report.setLineDash([])
		  .setFillColor(255, 255, 255)
		  .rect(95, currentY + 5, 105, 20, 'DF');

	switchFormat(report, simpleText);
	report.text(page.p2[3], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.p2[4], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))

	switchFormat(report, simpleText);
	report
		  .text(`${ page.student[0] } ${ dataModel.FullName }`, centerX - 8, currentY - 1, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${ page.student[1] } ${ dataModel.StudentCode }`, centerX - 8, currentY + 3.5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(`${ page.student[2] } ${ dataModel.InvoiceNumber }`, centerX - 8, currentY + 8.5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.student[3], centerX - 8, currentY + 13, PdfSettings.style(textStyles.CertificateLeft))

	report.setFontSize(sizes[0])
		  .text(page.p2[5], paddingX, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.p2[6], paddingX + 3, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))

	//Rectangle Checkbox
	report.setLineDash([])
		  .setFillColor(255, 255, 255)
	      .rect(10, currentY - 2, 2, 2, 'DF')
	let footerTableY = currentY + 14;
	report.setFillColor(255, 255, 255)
		  .line(10, footerTableY, 200, footerTableY)
		  .line(40, footerTableY + 5, 200, footerTableY + 5)
		  .line(10, footerTableY + 10, 200, footerTableY + 10)
		  .line(10, footerTableY + 15, 200, footerTableY + 15)
		  .line(10, footerTableY, 10, footerTableY + 15)
		  .line(40, footerTableY, 40, footerTableY + 15)
		  .line(75, footerTableY, 75, footerTableY + 15)
		  .line(165, footerTableY, 165, footerTableY + 15)
		  .line(200, footerTableY, 200, footerTableY + 15);

	report.setFontSize(sizes[1])
		  .text(page.payment[0], paddingX, currentY += 6, PdfSettings.style(textStyles.CertificateLeft))

	report.setFontSize(sizes[0])
		  .text(page.payment[1], paddingX + 8, currentY += 12, PdfSettings.style(textStyles.CertificateLeft))
		  .text(page.payment[2], paddingX + 20, currentY, PdfSettings.style(textStyles.CertificateLeft));

	//checkbox in header table 
	report.setFillColor(255, 255, 255)
		  .rect(15, currentY - 2, 2, 2, 'DF')
		  .rect(27, currentY - 2, 2, 2, 'DF');

	report.text(page.payment[3], paddingX + 46, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.payment[4], centerX + 15, currentY, PdfSettings.style(textStyles.CertificateCenter))
		  .text(page.payment[5], centerX + 94, currentY, PdfSettings.style(textStyles.CertificateRight))

	report.setFontSize(sizes[1])
		  .text(dataModel.TotalAmount, centerX + 94, currentY += 5, PdfSettings.style(textStyles.CertificateRight))
		  .text(page.payment[6], paddingX + 9, currentY += 5, PdfSettings.style(textStyles.CertificateCenter));

	if(dataModel.Base64Barcode)
	{
		var image = new Image();
		image.src = 'data:image/png;base64,' + dataModel.Base64Barcode;
		report.addImage(image, 'png', paddingX - 1.5, currentY + 5, 30, 30)
	}
	if(dataModel.Barcode)
	{
		//waiting barcode
		// let cardMaxWidth = 30;
		// let barcodeAreaWidth = 30
        // barcodeAreaHeight = 30
        // backPadding = 1.5
        // posX = cardMaxWidth - backPadding - barcodeAreaWidth
        // posY = backPadding;

    	// let barcodeSrc = Barcode.getBarcodePath("00020101021230670115313718450825545022006363810792100047189032020210920090559051021520470115303764540894500".replace("-", ""), { 
        // format: barcodeFormat.code128A,
        // height: 50});
	}

	$('#js-print-preview').attr('src', report.output('datauristring'));
}

$(document).ready(function () {
	let data = dataModel.Body;
	registrationInvoice(data);
})