document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == "admin-group-form") {
        e.preventDefault();
        fetch("/api/group", {
            method: "POST",
            body: new FormData(form)
        }).then(r => r.json()).then(console.log);
    }
    if (form.id == "admin-product-form") {
        e.preventDefault();
        fetch("/api/product", {
            method: "POST",
            body: new FormData(form)
        }).then(r => r.json()).then(console.log);
    }
});

document.addEventListener('DOMContentLoaded', () => {
    for (let btn of document.querySelectorAll("[data-add-to-cart]")) {
        btn.addEventListener('click', addToCartClick);
    }
    for (let btn of document.querySelectorAll('[data-goto="cart"]')) {
        btn.addEventListener('click', gotoToCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-item]')) {
        btn.addEventListener('click', modifyCartItemClick);
    }
});

function modifyCartItemClick(e) {
    e.preventDefault();
    let btn = e.target.closest("[data-cart-item]");
    let cartItemId = btn.getAttribute("data-cart-item");
    let inc = btn.getAttribute("data-cart-inc") || 1;
    console.log(cartItemId, inc);

    fetch("/api/cart/" + cartItemId + "?inc=" + inc, {
        method: "PATCH"
    }).then(r => r.json()).then(j => {
        // перевірити, що відповідь успішна
        window.location.reload();
    });
}

function gotoToCartClick(e) {
    e.preventDefault();
    window.location.href = "/Shop/Cart";
}


function addToCartClick(e) {
    e.preventDefault();
    let btn = e.target.closest("[data-add-to-cart]");
    let productId = btn.getAttribute("data-add-to-cart");
    console.log(productId);

    fetch("/api/cart/" + productId + "", {
        method: "POST"
    }).then(r => r.json()).then(j => {
        // перевірити, що відповідь успішна
        window.location.reload();
    });
}