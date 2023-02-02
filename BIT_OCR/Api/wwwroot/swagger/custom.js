(function () {
    window.addEventListener("load", function () {
        setTimeout(function () {
            var logo = document.getElementsByClassName('link');
            var title = document.createElement("h6");
            title.textContent = "BiT-Central";

            logo[0].children[0].alt = "Logo";
            logo[0].children[0].src = "/swagger/logo.png";
            logo[0].appendChild(title);
        });
    });
})();