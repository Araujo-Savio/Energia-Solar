(function () {
    function togglePasswordVisibility(inputId, iconId) {
        const input = document.getElementById(inputId);
        const icon = iconId ? document.getElementById(iconId) : null;

        if (!input) {
            return;
        }

        const isHidden = input.type === 'password';
        input.type = isHidden ? 'text' : 'password';

        if (icon) {
            icon.classList.toggle('fa-eye', !isHidden);
            icon.classList.toggle('fa-eye-slash', isHidden);
        }

        const toggleButton = document.querySelector(
            `[data-password-toggle][data-password-input="${inputId}"]`
        );

        if (toggleButton) {
            const label = isHidden ? 'Ocultar senha' : 'Mostrar senha';
            toggleButton.setAttribute('aria-label', label);
            toggleButton.setAttribute('title', label);
            toggleButton.setAttribute('aria-pressed', isHidden ? 'true' : 'false');
        }
    }

    function wireUpPasswordToggles() {
        document.querySelectorAll('[data-password-toggle]').forEach(button => {
            const inputId = button.getAttribute('data-password-input');
            const iconId = button.getAttribute('data-password-icon');

            if (!inputId) {
                return;
            }

            button.addEventListener('click', () => togglePasswordVisibility(inputId, iconId));
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wireUpPasswordToggles);
    } else {
        wireUpPasswordToggles();
    }

    window.togglePasswordVisibility = togglePasswordVisibility;
})();
