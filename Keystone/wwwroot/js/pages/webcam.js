const webcamElement = document.getElementById('webcam');
const canvasElement = document.getElementById('canvas');
const snapSoundElement = document.getElementById('snapSound');
const webcam = new Webcam(webcamElement, 'user', canvasElement, snapSoundElement);
var studentCode = "";
var returnController = "";
var isWebcamMode = false;

$(document).ready(function() {
    $('#webcam-modal').on('shown.bs.modal', function(e) {
        $('#webcam-row').addClass("d-none");
        $('#upload-row').addClass("d-none");
        $("#takePhotoButton").addClass("d-none");
        $("#js-confirm-add").addClass("d-none");
        studentCode = $(e.relatedTarget).data('value');
        returnController = $(e.relatedTarget).data('return');
        // if (webcam != undefined) {
        //     startWebcam();
        // }
    });

    $('#webcam-modal').on('hidden.bs.modal', function(e) {
        if (webcam != null) {
            webcam.stop();
        }
    });
});

function selectTakePhoto() {
    if (webcam != null) {
        startWebcam();
    }
    $('#webcam-row').removeClass("d-none");
    $('#upload-row').addClass("d-none");
    $("#takePhotoButton").removeClass("d-none");
    isWebcamMode = true;
}

function selectUpload() {
    if (webcam != null) {
        webcam.stop();
    }
    $('#UploadFile').val(''); 
    $("#filename").val('');
    $('#webcam-row').addClass("d-none");
    $('#upload-row').removeClass("d-none");
    $("#takePhotoButton").addClass("d-none");
    $("#restartButton").addClass("d-none");
    $("#js-confirm-add").addClass("d-none");
    isWebcamMode = false;
}

function onSelectFile(control, index) 
{
    $("#filename").val(control.files[0].name);
    $("#js-confirm-add").removeClass("d-none");
}

function takePhoto() {
    let picture = webcam.snap();
    document.querySelector('#download-photo').href = picture;
    webcam.stop();
    $("#webcam").addClass("d-none");
    $("#canvas").removeClass("d-none");
    $("#takePhotoButton").addClass("d-none");
    $("#restartButton").removeClass("d-none");
    $("#js-confirm-add").removeClass("d-none");
}

function startWebcam() {
    webcam.start()
        .then(result =>{
            //console.log("webcam started");
        })
        .catch(err => {
            console.log(err);
        });
    document.querySelector('#download-photo').href = null;
    $("#webcam").removeClass("d-none");
    $("#canvas").addClass("d-none");
    $("#takePhotoButton").removeClass("d-none");
    $("#restartButton").addClass("d-none");
    $("#js-confirm-add").addClass("d-none");
}

function submitForm() {
    $('#StudentCode').val(studentCode);
    $('#ReturnController').val(returnController);
    var form = document.getElementById("upload-profile-image-form");
    if (isWebcamMode) {
        var ImageURL = document.querySelector('#download-photo').href;
        var block = ImageURL.split(";");
        var realData = block[1].split(",")[1];
        $('#UploadFileBase64').val(realData);
        form.submit();
    } else {
        form.submit();
    }
}