// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Remove js-off so elements with js-on will show when Javascript is available
document.body.classList.remove("js-off");

{
    function replaceModalBodyWithSpinner() {
        let modalBody = document.getElementById("modalBody");

        while (modalBody.childElementCount > 1) {
            modalBody.removeChild(firstChild)
        }
        let article = document.createElement("article")
        article.appendChild(document.createElement("header"))
        let spinnerChildParent = document.createElement("section")
        article.appendChild(spinnerChildParent)
        spinnerChild = document.createElement("div")
        spinnerChildParent.appendChild(spinnerChild)
        article.appendChild(document.createElement("footer"))
        spinnerChild.classList.add("spinner", "p1", "m1")
        if (modalBody.firstChild) {
            modalBody.replaceChild(article, modalBody.firstChild)
        } else {
            modalBody.appendChild(article);
        }
    }

    let genericHTMXModal = document.getElementById("modal1");
    if (genericHTMXModal != null) {
        genericHTMXModal.checked = false;

        genericHTMXModal.addEventListener("change", function (event) {
            if (event.target.checked != true) {
                window.setTimeout(replaceModalBodyWithSpinner, 500)
            }
        })

        replaceModalBodyWithSpinner()
    }
}

// For transaction page
function OnPageNumSelectChange(event) {
    let value = event.target.value;
    window.location.href = window.location.origin + window.location.pathname + "?page=" + value;
}

{
    let selectElement = document.getElementById("OnPageNumSelect");
    if (selectElement != null) {
        selectElement.addEventListener("change", OnPageNumSelectChange);
    }
}

{
    let modalCheckmark = document.getElementById("modal1");

    if (modalCheckmark != null) {
        
    }
}
