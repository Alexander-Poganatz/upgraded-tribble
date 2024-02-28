// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Remove js-off so elements with js-on will show when Javascript is available
document.body.classList.remove("js-off");

{
    let genericHTMXModal = document.getElementById("modal1");
    if (genericHTMXModal != null) {
        genericHTMXModal.checked = false;
    }
}

// For transaction page
function OnPageNumSelectChange(event) {
    console.log(event.value);
    console.log(window.location);
    window.location.href = window.location.origin + window.location.pathname + "?page=" + event.value;
}

{
    let selectElement = document.getElementById("OnPageNumSelect");
    if (selectElement != null) {
        selectElement.addEventListener("change", OnPageNumSelectChange);
    }
}
