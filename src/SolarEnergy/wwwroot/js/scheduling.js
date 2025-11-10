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

    function setupServiceTypeOptions() {
        const companySelect = document.querySelector('[data-testid="select-company"]');
        const serviceTypeSelect = document.querySelector('[data-testid="input-servicetype"]');
        const serviceDataElement = document.querySelector('#companyServiceTypesData');

        if (!companySelect || !serviceTypeSelect || !serviceDataElement) {
            return;
        }

        let companyServiceTypes = {};
        try {
            const rawData = serviceDataElement.dataset.companyServiceTypes;
            if (rawData) {
                companyServiceTypes = JSON.parse(rawData);
            }
        } catch (error) {
            console.error('Não foi possível carregar os tipos de serviço das empresas.', error);
        }

        const serviceOptionSets = {
            PanelSales: [
                { value: 'Venda de Painéis Solares', text: 'Venda de Painéis Solares' }
            ],
            EnergyRental: [
                { value: 'Aluguel de Energia Solar', text: 'Aluguel de Energia Solar' }
            ],
            Both: [
                { value: 'Venda de Painéis Solares', text: 'Venda de Painéis Solares' },
                { value: 'Aluguel de Energia Solar', text: 'Aluguel de Energia Solar' },
                { value: 'Consultoria', text: 'Consultoria (Venda ou Aluguel)' }
            ],
            Default: [
                { value: 'Venda de Painéis Solares', text: 'Venda de Painéis Solares' },
                { value: 'Aluguel de Energia Solar', text: 'Aluguel de Energia Solar' }
            ]
        };

        const placeholderText = serviceTypeSelect.dataset.placeholder || 'Selecione o tipo de serviço';

        function getOptions(serviceTypeKey) {
            if (!serviceTypeKey) {
                return serviceOptionSets.Default;
            }

            return serviceOptionSets[serviceTypeKey] ?? serviceOptionSets.Default;
        }

        function renderOptions(companyId, preserveSelection) {
            serviceTypeSelect.innerHTML = '';
            const placeholderOption = document.createElement('option');
            placeholderOption.value = '';
            placeholderOption.textContent = placeholderText;
            serviceTypeSelect.appendChild(placeholderOption);

            if (!companyId) {
                serviceTypeSelect.value = '';
                serviceTypeSelect.dataset.selectedValue = '';
                serviceTypeSelect.disabled = true;
                return;
            }

            const serviceTypeKey = companyServiceTypes[companyId] || 'Default';
            const options = getOptions(serviceTypeKey);

            options.forEach(option => {
                const optionElement = document.createElement('option');
                optionElement.value = option.value;
                optionElement.textContent = option.text;
                serviceTypeSelect.appendChild(optionElement);
            });

            const selectedValue = preserveSelection ? (serviceTypeSelect.dataset.selectedValue || '') : '';

            if (selectedValue && options.some(option => option.value === selectedValue)) {
                serviceTypeSelect.value = selectedValue;
            } else {
                serviceTypeSelect.value = '';
                serviceTypeSelect.dataset.selectedValue = '';
            }

            serviceTypeSelect.disabled = false;
            serviceTypeSelect.dataset.selectedValue = serviceTypeSelect.value;
        }

        if (!serviceTypeSelect.dataset.selectedValue) {
            serviceTypeSelect.dataset.selectedValue = serviceTypeSelect.value || '';
        }

        renderOptions(companySelect.value, true);

        companySelect.addEventListener('change', () => {
            serviceTypeSelect.dataset.selectedValue = '';
            renderOptions(companySelect.value, false);
        });

        serviceTypeSelect.addEventListener('change', () => {
            serviceTypeSelect.dataset.selectedValue = serviceTypeSelect.value;
        });
    }

    ready(() => {
        setupConfirmationDialogs();
        setupLoadingState();
        setupAutoDismissAlerts();
        setupServiceTypeOptions();
    });
})();
