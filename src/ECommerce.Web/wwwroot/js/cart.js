/**
 * cart.js — Alpine.js store components for shopping cart functionality.
 * Registered as named component functions so views stay logic-free.
 */

/**
 * cartStore — global cart state, mounted on <body> via x-data="cartStore()" in _Layout.
 * Handles add/remove/update/clear and persists to localStorage.
 */
function cartStore() {
    return {
        cart: [],
        showCheckout: false,

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
        }
    };
}

/**
 * checkoutForm — Alpine component for the checkout form in Cart/Index.cshtml.
 * Reads cart state from the parent cartStore via Alpine.$data($root).
 */
function checkoutForm() {
    return {
        address: '',
        submitting: false,

        init() {
            // Inherits cart and clearCart from parent cartStore in Alpine 3
        },

        async placeOrder() {
            this.submitting = true;
            try {
                // Accessing this.cart from parent scope
                const cart = this.cart || [];
                const res = await fetch('/Cart/Checkout', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ items: cart, shippingAddress: this.address })
                });
                const data = await res.json();
                this.submitting = false;

                if (res.ok && data.success) {
                    this.clearCart();
                    await Swal.fire({ icon: 'success', title: 'Order Placed!', text: 'Your order has been placed successfully.' });
                    window.location.href = '/Orders';
                } else if (res.status === 401) {
                    await Swal.fire({ icon: 'warning', title: 'Login Required', text: data.message || 'You must be logged in to place an order.' });
                    window.location.href = '/Account/Login?ReturnUrl=/Cart';
                } else {
                    Swal.fire({ icon: 'error', title: 'Error', text: data.message || 'Failed to place order.' });
                }
            } catch (e) {
                this.submitting = false;
                Swal.fire({ icon: 'error', title: 'Error', text: 'Something went wrong. Please try again.' });
            }
        }
    };
}
