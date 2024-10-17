var PdfTemplate = (function() {

    var ReportHeaderA4 = function(pdf, paddingX, paddingY) {
        PdfSettings.format(pdf, textFormats.serifN)

        pdf.addImage(PdfSettings.img($('body').data('logo-url')), 'png', 96.25, 7.5, 17.5, 18) 

        pdf.setFontSize(16)
           .text($('body').data('uname-th'), (210 - paddingX), paddingY+=5, PdfSettings.style(textStyles.ReportRight))
           .text($('body').data('uname-en'), (210 - paddingX), paddingY+=5, PdfSettings.style(textStyles.ReportRight));
    }

    var LogoHeaderA4 = function(pdf, paddingX, paddingY) {
        pdf.addImage(PdfSettings.img($('body').data('logo-url')), 'png', paddingX+15, paddingY, 24.5, 25) 

        pdf.setFont('angsana', 'bold')
           .setFontSize(defaultTextSize.headerLarge)
           .setTextColor(defaultColor.black);
        pdf.fromHTML(`<h1>${ $('body').data('uname-en') } <i>of Thailand</i></h1>`, paddingX+45, paddingY+2.5, PdfSettings.style(textStyles.ReportLeft))
    }

    return {
        repoHeaderA4 : ReportHeaderA4,
        logoHeaderA4 : LogoHeaderA4,
    };

})();