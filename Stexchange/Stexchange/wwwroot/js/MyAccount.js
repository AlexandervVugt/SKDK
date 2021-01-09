document.getElementById("accountsettingsButton").style.backgroundColor = "#465D43";
document.getElementById("accountsettingsButton").style.color = "white";

function showRequestForm(form, buttonId, listingId, listingTitle) {
    let requestform = document.getElementById(form);

    let myadvertisementColumn = document.getElementById("myadvertisementsColumn");
    let settingsColumn = document.getElementById("mySettingsColumn");
    let ratingColumn = document.getElementById("ratingreviewColumn");
    let modifyColumn = document.getElementById("modifyAdvertismentForm");
    let ratingReviewColumn = document.getElementById("reviewAdvertisementColumn");
    myadvertisementColumn.style.display = "none";
    settingsColumn.style.display = "none";
    ratingColumn.style.display = "none";
    modifyColumn.style.display = "none";
    ratingReviewColumn.style.display = "none";
    if (form == "requestForm") {
        myadvertisementColumn.style.display = "block";
        requestform.style.display = "block";
        populateRequestForm(listingId, listingTitle);
    } else if (form == "modifyAdvertismentForm") {
        myadvertisementColumn.style.display = "none";
        requestform.style.display = "block";
    } else if (form == "reviewAdvertisementColumn"){
        ratingColumn.style.display = "none";
        requestform.style.display = "block";
    }
    else {
        requestform.style.display = "block";
    }

    toggleActive(buttonId);
}

function toggleVisible(id) {
    value = document.querySelector(`#li${id} > div.advertisementOptions > label > input`).checked;
    $.ajax({
        type: "POST",
        url: `/Account/SetVisible?listingId=${id}&value=${value}`,
        success: function (data) {
            console.log(data);
            alert(data);
        },
        error: function (err) {
            console.log("toggleVisible ajax request error");
            alert(err.responseText);
        }
    })
}

function populateModifyForm(id) {
    imgoutput = document.querySelector("#imgoutput");
    title = document.querySelector("#generalinformationContainer > input:nth-child(2)");
    description = document.querySelector("#generalinformationContainer > textarea");
    nameLT = document.querySelector("#generalinformationContainer > input:nth-child(6)");
    nameNL = document.querySelector("#generalinformationContainer > input:nth-child(8)");
    quantity = document.querySelector("#generalinformationContainer > input:nth-child(10)");

    $.ajax({
        type: "GET",
        url: `/Account/GetModifyFormData?listingId=${id}`,
        success: function (data) {
            data = JSON.parse(data);
            console.log(data);
            imgoutput.value = data["Pictures"];
            title.value = data["Title"];
            description.value = data["Description"];
            nameLT.value = data["NameLatin"];
            nameNL.value = data["NameNl"];
            quantity.value = data["Quantity"];
            let options = document.querySelectorAll("#requiredFiltersContainer > select > option, #extrafiltersContainer > select > option");
            for (option in options) {
                if (data["Filters"].includes(options[option].value)) {
                    options[option].selected = true;
                } else {
                    options[option].selected = false;
                }
            }
        },
        error: function (err) {
            console.log("populateModifyForm ajax request error");
            alert(err.responseText);
        }
    })
}


function changeSettings() {
    username = document.querySelector("input#username").value;
    postalcode = document.querySelector("#postalcode").value;
    email = document.querySelector("#email").value;
    password = document.querySelector("#password").value;
    confirm_password = document.querySelector("#confirm_password").value;

    $.ajax({
        type: "POST",
        //url: `/Account/ChangeAccountSettings?username=${username}&postalcode=${postalcode}&email=${email}&password=${password}&confirm_password=${confirm_password}`,
        url: '/Account/ChangeAccountSettings/',
        data: { username, postalcode, email, password, confirm_password },
        success: function (data) {
            alert(data);
        },
        error: function (err) {
            alert(err.responseText);
        }
    })
}



function populateRequestForm(listingId, listingTitle) {
    document.querySelector("#myrequestForm > p").innerHTML = listingTitle;
    document.getElementById("listingId").value = listingId;
    let select = document.getElementById("username");
    $.ajax({
        type: "GET",
        url: `/Account/GetInteractingUsers?listingId=${listingId}`,
        success: function (data) {
            console.log("hello");
            console.log(data);
            $(data).each(function (index, value) {
                console.log("hello" + index);
                let option = document.createElement("option");
                option.value = value;
                option.innerHTML = value;
                select.appendChild(option);
            });
        },
        error: function (err) {
            console.log(err);
        }
    })
    $.ajax({
        type: "GET",
        url: `/Account/GetQuantity?listingId=${listingId}`,
        success: function (data) {
            console.log(data);
            document.getElementById("quantity").value = data;
        },
        error: function (err) {
            console.log(err);
        }
    })
}

