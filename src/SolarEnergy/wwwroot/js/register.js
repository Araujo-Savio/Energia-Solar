// Funcionalidades para a página de cadastro
document.addEventListener('DOMContentLoaded', function() {
    const userTypeButtons = document.querySelectorAll('input[name="userTypeOptions"]');
    const userTypeHidden = document.getElementById('userTypeHidden');
    const clientSection = document.getElementById('clientFields');
    const companySection = document.getElementById('companyFields');
    const cpfField = document.getElementById('cpfField');
    const cnpjField = document.getElementById('cnpjField');
    const cpfInput = document.getElementById('cpfInput');
    const cnpjInput = document.getElementById('cnpjInput');
    const phoneInput = document.getElementById('Phone');
    const companyPhoneInput = document.getElementById('CompanyPhone');
    const responsibleCpfInput = document.getElementById('responsibleCpfInput');
    const responsibleNameInput = document.getElementById('ResponsibleName');
    const fullNameLabel = document.getElementById('fullNameLabel');
    const emailLabel = document.getElementById('emailLabel');
    const fullNameInput = document.getElementById('FullName');
    const emailInput = document.getElementById('Email');
    const passwordToggles = document.querySelectorAll('.password-toggle');
    const registerForm = document.getElementById('registerForm');

    const clientRequiredFields = clientSection ? Array.from(clientSection.querySelectorAll('[data-required-client]')) : [];
    const companyRequiredFields = companySection ? Array.from(companySection.querySelectorAll('[data-required-company]')) : [];

    const companyValue = '2';

    userTypeButtons.forEach(button => {
        button.addEventListener('change', function() {
            if (this.checked) {
                handleUserTypeChange(this.value);
            }
        });
    });

    function toggleSection(section, show) {
        if (!section) return;
        section.classList.toggle('d-none', !show);
        const inputs = section.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.disabled = !show;
            if (!show) {
                input.classList.remove('is-invalid', 'is-valid');
                input.setCustomValidity('');
            }
        });
    }

    function toggleRequired(fields, required) {
        fields.forEach(field => {
            field.required = required;
            if (!required) {
                field.setCustomValidity('');
            }
        });
    }

    function updateLabels(isCompany) {
        if (fullNameLabel) {
            const labelText = isCompany ? fullNameLabel.dataset.companyLabel : fullNameLabel.dataset.clientLabel;
            fullNameLabel.textContent = labelText;
        }
        if (emailLabel) {
            const labelText = isCompany ? emailLabel.dataset.companyLabel : emailLabel.dataset.clientLabel;
            emailLabel.textContent = labelText;
        }
        if (fullNameInput) {
            const placeholder = isCompany ? fullNameInput.dataset.companyPlaceholder : fullNameInput.dataset.clientPlaceholder;
            if (placeholder) {
                fullNameInput.placeholder = placeholder;
            }
        }
        if (emailInput) {
            const placeholder = isCompany ? emailInput.dataset.companyPlaceholder : emailInput.dataset.clientPlaceholder;
            if (placeholder) {
                emailInput.placeholder = placeholder;
            }
        }
    }

    function handleUserTypeChange(value) {
        const isCompany = value === companyValue;

        if (userTypeHidden) {
            userTypeHidden.value = value;
        }

        toggleSection(clientSection, !isCompany);
        toggleSection(companySection, isCompany);

        toggleRequired(clientRequiredFields, !isCompany);
        toggleRequired(companyRequiredFields, isCompany);

        if (cpfField) {
            cpfField.style.display = isCompany ? 'none' : 'block';
        }
        if (cnpjField) {
            cnpjField.style.display = isCompany ? 'block' : 'none';
        }

        if (cpfInput) {
            cpfInput.required = !isCompany;
            if (isCompany) {
                cpfInput.value = '';
            }
        }
        if (cnpjInput) {
            cnpjInput.required = isCompany;
            if (!isCompany) {
                cnpjInput.value = '';
            }
        }

        updateLabels(isCompany);

        if (isCompany && cnpjField) {
            showTooltip(cnpjField, 'Para empresas, utilize o CNPJ da sua empresa.', 'info');
        } else if (!isCompany && cpfField) {
            showTooltip(cpfField, 'Para consumidores, utilize seu CPF pessoal.', 'info');
        }
    }

    function showTooltip(element, message, type = 'info') {
        const existingTooltip = element.querySelector('.field-tooltip');
        if (existingTooltip) {
            existingTooltip.remove();
        }

        const tooltip = document.createElement('div');
        tooltip.className = `field-tooltip alert alert-${type} alert-dismissible fade show mt-1`;
        tooltip.style.padding = '0.25rem 0.5rem';
        tooltip.style.fontSize = '0.75rem';
        tooltip.innerHTML = `
            <i class="fas fa-info-circle me-1"></i>${message}
            <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="alert"></button>
        `;

        element.appendChild(tooltip);

        setTimeout(() => {
            if (tooltip && tooltip.parentNode) {
                tooltip.remove();
            }
        }, 3000);
    }

    function formatCpf(value) {
        const digits = value.replace(/\D/g, '').slice(0, 11);
        let formatted = '';

        for (let i = 0; i < digits.length; i++) {
            if (i === 3 || i === 6) {
                formatted += '.';
            }
            if (i === 9) {
                formatted += '-';
            }
            formatted += digits[i];
        }

        return formatted;
    }

    function formatCnpj(value) {
        const digits = value.replace(/\D/g, '').slice(0, 14);
        let formatted = '';

        for (let i = 0; i < digits.length; i++) {
            if (i === 2 || i === 5) {
                formatted += '.';
            }
            if (i === 8) {
                formatted += '/';
            }
            if (i === 12) {
                formatted += '-';
            }
            formatted += digits[i];
        }

        return formatted;
    }

    function applyCpfMask(input) {
        input.addEventListener('input', function(e) {
            if (input.disabled) return;
            e.target.value = formatCpf(e.target.value);
        });
    }

    function applyCnpjMask(input) {
        input.addEventListener('input', function(e) {
            if (input.disabled) return;
            e.target.value = formatCnpj(e.target.value);
        });
    }

    function applyPhoneMask(input) {
        input.addEventListener('input', function(e) {
            if (input.disabled) return;
            let value = e.target.value.replace(/\D/g, '');
            if (value.length <= 11) {
                if (value.length <= 10) {
                    value = value.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
                } else {
                    value = value.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                }
                e.target.value = value;
            }
        });
    }

    if (cpfInput) {
        applyCpfMask(cpfInput);
    }
    if (responsibleCpfInput) {
        applyCpfMask(responsibleCpfInput);
    }
    if (cnpjInput) {
        applyCnpjMask(cnpjInput);
    }
    if (phoneInput) {
        applyPhoneMask(phoneInput);
    }
    if (companyPhoneInput) {
        applyPhoneMask(companyPhoneInput);
    }

    const passwordInput = document.getElementById('Password');
    if (passwordInput) {
        passwordInput.addEventListener('input', function(e) {
            const password = e.target.value;
            let strength = 0;
            let suggestions = [];

            if (password.length >= 8) {
                strength++;
            } else {
                suggestions.push('pelo menos 8 caracteres');
            }

            if (/[a-z]/.test(password)) {
                strength++;
            } else {
                suggestions.push('uma letra minúscula');
            }

            if (/[A-Z]/.test(password)) {
                strength++;
            } else {
                suggestions.push('uma letra maiúscula');
            }

            if (/[0-9]/.test(password)) {
                strength++;
            } else {
                suggestions.push('um número');
            }

            if (/[^A-Za-z0-9]/.test(password)) {
                strength++;
            } else {
                suggestions.push('um caractere especial (!@#$%&*)');
            }

            const existingIndicator = document.querySelector('.password-strength');
            if (existingIndicator) {
                existingIndicator.remove();
            }

            if (password.length > 0) {
                const indicator = document.createElement('div');
                indicator.className = 'password-strength mt-1';

                let strengthText = '';
                let strengthClass = '';
                let strengthIcon = '';

                switch (strength) {
                    case 0:
                    case 1:
                        strengthText = 'Muito fraca';
                        strengthClass = 'text-danger';
                        strengthIcon = 'fas fa-times-circle';
                        break;
                    case 2:
                        strengthText = 'Fraca';
                        strengthClass = 'text-warning';
                        strengthIcon = 'fas fa-exclamation-circle';
                        break;
                    case 3:
                        strengthText = 'Média';
                        strengthClass = 'text-info';
                        strengthIcon = 'fas fa-info-circle';
                        break;
                    case 4:
                        strengthText = 'Forte';
                        strengthClass = 'text-success';
                        strengthIcon = 'fas fa-check-circle';
                        break;
                    case 5:
                        strengthText = 'Muito forte';
                        strengthClass = 'text-success fw-bold';
                        strengthIcon = 'fas fa-shield-alt';
                        break;
                }

                let suggestionText = '';
                if (suggestions.length > 0) {
                    suggestionText = `<br><small class="text-muted">Adicione: ${suggestions.join(', ')}</small>`;
                }

                indicator.innerHTML = `<small class="${strengthClass}"><i class="${strengthIcon} me-1"></i>Força da senha: ${strengthText}${suggestionText}</small>`;
                passwordInput.parentNode.appendChild(indicator);
            }
        });
    }

    const confirmPasswordInput = document.getElementById('ConfirmPassword');
    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('input', function(e) {
            const password = passwordInput.value;
            const confirmPassword = e.target.value;

            const existingIndicator = document.querySelector('.password-match');
            if (existingIndicator) {
                existingIndicator.remove();
            }

            if (confirmPassword.length > 0) {
                const indicator = document.createElement('div');
                indicator.className = 'password-match mt-1';

                if (password === confirmPassword) {
                    indicator.innerHTML = '<small class="text-success"><i class="fas fa-check me-1"></i>Senhas conferem perfeitamente!</small>';
                } else {
                    indicator.innerHTML = '<small class="text-danger"><i class="fas fa-times me-1"></i>As senhas não coincidem</small>';
                }

                confirmPasswordInput.parentNode.appendChild(indicator);
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener('submit', function() {
            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Criando sua conta...';
                submitButton.disabled = true;
                submitButton.classList.add('btn-loading');
            }
        });
    }

    const trackableFields = document.querySelectorAll('[data-required-client], [data-required-company], input[required]');
    trackableFields.forEach(field => {
        field.addEventListener('blur', function() {
            if (field.required && !field.disabled) {
                if (!field.value.trim()) {
                    field.classList.add('is-invalid');
                    showFieldError(field, 'Este campo é obrigatório');
                } else {
                    field.classList.remove('is-invalid');
                    field.classList.add('is-valid');
                    hideFieldError(field);
                }
            }
        });

        field.addEventListener('input', function() {
            if (!field.disabled && field.value.trim()) {
                field.classList.remove('is-invalid');
                field.classList.add('is-valid');
                hideFieldError(field);
            }
        });
    });

    function showFieldError(field, message) {
        hideFieldError(field);
        const errorDiv = document.createElement('div');
        errorDiv.className = 'field-error invalid-feedback d-block';
        errorDiv.textContent = message;
        field.parentNode.appendChild(errorDiv);
    }

    function hideFieldError(field) {
        const existingError = field.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
    }

    function validateCPF(cpf) {
        cpf = cpf.replace(/\D/g, '');
        if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;

        let sum = 0;
        for (let i = 0; i < 9; i++) {
            sum += parseInt(cpf.charAt(i)) * (10 - i);
        }
        let remainder = sum % 11;
        let digit1 = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (let i = 0; i < 10; i++) {
            sum += parseInt(cpf.charAt(i)) * (11 - i);
        }
        remainder = sum % 11;
        let digit2 = remainder < 2 ? 0 : 11 - remainder;

        return digit1 === parseInt(cpf.charAt(9)) && digit2 === parseInt(cpf.charAt(10));
    }

    function validateCNPJ(cnpj) {
        cnpj = cnpj.replace(/\D/g, '');
        if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) return false;

        let weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        let weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        let sum = 0;
        for (let i = 0; i < 12; i++) {
            sum += parseInt(cnpj.charAt(i)) * weights1[i];
        }
        let digit1 = sum % 11 < 2 ? 0 : 11 - (sum % 11);

        sum = 0;
        for (let i = 0; i < 13; i++) {
            sum += parseInt(cnpj.charAt(i)) * weights2[i];
        }
        let digit2 = sum % 11 < 2 ? 0 : 11 - (sum % 11);

        return digit1 === parseInt(cnpj.charAt(12)) && digit2 === parseInt(cnpj.charAt(13));
    }

    if (cpfInput) {
        cpfInput.addEventListener('blur', function() {
            if (clientSection && !clientSection.classList.contains('d-none') && cpfInput.value) {
                if (!validateCPF(cpfInput.value)) {
                    cpfInput.setCustomValidity('CPF inválido. Verifique os números digitados.');
                    cpfInput.classList.add('is-invalid');
                    showFieldError(cpfInput, 'CPF inválido. Verifique os números digitados.');
                } else {
                    cpfInput.setCustomValidity('');
                    cpfInput.classList.remove('is-invalid');
                    cpfInput.classList.add('is-valid');
                    hideFieldError(cpfInput);
                }
            }
        });
    }

    if (cnpjInput) {
        cnpjInput.addEventListener('blur', function() {
            if (companySection && !companySection.classList.contains('d-none') && cnpjInput.value) {
                if (!validateCNPJ(cnpjInput.value)) {
                    cnpjInput.setCustomValidity('CNPJ inválido. Verifique os números digitados.');
                    cnpjInput.classList.add('is-invalid');
                    showFieldError(cnpjInput, 'CNPJ inválido. Verifique os números digitados.');
                } else {
                    cnpjInput.setCustomValidity('');
                    cnpjInput.classList.remove('is-invalid');
                    cnpjInput.classList.add('is-valid');
                    hideFieldError(cnpjInput);
                }
            }
        });
    }

    if (emailInput) {
        emailInput.addEventListener('blur', function() {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (emailInput.value && !emailRegex.test(emailInput.value)) {
                emailInput.classList.add('is-invalid');
                showFieldError(emailInput, 'Por favor, insira um e-mail válido (exemplo: seu@email.com)');
            } else if (emailInput.value) {
                emailInput.classList.remove('is-invalid');
                emailInput.classList.add('is-valid');
                hideFieldError(emailInput);
            }
        });
    }

    passwordToggles.forEach(button => {
        button.addEventListener('click', function() {
            const target = document.getElementById(button.dataset.target);
            if (!target) return;
            const icon = button.querySelector('i');
            if (target.type === 'password') {
                target.type = 'text';
                if (icon) {
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                }
            } else {
                target.type = 'password';
                if (icon) {
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            }
        });
    });

    if (responsibleNameInput && fullNameInput) {
        responsibleNameInput.addEventListener('blur', function() {
            if (userTypeHidden && userTypeHidden.value === companyValue && !fullNameInput.value.trim()) {
                fullNameInput.value = responsibleNameInput.value;
            }
        });
    }

    if (userTypeHidden) {
        handleUserTypeChange(userTypeHidden.value);
    }
});
