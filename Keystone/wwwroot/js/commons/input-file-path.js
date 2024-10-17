var fileInput = '.form-file-input';

$(document).on('change', fileInput, function() {
    let inputLabel	 = $(this).next(),
        labelVal = inputLabel.html(),
        filePath = "";
    
    if (this.files && this.files.length > 1) {
        filePath = `${ this.files.length } files selected.`;
    }
    else {
        filePath = this.files[0].name;
    }
        
    if (filePath) {
        inputLabel.html(filePath)
                  .addClass('enable-item');
    }
    else {
        inputLabel.html(labelVal)
                  .removeClass('enable-item');
    }
});