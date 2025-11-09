(function () {
    'use strict';

    function ready(callback) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback, { once: true });
        } else {
            callback();
        }
    }

    function setupConfirmationDialogs() {
        const forms = document.querySelectorAll('form[data-confirm-form]');
        forms.forEach(form => {
            form.addEventListener('submit', event => {
                const message = form.getAttribute('data-confirm-message') || 'Deseja continuar?';
                if (!window.confirm(message)) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            });
        });
    }

    function setupLoadingState() {
        const form = document.querySelector('form[data-loading-form]');
        if (!form) {
            return;
        }

        const submitButton = form.querySelector('[data-loading-text]');
        if (!submitButton) {
            return;
        }

        form.addEventListener('submit', () => {
            if (submitButton.disabled) {
                return;
            }

            const loadingText = submitButton.getAttribute('data-loading-text') || 'Processando...';
            submitButton.dataset.originalContent = submitButton.innerHTML;
            submitButton.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>${loadingText}`;
            submitButton.setAttribute('aria-busy', 'true');
            submitButton.disabled = true;
        }, { once: true });
    }

    function setupAutoDismissAlerts() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            if (alert.dataset.manualDismiss === 'true') {
                return;
            }

            window.setTimeout(() => {
                alert.classList.remove('show');
                alert.classList.add('fade');
                const removeAlert = () => {
                    alert.removeEventListener('transitionend', removeAlert);
                    alert.remove();
                };
                alert.addEventListener('transitionend', removeAlert);
                window.setTimeout(removeAlert, 400); // Fallback caso não haja transição
            }, 6000);
        });
    }

    ready(() => {
        setupConfirmationDialogs();
        setupLoadingState();
        setupAutoDismissAlerts();
    });
})();
