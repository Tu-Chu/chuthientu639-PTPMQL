(() => {
    const page = document.querySelector("#productPage");
    if (!page) return;

    const tableBody = document.querySelector("#productTableBody");
    const searchInput = document.querySelector("#productSearch");
    const pageSizeSelect = document.querySelector("#productPageSize");
    const paginationInfo = document.querySelector("#productPaginationInfo");
    const pageLabel = document.querySelector("#productPageLabel");
    const previousButton = document.querySelector("#previousProductPage");
    const nextButton = document.querySelector("#nextProductPage");
    const notice = document.querySelector("#productNotice");
    const form = document.querySelector("#productForm");
    const modal = document.querySelector("#productModal");
    const token = form.querySelector('input[name="__RequestVerificationToken"]');

    let currentPage = 1;
    let currentPageSize = 10;
    let searchTimer;

    async function request(url, options = {}) {
        const headers = new Headers(options.headers || {});
        if (options.body) headers.set("Content-Type", "application/json");
        if (options.method && options.method !== "GET") headers.set("RequestVerificationToken", token.value);

        const response = await fetch(url, { ...options, headers });
        const payload = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(payload.message || "Yêu cầu không thành công");
        return payload;
    }

    function showNotice(message, error = false) {
        notice.textContent = message;
        notice.className = `alert ${error ? "alert-danger" : "alert-success"}`;
    }

    async function loadProducts() {
        const url = new URL(page.dataset.listUrl, window.location.origin);
        url.searchParams.set("search", searchInput.value.trim());
        url.searchParams.set("page", currentPage);
        url.searchParams.set("pageSize", currentPageSize);
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">Đang tải dữ liệu...</td></tr>';

        try {
            const result = await request(url);
            currentPage = result.page;
            currentPageSize = result.pageSize;
            tableBody.replaceChildren();

            if (result.items.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="5" class="text-center">Không tìm thấy dữ liệu phù hợp.</td></tr>';
            } else {
                for (const product of result.items) {
                    const row = document.createElement("tr");
                    row.innerHTML = `<td>${product.id}</td><td></td><td>${Number(product.price).toLocaleString("vi-VN")}</td><td>${product.stock}</td><td></td>`;
                    row.children[1].textContent = product.name;
                    const actions = row.children[4];

                    const edit = document.createElement("button");
                    edit.type = "button";
                    edit.className = "btn btn-sm btn-outline-primary me-1";
                    edit.textContent = "Sửa";
                    edit.dataset.action = "edit";
                    edit.dataset.id = product.id;

                    const remove = document.createElement("button");
                    remove.type = "button";
                    remove.className = "btn btn-sm btn-outline-danger";
                    remove.textContent = "Xóa";
                    remove.dataset.action = "delete";
                    remove.dataset.id = product.id;
                    remove.dataset.name = product.name;
                    actions.append(edit, remove);
                    tableBody.append(row);
                }
            }

            const first = result.totalCount === 0 ? 0 : (result.page - 1) * result.pageSize + 1;
            const last = Math.min(result.page * result.pageSize, result.totalCount);
            paginationInfo.textContent = `Hiển thị ${first}-${last} trong ${result.totalCount} sản phẩm`;
            pageLabel.textContent = `Trang ${result.page} / ${Math.max(result.totalPages, 1)}`;
            previousButton.disabled = result.page <= 1;
            nextButton.disabled = result.page >= result.totalPages;
        } catch (error) {
            showNotice(error.message, true);
        }
    }

    function openForm() {
        form.reset();
        document.querySelector("#ProductId").value = "0";
        document.querySelector("#modalTitle").textContent = "Thêm sản phẩm";
        document.querySelector("#formError").textContent = "";
        modal.classList.add("show");
        modal.style.display = "block";
    }

    function closeForm() {
        modal.classList.remove("show");
        modal.style.display = "none";
    }

    document.querySelector("#btnOpenCreate").addEventListener("click", openForm);
    modal.querySelectorAll("[data-bs-dismiss='modal']").forEach(button => button.addEventListener("click", closeForm));

    tableBody.addEventListener("click", async event => {
        const button = event.target.closest("button[data-action]");
        if (!button) return;

        if (button.dataset.action === "edit") {
            try {
                const url = new URL(page.dataset.itemUrl, window.location.origin);
                url.searchParams.set("id", button.dataset.id);
                const product = await request(url);
                document.querySelector("#ProductId").value = product.id;
                document.querySelector("#Name").value = product.name;
                document.querySelector("#Price").value = product.price;
                document.querySelector("#Stock").value = product.stock;
                document.querySelector("#modalTitle").textContent = "Sửa sản phẩm";
                document.querySelector("#formError").textContent = "";
                modal.classList.add("show");
                modal.style.display = "block";
            } catch (error) {
                showNotice(error.message, true);
            }
        } else if (confirm(`Bạn có chắc muốn xóa "${button.dataset.name}"?`)) {
            const url = new URL(page.dataset.deleteUrl, window.location.origin);
            url.searchParams.set("id", button.dataset.id);
            try {
                const result = await request(url, { method: "DELETE" });
                showNotice(result.message || "Đã xóa sản phẩm.");
                await loadProducts();
            } catch (error) {
                showNotice(error.message, true);
            }
        }
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (!form.reportValidity()) return;

        const id = document.querySelector("#ProductId").value;
        const product = {
            id: Number(id),
            name: document.querySelector("#Name").value.trim(),
            price: Number(document.querySelector("#Price").value),
            stock: Number(document.querySelector("#Stock").value)
        };
        const url = id !== "0"
            ? new URL(page.dataset.updateUrl, window.location.origin)
            : page.dataset.createUrl;
        if (id !== "0") url.searchParams.set("id", id);

        try {
            const result = await request(url, {
                method: id !== "0" ? "PUT" : "POST",
                body: JSON.stringify(product)
            });
            closeForm();
            if (id === "0") currentPage = 1;
            showNotice(result.message || "Đã lưu sản phẩm.");
            await loadProducts();
        } catch (error) {
            document.querySelector("#formError").textContent = error.message;
        }
    });

    searchInput.addEventListener("input", () => {
        currentPage = 1;
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadProducts, 300);
    });

    pageSizeSelect.addEventListener("change", () => {
        currentPage = 1;
        currentPageSize = Number(pageSizeSelect.value);
        loadProducts();
    });

    previousButton.addEventListener("click", () => {
        if (currentPage > 1) { currentPage--; loadProducts(); }
    });

    nextButton.addEventListener("click", () => { currentPage++; loadProducts(); });

    loadProducts();
})();
