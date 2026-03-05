/**
 * admin-cart.js — client-side cart management for the Admin Cart page.
 * All data is loaded via fetch to /api/admin/carts; no server-side rendering.
 * The page element must have a data-carts-url attribute set to the admin carts API URL.
 */
document.addEventListener('DOMContentLoaded', function () {
    const page = document.getElementById('admin-cart-page');
    if (!page) return;

    const cartsUrl = page.dataset.cartsUrl;
    const statusFilter = document.getElementById('status-filter');
    const alertSuccess = document.getElementById('alert-success');
    const alertError = document.getElementById('alert-error');
    const loadingIndicator = document.getElementById('loading-indicator');
    const tableWrapper = document.getElementById('carts-table-wrapper');
    const tbody = document.getElementById('carts-tbody');
    const emptyState = document.getElementById('empty-state');
    const paginationEl = document.getElementById('pagination');

    let currentPage = 1;
    const pageSize = 20;

    function getCsrfToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function showAlert(type, message) {
        alertSuccess.classList.add('hidden');
        alertError.classList.add('hidden');
        if (type === 'success') {
            alertSuccess.textContent = message;
            alertSuccess.classList.remove('hidden');
            setTimeout(() => alertSuccess.classList.add('hidden'), 3000);
        } else {
            alertError.textContent = message;
            alertError.classList.remove('hidden');
            setTimeout(() => alertError.classList.add('hidden'), 5000);
        }
    }

    function buildStatusSelect(orderId, currentStatus) {
        const statuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];
        const options = statuses
            .map(s => `<option value="${s}"${s === currentStatus ? ' selected' : ''}>${s}</option>`)
            .join('');
        return `<select data-order-id="${orderId}"
                        class="status-select px-3 py-1 rounded-lg text-sm border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-indigo-500">
                    ${options}
                </select>`;
    }

    function renderItems(items) {
        if (!items || items.length === 0) return '<span class="text-gray-400 text-xs">—</span>';
        return items
            .map(i => `<span class="inline-block text-xs bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded px-2 py-0.5 mr-1 mb-1">${escapeHtml(i.productName)} ×${i.quantity}</span>`)
            .join('');
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function renderRows(orders) {
        if (orders.length === 0) return;
        tbody.innerHTML = orders.map(order => `
            <tr class="hover:bg-gray-50 dark:hover:bg-gray-750" data-id="${order.id}">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-indigo-600 dark:text-indigo-400">
                    #${order.id.substring(0, 8).toUpperCase()}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                    ${escapeHtml(order.customerEmail)}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                    ${new Date(order.orderDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    ${buildStatusSelect(order.id, order.status)}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white text-right">
                    $${order.total.toFixed(2)}
                </td>
                <td class="px-6 py-4 text-sm">
                    ${renderItems(order.items)}
                </td>
            </tr>
        `).join('');

        tbody.querySelectorAll('.status-select').forEach(select => {
            select.addEventListener('change', handleStatusChange);
        });
    }

    function renderPagination(data) {
        if (data.totalPages <= 1) {
            paginationEl.classList.add('hidden');
            return;
        }
        paginationEl.classList.remove('hidden');
        paginationEl.innerHTML = '';
        for (let i = 1; i <= data.totalPages; i++) {
            const btn = document.createElement('button');
            btn.textContent = i;
            btn.className = `px-4 py-2 rounded-lg text-sm font-medium transition ${i === data.page
                ? 'bg-indigo-600 text-white'
                : 'bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600'}`;
            btn.addEventListener('click', () => {
                currentPage = i;
                loadCarts();
            });
            paginationEl.appendChild(btn);
        }
    }

    async function loadCarts() {
        const filterValue = statusFilter.value;
        let url = `${cartsUrl}?page=${currentPage}&pageSize=${pageSize}`;

        loadingIndicator.classList.remove('hidden');
        tableWrapper.classList.add('hidden');
        emptyState.classList.add('hidden');
        paginationEl.classList.add('hidden');

        try {
            const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
            if (!res.ok) {
                const err = await res.json().catch(() => null);
                showAlert('error', (err && err.error) ? err.error : 'Failed to load carts.');
                loadingIndicator.classList.add('hidden');
                return;
            }

            const data = await res.json();

            const filteredItems = filterValue
                ? data.items.filter(o => o.status === filterValue)
                : data.items;

            loadingIndicator.classList.add('hidden');

            if (filteredItems.length === 0) {
                emptyState.classList.remove('hidden');
            } else {
                tbody.innerHTML = '';
                renderRows(filteredItems);
                tableWrapper.classList.remove('hidden');
                renderPagination(data);
            }
        } catch {
            loadingIndicator.classList.add('hidden');
            showAlert('error', 'Network error. Please check your connection and try again.');
        }
    }

    async function handleStatusChange(e) {
        const select = e.target;
        const orderId = select.dataset.orderId;
        const newStatus = select.value;
        const url = `${cartsUrl}/${orderId}/status`;

        select.disabled = true;

        try {
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
            } else {
                const err = await res.json().catch(() => null);
                showAlert('error', (err && err.error) ? err.error : 'Failed to update status.');
                await loadCarts();
            }
        } catch {
            showAlert('error', 'Network error. Please check your connection and try again.');
            await loadCarts();
        } finally {
            select.disabled = false;
        }
    }

    statusFilter.addEventListener('change', () => {
        currentPage = 1;
        loadCarts();
    });

    loadCarts();
});
