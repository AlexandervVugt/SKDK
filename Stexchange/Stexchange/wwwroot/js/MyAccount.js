﻿document.getElementById("accountsettingsButton").style.backgroundColor = "#465D43";
document.getElementById("accountsettingsButton").style.color = "white";

function showRequestForm(form, buttonId) {
    let requestform = document.getElementById(form);

    let myadvertisementColumn = document.getElementById("myadvertisementsColumn");
    let settingsColumn = document.getElementById("mySettingsColumn");
    let ratingColumn = document.getElementById("ratingreviewColumn");
    myadvertisementColumn.style.display = "none";
    settingsColumn.style.display = "none";
    ratingColumn.style.display = "none";
    if (form == "requestForm") {
        myadvertisementColumn.style.display = "block";
        requestform.style.display = "block";
    } else {
        requestform.style.display = "block";
    }

    toggleActive(buttonId);
}

function closeRequestForm(form) {
    let myadvertisementColumn = document.getElementById("myadvertisementsColumn");
    let requestform = document.getElementById(form);
    requestform.style.display = "none";
    if (form == 'mySettingsColumn') {
        myadvertisementColumn.style.display = "block";
    }
    toggleActive("accountsettingsButton");
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