let acc = document.getElementsByClassName("accordion");
let filter = document.getElementById("extrafilters");

/* hides extrafilter field*/
filter.style.display = "none";

/* click function for hiding/showing panel*/
acc[0].addEventListener("click", function () {
    let panel = this.nextElementSibling;

    if (panel.style.display === "block") {
        panel.style.display = "none";
    } else {
        panel.style.display = "block";
    }
});
/* message*/
let message = document.getElementById("messagepicture");

/* previous button*/
let previous = document.getElementsByClassName("previous");

/* next button*/
let next = document.getElementsByClassName("next");
previous[0].style.display = "none";
next[0].style.display = "none";

/* image field*/
let image = document.getElementById("imgoutput");

/* image files*/
//let files = document.getElementById("imginput").files;

/* span text*/
let imagecounter = document.getElementById("imagecounter");
imagecounter.style.display = "none";

let currentImage = 0;
function decCurrentImage(length) {
    currentImage = currentImage > 0 ? --currentImage : currentImage + length - 1;
}

/* inserts first image in list into img field*/
let loadFile = function (event) {
    let files = document.getElementById("imginput").files[0];
    if (files) {
        filereader(files); /*display first image*/
        imagecount();
        message.style.display = "none";
        previous[0].style.display = "inline";
        imagecounter.style.display = "inline";
        imagecounter.style.backgroundColor = "rgba(0, 0, 0, 0.5)";
        next[0].style.display = "inline";
        image.style.display = "block";
    } else {
        message.style.display = "inline";
        imagecounter.style.display = "none";
        previous[0].style.display = "none";
        next[0].style.display = "none";
        image.style.display = "none";
    }
};

/* inserts next file into img field*/
function nextImg() {
    let files = document.getElementById("imginput").files;
    currentImage++;
    filereader(files[currentImage % files.length]);
    imagecount();
}

/* inserts previous file into img field*/
function previousImg() {
    let files = document.getElementById("imginput").files;
    decCurrentImage(files.length);
    filereader(files[currentImage % files.length]);
    imagecount();
}

/*reads file as a data url*/
function filereader(files) {
    if (files) {
        let fileReader = new FileReader();

        fileReader.onload = function (event) {
            image.src = fileReader.result;
        };
        fileReader.readAsDataURL(files);
    }
}


/*image counter*/
function imagecount() {
    let files = document.getElementById("imginput").files;
    imagecounter.textContent = ((currentImage % files.length) + 1) + "/" + files.length;
}

window.onload = function () {
    if (document.getElementById("errorpopup")) {
        toggleElements();
    }
}

function hideForm() {
    let errorElement = document.getElementById("errorpopup");
    errorElement.style.display = "none";
    toggleElements();
}

function toggleElements() {
    let inputs = document.getElementsByTagName("input");
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].id != "errorbutton") {
            inputs[i].disabled = !inputs[i].disabled;
        }
    }

    let selects = document.getElementsByTagName("select");
    for (var i = 0; i < selects.length; i++) {
        selects[i].disabled = !selects[i].disabled
    }

    let textarea = document.getElementsByClassName("descriptionInput");
    textarea[0].disabled = !textarea[0].disabled;

    let submitbutton = document.getElementById("advertisementsubmit");
    submitbutton.disabled = !submitbutton.disabled;
}





