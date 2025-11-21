(function () {
    const TOGGLE_SELECTOR = '[data-password-toggle="true"]';

    function getElementFromAttribute(source, attributeName) {
        const id = source.getAttribute(attributeName);
        return id ? document.getElementById(id) : null;
    }

    function resolveInput(toggleButton) {
        const explicitInput = getElementFromAttribute(toggleButton, 'data-password-input');
        if (explicitInput) {
            return explicitInput;
        }

        const field = toggleButton.closest('[data-password-field]');
        if (field) {
            const fallbackInput = field.querySelector('input[type="password"], input[type="text"]');
            if (fallbackInput) {
                return fallbackInput;
            }
        }

        return null;
    }

    function resolveIcon(toggleButton) {
        const explicitIcon = getElementFromAttribute(toggleButton, 'data-password-icon');
        if (explicitIcon) {
            return explicitIcon;
        }

        const iconInButton = toggleButton.querySelector('[data-password-icon]');
        return iconInButton || null;
    }

    function applyButtonState(toggleButton, input, icon) {
        if (!toggleButton || !input) {
            return;
        }

        const isHidden = input.getAttribute('type') === 'password';
        const label = isHidden ? 'Mostrar senha' : 'Ocultar senha';

        toggleButton.setAttribute('aria-label', label);
        toggleButton.setAttribute('title', label);
        toggleButton.setAttribute('aria-pressed', isHidden ? 'false' : 'true');

        if (input.id && !toggleButton.getAttribute('aria-controls')) {
            toggleButton.setAttribute('aria-controls', input.id);
        }

        if (icon) {
            icon.classList.toggle('fa-eye', isHidden);
            icon.classList.toggle('fa-eye-slash', !isHidden);
        }
    }

    function togglePasswordVisibility(input) {
        if (!input) {
            return;
        }

        const isHidden = input.getAttribute('type') === 'password';
        input.setAttribute('type', isHidden ? 'text' : 'password');
    }

    function initializeToggle(toggleButton) {
        const input = resolveInput(toggleButton);
        const icon = resolveIcon(toggleButton);

        if (!input) {
            toggleButton.disabled = true;
            toggleButton.setAttribute('aria-disabled', 'true');
            return;
        }

        applyButtonState(toggleButton, input, icon);

        toggleButton.addEventListener('click', function (event) {
            event.preventDefault();

            const targetInput = resolveInput(toggleButton);
            if (!targetInput) {
                toggleButton.disabled = true;
                toggleButton.setAttribute('aria-disabled', 'true');
                return;
            }

            togglePasswordVisibility(targetInput);
            applyButtonState(toggleButton, targetInput, resolveIcon(toggleButton));
        });
    }

    function wireUpPasswordToggles() {
        document.querySelectorAll(TOGGLE_SELECTOR).forEach(initializeToggle);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wireUpPasswordToggles);
    } else {
        wireUpPasswordToggles();
    }

    window.togglePasswordVisibility = function legacyToggle(inputId, iconSelector) {
        const input = document.getElementById(inputId);
        const icon = iconSelector ? document.querySelector(iconSelector) : null;
        if (!input) {
            return;
        }

        const isHidden = input.getAttribute('type') === 'password';
        input.setAttribute('type', isHidden ? 'text' : 'password');

        if (icon) {
            icon.classList.toggle('fa-eye', !isHidden);
            icon.classList.toggle('fa-eye-slash', isHidden);
        }
    };
})();
