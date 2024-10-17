const barcodeFormat = {
    'code128A' : "CODE128A"
}

var Barcode = (function() {

    var GenerateBarcodePath = function(data, option) {
        let imageArea = document.createElement("img");
        imageArea.id = "temporary-image";
        $('body').append(imageArea);

        let barcodeImage = document.getElementById('temporary-image');
        $(barcodeImage).JsBarcode(data, option);
        
        let barcodeSrc = barcodeImage.src;
        barcodeImage.remove();

        return barcodeSrc;
    }

    var GenerateBarcodeImage = function(data, option, target) {
        $(target).JsBarcode(data, option);
    }

    return {
        getBarcodePath : GenerateBarcodePath,
        renderBarcode : GenerateBarcodeImage
    }

})();