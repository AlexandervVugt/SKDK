var spanEl = document.getElementsByClassName("caret");

for (var i = 0; i < spanEl.length; i++) {
    spanEl[i].addEventListener("click", function () {
        this.parentElement.querySelector(".nested").classList.toggle("expanded");
        this.classList.toggle("rotated");
    });
}

var sortRangeTogglers = document.getElementsByClassName("sort-range-toggle");
var sortRange = document.getElementsByClassName("sort-range");
var displays = document.getElementsByClassName("sort-range-display");

for (var i = 0; i < sortRange.length; i++) {
    sortRangeTogglers[i].addEventListener("click", togglerDecorator(i));
    sortRange[i].addEventListener("input", (event) => {
        let outerspan = document.getElementById(event.target.getAttribute("id") + "_amount");
        let innerspan = outerspan.childNodes[0];
        if (event.target.getAttribute("id") == "recent") {
            if (event.target.value > 6) {
                event.target.step = 7;
                innerspan.innerText = Math.floor(event.target.value / 7);
                innerspan.innerText == 1 ? outerspan.innerText = " week" : outerspan.innerText = " weken";
                outerspan.prepend(innerspan);
            } else {
                event.target.step = 1;
                innerspan.innerText = event.target.value;
                innerspan.innerText == 1 ? outerspan.innerText = " dag" : outerspan.innerText = " dagen";
                outerspan.prepend(innerspan);
            }
        } else {
            innerspan.innerText = event.target.value;
        }
    });
}

function togglerDecorator(j) {
    return function () {
        sortRange[j].disabled = !sortRange[j].disabled;
        displays[j].hidden = !displays[j].hidden;
    };
}

// Loops through keys
// sessionStorage automatically clears the data in storage after closing window or tab
Object.keys(sessionStorage).forEach((key) => {
    if (key == "searchbar") {
        // Get saved data from sessionStorage
        document.getElementById(key).value = sessionStorage.getItem(key);
    } else if (key == "distance" || key == "recent" || key == "rating") {
        // Adds value to toggle and enables toggle if checkbox is checked
        if (sessionStorage.getItem(key + "_toggle") == "true") {
            document.getElementById(key).parentElement.parentElement.className = "nested expanded";
            document.getElementById(key).parentElement.parentElement.previousElementSibling.previousElementSibling.className = "caret rotated";
            document.getElementById(key).value = sessionStorage.getItem(key);
            document.getElementById(key).disabled = false;
        }
    } else if (key == "distance_amount" || key == "recent_amount" || key == "rating_amount") {
        // Adds and shows text between tags if checkbox is checked
        if (sessionStorage.getItem(key.replace("_amount", "_toggle")) == "true") {
            document.getElementById(key).innerHTML = sessionStorage.getItem(key);
            document.getElementById(key).hidden = false;
        }
    } else {
        // Checks checkboxes if they are checked
        let checked = JSON.parse(sessionStorage.getItem(key));
        if (checked == true) {
            document.getElementById(key).checked = true;
            document.getElementById(key).parentElement.parentElement.className = "nested expanded";
            document.getElementById(key).parentElement.parentElement.previousElementSibling.previousElementSibling.className = "caret rotated";
        }
    }
});

// Stores checkbox values in localstorage
function onClick() {
    var checkboxinputs = document.getElementsByTagName("input");
    // Loops through all input elements
    for (var i = 0; i < checkboxinputs.length; i++) {
        // Save data to sessionStorage
        if (checkboxinputs[i].type.toLowerCase() == "checkbox") {
            sessionStorage.setItem(checkboxinputs[i].id, checkboxinputs[i].checked);
        }
        if (checkboxinputs[i].type.toLowerCase() == "range") {
            sessionStorage.setItem(checkboxinputs[i].id, checkboxinputs[i].value);
            sessionStorage.setItem(checkboxinputs[i].nextElementSibling.id, checkboxinputs[i].nextElementSibling.innerHTML);
        }
    }
    var searchinput = document.getElementById("searchbar");
    sessionStorage.setItem(searchinput.id, searchinput.value);
}




