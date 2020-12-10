var basic = document.getElementsByClassName("basic_information");
var filters = document.getElementsByClassName("filters_information");

basic[0].style.display = "block";
filters[0].style.display = "none";

function switchtab(infosection, tabname) {
    var i;
    var tabs = document.getElementsByClassName("tabs");
    var tabinfo = document.getElementsByClassName("infoblock");
    for (i = 0; i < tabs.length; i++) {
        tabinfo[i].style.display = "none";

        tabs[i].style.backgroundColor = "white";
        tabs[i].style.color = "black";
    }
    var infoshow = document.getElementsByClassName(infosection);
    infoshow[0].style.display = "block";

    var tabs = document.getElementsByClassName(tabname);
    tabs[0].style.backgroundColor = "#015e54";
    tabs[0].style.color = "white";
}

var imgcount = 1;
function imageslider() {
    var image = document.getElementById("imgoutput");
    image.src = imgList[imgcount % imgList.length];
    imgcount++;
}