function closeRequestForm(form) {
    let myadvertisementColumn = document.getElementById("myadvertisementsColumn");
    let ratingColumn = document.getElementById("ratingreviewColumn");
    let requestform = document.getElementById(form);
    requestform.style.display = "none";
    if (form == 'mySettingsColumn' || form == 'modifyAdvertismentForm') {
        myadvertisementColumn.style.display = "block";
        toggleActive("accountsettingsButton");
    } else if (form == 'reviewAdvertisementColumn') {
        ratingColumn.style.display = "block";
        toggleActive("ratingreviewButton");
    }
}

function submitForm(form, submitform) {
    let requestform = document.getElementById(submitform);
    closeRequestForm(form);
    toggleActive("accountsettingsButton");
    requestform.submit();
}

function toggleActive(buttonId) {
    var buttons = document.getElementsByTagName("button");
    let i;

    for (i = 0; i < buttons.length; i++) {
        buttons[i].style.backgroundColor = "white";
        buttons[i].style.color = "#465D43";
    }

    document.getElementById(buttonId).style.backgroundColor = "#465D43";
    document.getElementById(buttonId).style.color = "white";
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

    let buttons = document.getElementsByTagName("button");
    for (var i = 0; i < buttons.length; i++) {
        if (buttons[i].id == "accountsettingsButton" || buttons[i].id == "ratingreviewButton" || buttons[i].id == "mySettingsButton") {
            buttons[i].disabled = !buttons[i].disabled;
        }
    }

    let selects = document.getElementsByTagName("select");
    for (var i = 0; i < selects.length; i++) {
        selects[i].disabled = !selects[i].disabled
    }
}

function confirmToDelete(id) {
    let confirmMessage = document.getElementById("confirmDelete");
    document.querySelector("#confirmDelete > [name='listingId']").value = id;
    confirmMessage.style.display = "block";
}

function populateReviewForm(reviewId, username, plantname, hasReviewQuality) {
    document.querySelector("#reviewAdvertisementColumn > p:nth-child(6)").innerHTML = "Naam gebruiker: " + username;
    document.querySelector("#reviewAdvertisementColumn > p:nth-child(5)").innerHTML = "Naam plant: " + plantname;
    document.querySelector("#reviewAdvertisementColumn > p:nth-child(3)").innerHTML = "Vul de volgende gegevens in om de advertentie van " + username + " te beoordelen";
    document.querySelector("#reviewAdvertisementColumn > form > input[name='reviewId']").value = reviewId;
    if (hasReviewQuality == false) {
        document.querySelector("#reviewAdvertisementColumn > form > div.qualityrating").style.display = "none";
    }
}

// Image functions for modify advertisement

/* previous button*/
let previous = document.getElementsByClassName("previous");

/* next button*/
let next = document.getElementsByClassName("next");
previous[0].style.display = "none";
next[0].style.display = "none";

/* image field*/
let image = document.getElementById("imgoutput");

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
        previous[0].style.display = "inline";
        imagecounter.style.display = "inline";
        imagecounter.style.backgroundColor = "rgba(0, 0, 0, 0.5)";
        next[0].style.display = "inline";
        image.style.display = "block";
    } else {
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

function submitReviewForm() {
    let rrId = document.querySelector("#reviewAdvertisementColumn > form > input[name='reviewId']").value;
    let communication = document.querySelector("div.communicationrating > div > input:checked").value;
    let quality = (document.querySelector("div.qualityrating").style.display == "none") ?
        null : document.querySelector("div.qualityrating > div > input:checked").value;
    $.ajax({
        type: "POST",
        url: `/Account/PostReview?ratingRequestId=${rrId}&communication=${communication}&quality=${quality}`,
        success: function (data) {
            alert(data);
            window.location.href = "/Account/MyAccount";
        },
        error: function (err) {
            if (err.statusCode == 302) {
                window.location = err.getResponseHeader("RedirectUrl");
            }
            alert(err.responseText);
        }
    })
}