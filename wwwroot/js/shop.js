﻿document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == "admin-group-form") {
        e.preventDefault();
        fetch("/api/group", {
            method: "POST",
            body: new FormData(form)
        }).then(r => r.json()).then(console.log);
    }
});