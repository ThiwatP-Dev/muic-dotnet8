$(document).ready(function() {
    $('#summernote').summernote({
        height: 300, // set editor height
        minHeight: null, // set minimum height of editor
        maxHeight: null, // set maximum height of editor
        focus: false, // set focus to editable area after initializing summernote
        toolbar: [
            ['fontsize', ['fontsize']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['style', ['bold', 'italic', 'underline']],
            ['insert', ['link', 'picture']],
        ],
        dialogsFade: true,
        disableDragAndDrop: true,
        disableResizeEditor: true
    });

    $('.note-statusbar').hide();
    $('.note-editable').css("height", "30rem");
});