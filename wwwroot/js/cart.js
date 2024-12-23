// wwwroot/js/cart.js
class Cart {
    static async addToCart(bookId, format, isBorrow) {
        try {
            const response = await fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ bookId, format, isBorrow })
            });

            const result = await response.json();
            if (result.success) {
                this.updateCartCounter(result.cartItemsCount);
                this.showNotification('הפריט נוסף לעגלה בהצלחה', 'success');
            } else {
                this.showNotification(result.message || 'אירעה שגיאה בהוספת הפריט לעגלה', 'error');
            }
        } catch (error) {
            this.showNotification('אירעה שגיאה בהוספת הפריט לעגלה', 'error');
        }
    }

    static updateCartCounter(count) {
        const counter = document.getElementById('cart-counter');
        if (counter) {
            counter.textContent = count;
            counter.classList.remove('hidden');
        }
    }

    static showNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `fixed bottom-4 right-4 p-4 rounded-lg ${
            type === 'success' ? 'bg-green-500' : 'bg-red-500'
        } text-white`;
        notification.textContent = message;

        document.body.appendChild(notification);
        setTimeout(() => {
            notification.remove();
        }, 3000);
    }
}

// Credit card validation
class PaymentValidator {
    static validateCard(number) {
        return this.luhnCheck(number.replace(/\s/g, ''));
    }

    static luhnCheck(cardNumber) {
        if (!cardNumber) return false;
        let nCheck = 0;
        let bEven = false;

        for (let n = cardNumber.length - 1; n >= 0; n--) {
            let cDigit = cardNumber.charAt(n);
            let nDigit = parseInt(cDigit, 10);

            if (bEven) {
                if ((nDigit *= 2) > 9) 
                    nDigit -= 9;
            }

            nCheck += nDigit;
            bEven = !bEven;
        }

        return (nCheck % 10) == 0;
    }

    static formatCardNumber(input) {
        let value = input.value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
        let matches = value.match(/\d{4,16}/g);
        let match = matches && matches[0] || '';
        let parts = [];

        for (let i = 0; i < match.length; i += 4) {
            parts.push(match.substring(i, i + 4));
        }

        if (parts.length) {
            input.value = parts.join(' ');
        }
    }

    static validateExpiryDate(month, year) {
        const currentDate = new Date();
        const currentYear = currentDate.getFullYear();
        const currentMonth = currentDate.getMonth() + 1;

        const expYear = parseInt(year);
        const expMonth = parseInt(month);

        if (expYear < currentYear || 
            (expYear === currentYear && expMonth < currentMonth)) {
            return false;
        }

        return true;
    }
}