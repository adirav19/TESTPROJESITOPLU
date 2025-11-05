// wwwroot/js/editableTable.js
document.addEventListener("DOMContentLoaded", () => {
    const tables = document.querySelectorAll("table[data-editable='true']");

    tables.forEach(table => {
        table.addEventListener("click", async e => {
            const cell = e.target.closest("td[data-field]");
            if (!cell || cell.querySelector("input")) return; // zaten edit modundaysa çık

            const oldValue = cell.innerText.trim();
            const field = cell.dataset.field;
            const row = cell.closest("tr");
            const key = row.dataset.key; // cari kod, stok kodu vs.
            const input = document.createElement("input");

            input.value = oldValue;
            input.className = "form-control form-control-sm";
            cell.innerHTML = "";
            cell.appendChild(input);
            input.focus();

            // ESC -> iptal, ENTER -> kaydet
            input.addEventListener("keydown", async ev => {
                if (ev.key === "Escape") {
                    cell.innerHTML = oldValue;
                }
                if (ev.key === "Enter") {
                    const newValue = input.value.trim();
                    if (newValue !== oldValue) {
                        cell.innerHTML = `<span class='text-secondary'>⏳</span>`;
                        const res = await fetch(`/TestApi/UpdateField`, {
                            method: "PUT",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify({
                                key: key,
                                field: field,
                                value: newValue
                            })
                        });
                        if (res.ok) {
                            cell.innerHTML = newValue;
                        } else {
                            alert("Güncelleme başarısız!");
                            cell.innerHTML = oldValue;
                        }
                    } else {
                        cell.innerHTML = oldValue;
                    }
                }
            });
        });
    });
});
