(() => {
    const page = document.querySelector("#employeePage");
    const tableBody = document.querySelector("#employeeTableBody");
    const dialog = document.querySelector("#employeeDialog");
    const form = document.querySelector("#employeeForm");
    const notice = document.querySelector("#employeeNotice");
    const formError = document.querySelector("#employeeFormError");
    const saveButton = document.querySelector("#saveEmployeeButton");
    const searchInput = document.querySelector("#employeeSearch");
    const pageSizeSelect = document.querySelector("#employeePageSize");
    const paginationInfo = document.querySelector("#employeePaginationInfo");
    const pageLabel = document.querySelector("#employeePageLabel");
    const previousPageButton = document.querySelector("#previousPageButton");
    const nextPageButton = document.querySelector("#nextPageButton");
    const token = form.querySelector('input[name="__RequestVerificationToken"]');
    const currency = new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0
    });
    let currentPage = 1;
    let currentPageSize = Number(pageSizeSelect.value);
    let searchTimer;
    let listRequestSequence = 0;

    async function request(url, options = {}) {
        const headers = new Headers(options.headers || {});

        if (options.body) {
            headers.set("Content-Type", "application/json");
        }

        if (options.method && options.method !== "GET") {
            headers.set("RequestVerificationToken", token.value);
        }

        const response = await fetch(url, { ...options, headers });
        const responseText = await response.text();
        let payload = {};

        if (responseText) {
            try {
                payload = JSON.parse(responseText);
            } catch {
                payload = { message: responseText };
            }
        }

        if (!response.ok) {
            const error = new Error(payload.message || "Yêu cầu không thành công.");
            error.payload = payload;
            throw error;
        }

        return payload;
    }

    function showNotice(message, kind = "success") {
        notice.textContent = message;
        notice.dataset.kind = kind;
        notice.hidden = false;
    }

    function clearFieldErrors() {
        form.querySelectorAll("[data-error-for]").forEach((element) => {
            element.textContent = "";
        });
        formError.textContent = "";
    }

    function showValidationErrors(error) {
        const errors = error.payload?.errors;

        if (errors) {
            for (const [field, messages] of Object.entries(errors)) {
                const errorElement = form.querySelector(`[data-error-for="${field.toLowerCase()}"]`);
                if (errorElement) {
                    errorElement.textContent = messages.join(" ");
                }
            }
        }

        formError.textContent = error.message;
    }

    function createCell(value) {
        const cell = document.createElement("td");
        cell.textContent = value;
        return cell;
    }

    function renderEmployees(result) {
        const { items, totalCount, page, pageSize, totalPages } = result;
        tableBody.replaceChildren();
        currentPage = page;
        currentPageSize = pageSize;

        if (items.length === 0) {
            const row = document.createElement("tr");
            const emptyMessage = searchInput.value.trim()
                ? "Không tìm thấy nhân viên phù hợp với từ khóa."
                : "Chưa có nhân viên. Hãy thêm bản ghi đầu tiên.";
            const cell = createCell(emptyMessage);
            cell.colSpan = 6;
            row.append(cell);
            tableBody.append(row);
        } else {
            for (const employee of items) {
                const row = document.createElement("tr");
                row.append(
                    createCell(employee.id),
                    createCell(employee.fullName),
                    createCell(employee.email),
                    createCell(employee.department),
                    createCell(currency.format(employee.salary))
                );

                const actions = document.createElement("td");
                const editButton = document.createElement("button");
                editButton.className = "employee-button";
                editButton.type = "button";
                editButton.textContent = "Sửa";
                editButton.dataset.action = "edit";
                editButton.dataset.id = employee.id;

                const deleteButton = document.createElement("button");
                deleteButton.className = "employee-button employee-button-danger";
                deleteButton.type = "button";
                deleteButton.textContent = "Xóa";
                deleteButton.dataset.action = "delete";
                deleteButton.dataset.id = employee.id;
                deleteButton.dataset.name = employee.fullName;

                actions.append(editButton, deleteButton);
                row.append(actions);
                tableBody.append(row);
            }
        }

        const firstItem = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
        const lastItem = Math.min(page * pageSize, totalCount);
        paginationInfo.textContent = `Hiển thị ${firstItem}-${lastItem} trong ${totalCount} nhân viên`;
        pageLabel.textContent = `Trang ${page} / ${Math.max(totalPages, 1)}`;
        previousPageButton.disabled = page <= 1;
        nextPageButton.disabled = page >= totalPages;
    }

    async function loadEmployees() {
        const requestSequence = ++listRequestSequence;
        const url = new URL(page.dataset.listUrl, window.location.origin);
        url.searchParams.set("search", searchInput.value.trim());
        url.searchParams.set("page", currentPage);
        url.searchParams.set("pageSize", currentPageSize);
        const loadingRow = document.createElement("tr");
        const loadingCell = createCell("Đang tải dữ liệu...");
        loadingCell.colSpan = 6;
        loadingRow.append(loadingCell);
        tableBody.replaceChildren(loadingRow);

        try {
            const result = await request(url);
            if (requestSequence === listRequestSequence) {
                renderEmployees(result);
            }
        } catch (error) {
            if (requestSequence !== listRequestSequence) {
                return;
            }
            tableBody.replaceChildren();
            showNotice(error.message, "error");
        }
    }

    function openCreateDialog() {
        form.reset();
        form.elements.id.value = "";
        clearFieldErrors();
        document.querySelector("#employeeDialogTitle").textContent = "Thêm nhân viên";
        dialog.showModal();
        form.elements.fullName.focus();
    }

    async function openEditDialog(id) {
        try {
            const url = new URL(page.dataset.itemUrl, window.location.origin);
            url.searchParams.set("id", id);
            const employee = await request(url);

            form.reset();
            form.elements.id.value = employee.id;
            form.elements.fullName.value = employee.fullName;
            form.elements.email.value = employee.email;
            form.elements.department.value = employee.department;
            form.elements.salary.value = employee.salary;
            clearFieldErrors();
            document.querySelector("#employeeDialogTitle").textContent = "Sửa nhân viên";
            dialog.showModal();
            form.elements.fullName.focus();
        } catch (error) {
            showNotice(error.message, "error");
        }
    }

    document.querySelector("#addEmployeeButton").addEventListener("click", openCreateDialog);

    document.querySelectorAll("[data-close-dialog]").forEach((button) => {
        button.addEventListener("click", () => dialog.close());
    });

    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) {
            dialog.close();
        }
    });

    tableBody.addEventListener("click", async (event) => {
        const button = event.target.closest("button[data-action]");
        if (!button) {
            return;
        }

        if (button.dataset.action === "edit") {
            await openEditDialog(button.dataset.id);
            return;
        }

        if (window.confirm(`Bạn có chắc muốn xóa nhân viên "${button.dataset.name}"?`)) {
            const url = new URL(page.dataset.deleteUrl, window.location.origin);
            url.searchParams.set("id", button.dataset.id);

            try {
                const result = await request(url, { method: "DELETE" });
                showNotice(result.message || "Đã xóa nhân viên.");
                await loadEmployees();
            } catch (error) {
                showNotice(error.message, "error");
            }
        }
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        clearFieldErrors();

        if (!form.reportValidity()) {
            return;
        }

        const id = form.elements.id.value;
        const employee = {
            fullName: form.elements.fullName.value.trim(),
            email: form.elements.email.value.trim(),
            department: form.elements.department.value.trim(),
            salary: Number(form.elements.salary.value)
        };
        const url = id
            ? new URL(page.dataset.updateUrl, window.location.origin)
            : page.dataset.createUrl;
        const options = {
            method: id ? "PUT" : "POST",
            body: JSON.stringify(employee)
        };

        if (id) {
            url.searchParams.set("id", id);
        }

        saveButton.disabled = true;

        try {
            const result = await request(url, options);
            if (!id) {
                currentPage = 1;
            }
            dialog.close();
            showNotice(result.message || "Đã lưu nhân viên.");
            await loadEmployees();
        } catch (error) {
            showValidationErrors(error);
        } finally {
            saveButton.disabled = false;
        }
    });

    searchInput.addEventListener("input", () => {
        currentPage = 1;
        listRequestSequence += 1;
        window.clearTimeout(searchTimer);
        searchTimer = window.setTimeout(loadEmployees, 300);
    });

    pageSizeSelect.addEventListener("change", () => {
        currentPage = 1;
        currentPageSize = Number(pageSizeSelect.value);
        loadEmployees();
    });

    previousPageButton.addEventListener("click", () => {
        if (currentPage > 1) {
            currentPage -= 1;
            loadEmployees();
        }
    });

    nextPageButton.addEventListener("click", () => {
        currentPage += 1;
        loadEmployees();
    });

    loadEmployees();
})();