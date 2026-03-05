/**
 * cart.js — Alpine.js store components for shopping cart functionality.
 * Registered as named component functions so views stay logic-free.
 */

/**
 * cartStore — global cart state, mounted on <body> via x-data="cartStore()" in _Layout.
 * Handles add/remove/update/clear and persists to localStorage.
 * Also contains checkout form state so form is in the same Alpine scope (no nested x-data needed).
 */
function cartStore() {
    return {
        cart: [],
        showCheckout: false,

        // Checkout form state
        checkoutAddress: '',
        checkoutSubmitting: false,

        get cartCount() {
            return this.cart.reduce((sum, item) => sum + item.quantity, 0);
        },

        loadCart() {
            try {
                this.cart = JSON.parse(localStorage.getItem('cart') || '[]');
            } catch {
                this.cart = [];
            }
        },

        saveCart() {
            localStorage.setItem('cart', JSON.stringify(this.cart));
        },

        addToCart(productId, name, price, qty = 1) {
            const isAuthenticated = document.querySelector('meta[name="user-authenticated"]')?.content === 'true';
            if (!isAuthenticated) {
                Swal.fire({
                    icon: 'info',
                    title: 'Login Required',
                    html: 'You need to be logged in to add items to your cart.',
                    confirmButtonText: '🔑 Login',
                    confirmButtonColor: '#4f46e5',
                    showDenyButton: true,
                    denyButtonText: '✨ Register',
                    denyButtonColor: '#6366f1',
                    showCancelButton: true,
                    cancelButtonText: 'Cancel',
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = '/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
                    } else if (result.isDenied) {
                        window.location.href = '/Account/Register';
                    }
                });
                return;
            }
            const existing = this.cart.find(i => i.productId === productId);
            if (existing) {
                existing.quantity += qty;
            } else {
                this.cart.push({ productId, name, price, quantity: qty });
            }
            this.saveCart();
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: `${name} added to cart`,
                showConfirmButton: false,
                timer: 1500,
                timerProgressBar: true
            });
        },

        removeFromCart(productId) {
            this.cart = this.cart.filter(i => i.productId !== productId);
            this.saveCart();
        },

        updateQuantity(productId, qty) {
            const item = this.cart.find(i => i.productId === productId);
            if (item) {
                item.quantity = Math.max(1, parseInt(qty) || 1);
                this.saveCart();
            }
        },

        get cartTotal() {
            return this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        },

        clearCart() {
            this.cart = [];
            this.saveCart();
        },

        async placeOrder() {
            this.checkoutSubmitting = true;
            try {
                const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                const headers = { 'Content-Type': 'application/json' };
                if (antiForgeryToken) {
                    headers['RequestVerificationToken'] = antiForgeryToken;
                }

                const res = await fetch('/api/orders', {
                    method: 'POST',
                    headers,
                    body: JSON.stringify({ items: this.cart, shippingAddress: this.checkoutAddress })
                });
                const data = await res.json();
                this.checkoutSubmitting = false;

                if (res.ok && data.success) {
                    this.clearCart();
                    this.showCheckout = false;
                    await Swal.fire({ icon: 'success', title: 'Order Placed!', text: 'Your order has been placed successfully.' });
                    window.location.href = '/Orders';
                } else if (res.status === 401) {
                    await Swal.fire({ icon: 'warning', title: 'Login Required', text: data.message || 'You must be logged in to place an order.' });
                    window.location.href = '/Account/Login?ReturnUrl=/Cart';
                } else {
                    Swal.fire({ icon: 'error', title: 'Error', text: data.message || 'Failed to place order.' });
                }
            } catch (e) {
                this.checkoutSubmitting = false;
                Swal.fire({ icon: 'error', title: 'Error', text: 'Something went wrong. Please try again.' });
            }
        }
    };
}
