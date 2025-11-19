(function () {
    /**
     * Resolve an element either via explicit selector stored in the
     * data attribute or by looking inside the password field wrapper.
     */
    function resolveTarget(source, attributeName, fallbackSelector) {
        const selector = source.getAttribute(attributeName);
        if (selector) {
            if (selector.startsWith('#') || selector.startsWith('.') || selector.startsWith('[')) {
                return document.querySelector(selector);
            }
            return document.getElementById(selector);
        }

        const field = source.closest('[data-password-field]');
        if (field && fallbackSelector) {
            return field.querySelector(fallbackSelector);
        }

        return null;
    }

    function applyLabel(toggleButton, isHidden) {
        const label = isHidden ? 'Mostrar senha' : 'Ocultar senha';
        toggleButton.setAttribute('aria-label', label);
        toggleButton.setAttribute('title', label);
        toggleButton.setAttribute('aria-pressed', isHidden ? 'false' : 'true');
    }

    function togglePasswordVisibility(input, icon, toggleButton) {
        if (!input) {
            return;
        }

        const isHidden = input.getAttribute('type') === 'password';
        input.setAttribute('type', isHidden ? 'text' : 'password');

        if (icon) {
            icon.classList.toggle('fa-eye', !isHidden);
            icon.classList.toggle('fa-eye-slash', isHidden);
        }

        if (toggleButton) {
            applyLabel(toggleButton, !isHidden);
        }
    }

    function initializeToggle(toggleButton) {
        const input = resolveTarget(toggleButton, 'data-password-input', '[data-password-input]');
        const icon = resolveTarget(toggleButton, 'data-password-icon', '[data-password-icon]')
            || toggleButton.querySelector('[data-password-icon]');

        if (!input) {
            toggleButton.disabled = true;
            return;
        }

        applyLabel(toggleButton, input.getAttribute('type') === 'password');

        toggleButton.addEventListener('click', function (event) {
            event.preventDefault();
            togglePasswordVisibility(input, icon, toggleButton);
        });
    }

    function wireUpPasswordToggles() {
        document.querySelectorAll('[data-password-toggle]').forEach(initializeToggle);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wireUpPasswordToggles);
    } else {
        wireUpPasswordToggles();
    }

    // Preserve backwards compatibility in case other scripts call it.
    window.togglePasswordVisibility = function legacyToggle(inputId, iconSelector) {
        const input = document.getElementById(inputId);
        const icon = iconSelector ? document.querySelector(iconSelector) : null;
        togglePasswordVisibility(input, icon, null);
    };
})();
