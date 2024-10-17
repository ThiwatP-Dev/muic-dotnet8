var selDiv = "";

document.querySelectorAll(".drop-zone__input").forEach((inputElement) => {
    const dropZoneElement = inputElement.closest(".drop-zone");
  
    dropZoneElement.addEventListener("click", (e) => {
        inputElement.click();
    });
  
    inputElement.addEventListener("change", (e) => {
        if (inputElement.files.length) {
            updateThumbnail(dropZoneElement, inputElement.files);
        }
    });
  
    dropZoneElement.addEventListener("dragover", (e) => {
        e.preventDefault();
        dropZoneElement.classList.add("drop-zone--over");
    });
  
    ["dragleave", "dragend"].forEach((type) => {
        dropZoneElement.addEventListener(type, (e) => {
            dropZoneElement.classList.remove("drop-zone--over");
        });
    });
  
    dropZoneElement.addEventListener("drop", (e) => {
        e.preventDefault();
  
        if (e.dataTransfer.files.length) {
            inputElement.files = e.dataTransfer.files;
            updateThumbnail(dropZoneElement, inputElement.files);
        }
  
        dropZoneElement.classList.remove("drop-zone--over");
    });
});

function updateThumbnail(dropZoneElement, files) {

    if (dropZoneElement.querySelector(".drop-zone__prompt")) {
        dropZoneElement.querySelector(".drop-zone__prompt").remove();
    }
    
    selDiv.innerHTML = "";
    
    for(var i=0; i < files.length; i++) {
        var f = files[i];
        
        selDiv.innerHTML += f.name + "<br/>";
    }
}
		
document.addEventListener("DOMContentLoaded", init, false);

function init() {
    document.querySelector('#files').addEventListener('change', handleFileSelect, false);
    selDiv = document.querySelector("#selectedFiles");
}
    
function handleFileSelect(e) {
    
    if(!e.target.files) return;
    
    selDiv.innerHTML = "";
    
    var files = e.target.files;
    for(var i = 0; i < files.length; i++) {
        var f = files[i];
        
        selDiv.innerHTML += f.name + "<br/>";

    }
}