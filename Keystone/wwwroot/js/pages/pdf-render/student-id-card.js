var dataModel = $('#id-card-preview').data('model');
var universityNameEn = $('body').data('uname-en');
var universityNameTh = $('body').data('uname-th');

function studentIdCard(dataModel, card, index) {
    PdfSettings.property(card, dataModel);

    let cardMaxWidth = 85,
        cardMaxHeight = 54,
        padding = 5,
        imageSize = [15, 18];

    let cardTextStyle = {
        align: 'right',
        lineHeightFactor: defaultSetting.lineHeight,
        maxWidth: cardMaxWidth - (padding *2) - imageSize[0]
    }

    let data = dataModel.Body[index];

    let currentY = padding + 10;
    //draw student's picture
    let imageLeft = (cardMaxWidth - padding) - imageSize[0];
    var imgHeight = $('#img-student-'+data.Code)[0].height;
    var imageWidth = $('#img-student-'+data.Code)[0].width;
    var imgRatio = imgHeight / imageWidth
    var imgRatioWidth = 18 * (1 / imgRatio)
    card.addImage($('#img-student-'+data.Code)[0], 'png', imageLeft, currentY, imgRatioWidth, imageSize[1]);

    //write name
    PdfSettings.format(card, 'ContentExtraSmallBold');
    currentY += 3.5;
    card.text(data.FirstName.toUpperCase(), imageLeft - 2, currentY, cardTextStyle)
        .text(data.LastName.toUpperCase(), imageLeft - 2, currentY += 4.5, cardTextStyle);

    //write info
    let rightest = cardMaxWidth - padding;
    PdfSettings.format(card, textFormats.sansBoldXSEn)
    card.text(data.AcademicLevel.toUpperCase(), imageLeft - 2, currentY += 3.5, cardTextStyle);

    cardTextStyle.maxWidth = 35;
    card.text(data.Faculty.toUpperCase(), imageLeft - 2, currentY += 3.5, cardTextStyle);

    PdfSettings.format(card, 'ContentExtraSmallBold');
    card.text(data.Code.replace("-", ""), rightest, currentY += 7.5, cardTextStyle);

    card.setFontSize(2.5)
        .text("MONTH/YEAR".toUpperCase(), rightest, currentY += 2.5, cardTextStyle);

    PdfSettings.format(card, textFormats.sansBoldXSEn)
    card.text(`EXP   ${ (data.ExpiredDateString) ? data.ExpiredDateString : "-" }`, rightest, currentY += 2.5, cardTextStyle);

    //back side
    card.addPage('credit-card', 'l')
    let barcodeSrc = Barcode.getBarcodePath(data.Code.replace("-", ""), { 
                                            format: barcodeFormat.code128A,
                                            height: 50});

    let barcodeAreaWidth = 10
        barcodeAreaHeight = 30
        backPadding = 1.5
        posX = cardMaxWidth - backPadding - barcodeAreaWidth
        posY = backPadding;
    card.setFillColor(defaultColor.white)
        .roundedRect(posX, posY, barcodeAreaWidth, barcodeAreaHeight, 2, 2, 'F');
    card.addImage(barcodeSrc, 'JPEG', 
                  posX + barcodeAreaWidth - backPadding, 
                  posY + barcodeAreaHeight - 10, 
                  25, 7.5, '', 'FAST', 90);

    $('#js-print-preview').attr('src', card.output('datauristring'));
}

$(function() {
    var card = new jsPDF('l', 'mm', 'credit-card');
    for (var i = 0; i < dataModel.Body.length; i++) {
        studentIdCard(dataModel, card, i);
        card.addPage('l', 'mm', 'credit-card');
    }
})