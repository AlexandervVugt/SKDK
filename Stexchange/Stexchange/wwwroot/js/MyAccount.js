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
}

function submitForm(form) {
    let requestform = document.getElementById(form);
    closeRequestForm(form);
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

