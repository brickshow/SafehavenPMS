// Show toast if TempData has a message
(function () {
    var message = '@TempData["ToastMessage"]';
    var type = '@TempData["ToastType"]';

    if (message) {
        const toastContainer = document.getElementById('toast-container');

        const toastEl = document.createElement('div');
        toastEl.className = `toast align-items-center text-white border-0 ${type === "success" ? "bg-success" : "bg-danger"}`;
        toastEl.role = 'alert';
        toastEl.ariaLive = 'assertive';
        toastEl.ariaAtomic = 'true';
        toastEl.innerHTML = `
                    <div class="d-flex">
                        <div class="toast-body">${message}</div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                `;

        toastContainer.appendChild(toastEl);

        // Show toast
        const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
        bsToast.show();

        // Remove toast after hidden
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    }
})();