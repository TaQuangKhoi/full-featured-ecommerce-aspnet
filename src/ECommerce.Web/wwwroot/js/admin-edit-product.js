/**
 * admin-edit-product.js — handles the product edit form submission via the REST API.
 * The form element must have a `data-index-url` attribute set to the products index URL.
 */
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('edit-product-form');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const alertSuccess = document.getElementById('alert-success');
        const alertError = document.getElementById('alert-error');
        const submitBtn = document.getElementById('submit-btn');

        alertSuccess.classList.add('hidden');
        alertError.classList.add('hidden');
        document.querySelectorAll('.field-error').forEach(el => el.textContent = '');

        const id = document.getElementById('Id').value;

        const priceVal = parseFloat(document.getElementById('Price').value);
        const stockVal = parseInt(document.getElementById('Stock').value, 10);
        const categoryId = document.getElementById('CategoryId').value;

        if (isNaN(priceVal) || priceVal <= 0) {
            const priceSpan = document.querySelector('.field-error[data-field="Price"]');
            if (priceSpan) priceSpan.textContent = 'Please enter a valid price greater than 0.';
            alertError.textContent = 'Please fix the validation errors above.';
            alertError.classList.remove('hidden');
            return;
        }

        if (isNaN(stockVal) || stockVal < 0) {
            const stockSpan = document.querySelector('.field-error[data-field="Stock"]');
            if (stockSpan) stockSpan.textContent = 'Please enter a valid non-negative stock quantity.';
            alertError.textContent = 'Please fix the validation errors above.';
            alertError.classList.remove('hidden');
            return;
        }

        if (!categoryId) {
            const catSpan = document.querySelector('.field-error[data-field="CategoryId"]');
            if (catSpan) catSpan.textContent = 'Please select a category.';
            alertError.textContent = 'Please fix the validation errors above.';
            alertError.classList.remove('hidden');
            return;
        }

        const payload = {
            id: id,
            name: document.getElementById('Name').value,
            description: document.getElementById('Description').value,
            price: priceVal,
            stock: stockVal,
            imageUrl: document.getElementById('ImageUrl').value,
            categoryId: categoryId,
            isActive: document.getElementById('IsActive').checked
        };

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const headers = { 'Content-Type': 'application/json' };
        if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;

        submitBtn.disabled = true;
        submitBtn.textContent = 'Saving\u2026';

        try {
            const response = await fetch(`/api/products/${id}`, {
                method: 'PUT',
                headers: headers,
                body: JSON.stringify(payload)
            });

            if (response.status === 204) {
                alertSuccess.textContent = 'Product updated successfully!';
                alertSuccess.classList.remove('hidden');
                const indexUrl = form.dataset.indexUrl || '/Admin/Products';
                setTimeout(() => { window.location.href = indexUrl; }, 1000);
                return;
            }

            const data = await response.json().catch(() => null);

            if (response.status === 400 && data?.errors) {
                Object.entries(data.errors).forEach(([field, messages]) => {
                    const span = document.querySelector(`.field-error[data-field="${field}"]`);
                    if (span) span.textContent = messages[0];
                });
                alertError.textContent = 'Please fix the validation errors above.';
            } else if (response.status === 404) {
                alertError.textContent = (data && data.error) ? data.error : 'Product not found.';
            } else {
                alertError.textContent = (data && data.error) ? data.error : 'An unexpected error occurred. Please try again.';
            }
            alertError.classList.remove('hidden');
        } catch {
            alertError.textContent = 'Network error. Please check your connection and try again.';
            alertError.classList.remove('hidden');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Update Product';
        }
    });
});
