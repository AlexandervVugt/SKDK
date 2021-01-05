function showRequestForm(form) {
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