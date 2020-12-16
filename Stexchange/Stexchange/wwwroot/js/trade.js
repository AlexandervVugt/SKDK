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
            if (event.target.value > 7) {
                console.log("bigger " + event.target.value);
                event.target.step = 7;
                innerspan.innerText = Math.floor(event.target.value / 7);
                outerspan.innerHTML = " weken";
                outerspan.prepend(innerspan);
            } else {
                console.log("smaller" + event.target.value);
                console.log(event.target);
                console.log(innerspan);
                event.target.step = 1;
                innerspan.innerText = event.target.value;
                outerspan.innerText = " dagen";
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
Object.keys(localStorage).forEach((key) => {
    let checked = JSON.parse(localStorage.getItem(key));
    if (checked == true) {
        document.getElementById(key).checked = true;
        document.getElementById(key).parentElement.parentElement.className = "nested expanded";
    };
    localStorage.removeItem(key);
});

// Stores checkbox values in localstorage
function onClick() {
    var checkboxinputs = document.getElementsByTagName("input");
    for (var i = 0; i < checkboxinputs.length; i++) {
        if (checkboxinputs[i].type.toLowerCase() == "checkbox") {
            localStorage.setItem(checkboxinputs[i].id, checkboxinputs[i].checked);
        }
    }
}



