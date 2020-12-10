var acc = document.getElementsByClassName("accordion");
var filter = document.getElementById("extrafilters");

/* hides extrafilter field*/
filter.style.display = "none";

/* click function for hiding/showing panel*/
acc[0].addEventListener("click", function () {
    var panel = this.nextElementSibling;

    if (panel.style.display === "block") {
        panel.style.display = "none";
    } else {
        panel.style.display = "block";
    }
});
/* message*/
var message = document.getElementById("messagepicture");

/* previous button*/
var previous = document.getElementsByClassName("previous");

/* next button*/
var next = document.getElementsByClassName("next");
previous[0].style.display = "none";
next[0].style.display = "none";

/* image field*/
var image = document.getElementById("imgoutput");

/* image files*/
var files = document.getElementById("imginput").files;

/* span text*/
var imagecounter = document.getElementById("imagecounter")

var currentImage = 0;

/* inserts first image in list into img field*/
var loadFile = function (event) {
    filereader(0); /*display first image*/
    imagecount();
    message.style.display = "none";
    previous[0].style.display = "inline";
    imagecounter.style.backgroundColor = "rgba(0, 0, 0, 0.5)";
    next[0].style.display = "inline";
};

/* inserts next file into img field*/
function nextImg(n) {
    var files = document.getElementById("imginput").files;
    if (currentImage == files.length - 1) {
        currentImage = 0;
    }
    else {
        currentImage += n;
    }
    filereader(currentImage);
    imagecount();
}


/* inserts previous file into img field*/
function previousImg(n) {
    var files = document.getElementById("imginput").files;
    if (currentImage > 0) {
        currentImage -= n;
    } else {
        currentImage = files.length - 1;
    }
    filereader(currentImage);
    imagecount();
}

/*reads file as a data url*/
function filereader(n) {
    var files = document.getElementById("imginput").files[n];
    if (files) {
        var fileReader = new FileReader();

        fileReader.onload = function (event) {
            image.src = fileReader.result;
        };
        fileReader.readAsDataURL(files);
    }
}

/*image counter*/
function imagecount() {
    var files = document.getElementById("imginput").files;
    imagecounter.textContent = (currentImage + 1) + "/" + files.length;
}





