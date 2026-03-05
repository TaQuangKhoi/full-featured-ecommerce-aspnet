/**
 * admin-orders.js — client-side order status management for Admin Orders pages.
 * All status updates are sent via PATCH to /api/admin/carts/{id}/status.
 * No server-side form submissions are used.
 *
 * Supports two pages:
 *   - Index:   #admin-orders-index[data-carts-url]  — status selects with data-order-id
 *   - Details: #admin-order-details[data-carts-url, data-order-id] — single select + button
 */
document.addEventListener('DOMContentLoaded', function () {

    function getCsrfToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function showAlert(type, message) {
        const successEl = document.getElementById('alert-success');
        const errorEl = document.getElementById('alert-error');
        if (successEl) successEl.classList.add('hidden');
        if (errorEl) errorEl.classList.add('hidden');
        if (type === 'success' && successEl) {
            successEl.textContent = message;
            successEl.classList.remove('hidden');
            setTimeout(() => successEl.classList.add('hidden'), 3000);
        } else if (type === 'error' && errorEl) {
            errorEl.textContent = message;
            errorEl.classList.remove('hidden');
            setTimeout(() => errorEl.classList.add('hidden'), 5000);
        }
    }

    async function patchStatus(cartsUrl, orderId, newStatus, onSuccess) {
        const url = `${cartsUrl}/${orderId}/status`;
        const res = await fetch(url, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getCsrfToken()
            },
            body: JSON.stringify({ status: newStatus })
        });

        if (res.status === 204) {
            showAlert('success', 'Order status updated successfully.');
            if (onSuccess) onSuccess();
        } else {
            const err = await res.json().catch(() => null);
            showAlert('error', (err && err.error) ? err.error : 'Failed to update status.');
        }
    }

    // ── Index page ────────────────────────────────────────────────────────────
    const indexPage = document.getElementById('admin-orders-index');
    if (indexPage) {
        const cartsUrl = indexPage.dataset.cartsUrl;

        indexPage.querySelectorAll('.status-select').forEach(select => {
            select.addEventListener('change', async function () {
                const orderId = this.dataset.orderId;
                const newStatus = this.value;
                this.disabled = true;
                try {
                    await patchStatus(cartsUrl, orderId, newStatus, null);
                } catch {
                    showAlert('error', 'Network error. Please check your connection and try again.');
                } finally {
                    this.disabled = false;
                }
            });
        });
    }

    // ── Details page ──────────────────────────────────────────────────────────
    const detailsPage = document.getElementById('admin-order-details');
    if (detailsPage) {
        const cartsUrl = detailsPage.dataset.cartsUrl;
        const orderId = detailsPage.dataset.orderId;
        const updateBtn = document.getElementById('update-status-btn');
        const statusSelect = document.getElementById('order-status-select');

        if (updateBtn && statusSelect) {
            updateBtn.addEventListener('click', async function () {
                const newStatus = statusSelect.value;
                updateBtn.disabled = true;
                try {
                    await patchStatus(cartsUrl, orderId, newStatus, null);
                } catch {
                    showAlert('error', 'Network error. Please check your connection and try again.');
                } finally {
                    updateBtn.disabled = false;
                }
            });
        }
    }
});
