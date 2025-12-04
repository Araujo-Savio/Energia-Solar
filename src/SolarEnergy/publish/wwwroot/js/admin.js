// Admin JavaScript Functions
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize admin features
    initializeAdminFeatures();
    
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize charts if available
    initializeCharts();
    
    // Auto-refresh data
    initializeAutoRefresh();
    
});

// Initialize admin-specific features
function initializeAdminFeatures() {
    // Bulk selection
    initializeBulkSelection();
    
    // Confirmation modals
    initializeConfirmationModals();
    
    // Data tables
    initializeDataTables();
    
    // Form validation
    initializeFormValidation();
    
    // Real-time notifications
    initializeRealTimeNotifications();
}

// Bulk selection for tables
function initializeBulkSelection() {
    const selectAllCheckbox = document.getElementById('select-all');
    const itemCheckboxes = document.querySelectorAll('.item-checkbox');
    const bulkActions = document.querySelector('.bulk-actions');
    
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            itemCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
            toggleBulkActions();
        });
    }
    
    itemCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            updateSelectAll();
            toggleBulkActions();
        });
    });
    
    function updateSelectAll() {
        if (selectAllCheckbox) {
            const checkedItems = document.querySelectorAll('.item-checkbox:checked').length;
            selectAllCheckbox.checked = checkedItems === itemCheckboxes.length;
            selectAllCheckbox.indeterminate = checkedItems > 0 && checkedItems < itemCheckboxes.length;
        }
    }
    
    function toggleBulkActions() {
        const checkedItems = document.querySelectorAll('.item-checkbox:checked').length;
        if (bulkActions) {
            bulkActions.style.display = checkedItems > 0 ? 'flex' : 'none';
        }
        
        // Update bulk action buttons with count
        const bulkButtons = document.querySelectorAll('.bulk-action-btn');
        bulkButtons.forEach(button => {
            const originalText = button.getAttribute('data-original-text') || button.textContent;
            if (!button.getAttribute('data-original-text')) {
                button.setAttribute('data-original-text', originalText);
            }
            button.textContent = checkedItems > 0 ? `${originalText} (${checkedItems})` : originalText;
        });
    }
}

// Confirmation modals for dangerous actions
function initializeConfirmationModals() {
    const confirmButtons = document.querySelectorAll('[data-confirm]');
    
    confirmButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const message = this.getAttribute('data-confirm');
            const action = this.getAttribute('href') || this.closest('form')?.getAttribute('action');
            
            showConfirmationModal(message, () => {
                if (this.tagName === 'A') {
                    window.location.href = action;
                } else if (this.closest('form')) {
                    this.closest('form').submit();
                }
            });
        });
    });
}

// Show confirmation modal
function showConfirmationModal(message, onConfirm) {
    const modalHtml = `
        <div class="modal fade admin-confirmation-modal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-admin-accent text-white">
                        <h5 class="modal-title">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            Confirmação
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="text-center py-3">
                            <i class="fas fa-exclamation-triangle text-warning mb-3" style="font-size: 3rem;"></i>
                            <p class="mb-0">${message}</p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times me-1"></i>Cancelar
                        </button>
                        <button type="button" class="btn btn-admin-accent confirm-action">
                            <i class="fas fa-check me-1"></i>Confirmar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove any existing modal
    const existingModal = document.querySelector('.admin-confirmation-modal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add new modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.querySelector('.admin-confirmation-modal'));
    
    // Handle confirmation
    document.querySelector('.confirm-action').addEventListener('click', function() {
        onConfirm();
        modal.hide();
    });
    
    modal.show();
}

// Initialize data tables with search and pagination
function initializeDataTables() {
    const tables = document.querySelectorAll('.admin-data-table');
    
    tables.forEach(table => {
        // Add search functionality
        const searchInput = table.parentElement.querySelector('.table-search');
        if (searchInput) {
            searchInput.addEventListener('input', function() {
                searchTable(table, this.value);
            });
        }
        
        // Add sorting functionality
        const headers = table.querySelectorAll('th[data-sortable]');
        headers.forEach(header => {
            header.style.cursor = 'pointer';
            header.addEventListener('click', function() {
                sortTable(table, this.cellIndex, this.getAttribute('data-sort-type') || 'string');
            });
        });
    });
}

// Search table functionality
function searchTable(table, searchTerm) {
    const rows = table.querySelectorAll('tbody tr');
    const term = searchTerm.toLowerCase();
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(term) ? '' : 'none';
    });
    
    // Update "no results" message
    const visibleRows = table.querySelectorAll('tbody tr[style*="display: none"]').length;
    updateNoResultsMessage(table, visibleRows === rows.length);
}

// Sort table functionality
function sortTable(table, columnIndex, sortType) {
    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr'));
    
    // Determine sort direction
    const header = table.querySelectorAll('th')[columnIndex];
    const currentDirection = header.getAttribute('data-sort-direction') || 'asc';
    const newDirection = currentDirection === 'asc' ? 'desc' : 'asc';
    
    // Update header indicators
    table.querySelectorAll('th').forEach(th => {
        th.removeAttribute('data-sort-direction');
        th.classList.remove('sorted-asc', 'sorted-desc');
    });
    
    header.setAttribute('data-sort-direction', newDirection);
    header.classList.add(`sorted-${newDirection}`);
    
    // Sort rows
    rows.sort((a, b) => {
        const aText = a.cells[columnIndex].textContent.trim();
        const bText = b.cells[columnIndex].textContent.trim();
        
        let aValue = aText;
        let bValue = bText;
        
        if (sortType === 'number') {
            aValue = parseFloat(aText.replace(/[^\d.-]/g, '')) || 0;
            bValue = parseFloat(bText.replace(/[^\d.-]/g, '')) || 0;
        } else if (sortType === 'date') {
            aValue = new Date(aText);
            bValue = new Date(bText);
        }
        
        if (newDirection === 'asc') {
            return aValue > bValue ? 1 : -1;
        } else {
            return aValue < bValue ? 1 : -1;
        }
    });
    
    // Append sorted rows
    rows.forEach(row => tbody.appendChild(row));
}

// Form validation
function initializeFormValidation() {
    const forms = document.querySelectorAll('.admin-form');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!validateForm(this)) {
                e.preventDefault();
                showFormErrors(this);
            }
        });
        
        // Real-time validation
        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateField(this);
            });
        });
    });
}

// Validate form
function validateForm(form) {
    let isValid = true;
    const inputs = form.querySelectorAll('[required]');
    
    inputs.forEach(input => {
        if (!validateField(input)) {
            isValid = false;
        }
    });
    
    return isValid;
}

// Validate individual field
function validateField(field) {
    const value = field.value.trim();
    let isValid = true;
    let errorMessage = '';
    
    // Required validation
    if (field.hasAttribute('required') && !value) {
        isValid = false;
        errorMessage = 'Este campo é obrigatório';
    }
    
    // Email validation
    if (field.type === 'email' && value && !isValidEmail(value)) {
        isValid = false;
        errorMessage = 'Email inválido';
    }
    
    // Custom validation
    const pattern = field.getAttribute('pattern');
    if (pattern && value && !new RegExp(pattern).test(value)) {
        isValid = false;
        errorMessage = field.getAttribute('data-error-message') || 'Formato inválido';
    }
    
    // Update field appearance
    field.classList.toggle('is-invalid', !isValid);
    field.classList.toggle('is-valid', isValid && value);
    
    // Show/hide error message
    const errorElement = field.nextElementSibling;
    if (errorElement && errorElement.classList.contains('invalid-feedback')) {
        errorElement.textContent = errorMessage;
    }
    
    return isValid;
}

// Email validation helper
function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

// Initialize tooltips
function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipElements.forEach(element => {
        new bootstrap.Tooltip(element);
    });
}

// Initialize charts
function initializeCharts() {
    // This will be expanded based on the specific chart library used
    if (typeof Chart !== 'undefined') {
        initializeChartJS();
    }
}

// Auto-refresh functionality
function initializeAutoRefresh() {
    const refreshElements = document.querySelectorAll('[data-auto-refresh]');
    
    refreshElements.forEach(element => {
        const interval = parseInt(element.getAttribute('data-auto-refresh')) * 1000;
        const url = element.getAttribute('data-refresh-url');
        
        if (interval && url) {
            setInterval(() => {
                refreshContent(element, url);
            }, interval);
        }
    });
}

// Refresh content
async function refreshContent(element, url) {
    try {
        const response = await fetch(url);
        const html = await response.text();
        
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const newContent = doc.querySelector(`#${element.id}`) || doc.body;
        
        element.innerHTML = newContent.innerHTML;
        
        // Reinitialize features for new content
        initializeAdminFeatures();
        
    } catch (error) {
        console.error('Error refreshing content:', error);
    }
}

// Real-time notifications
function initializeRealTimeNotifications() {
    // Sistema de notificações em tempo real habilitado
    console.log('Sistema de notificações em tempo real: Ativo');
    
    // Verificar notificações a cada 30 segundos
    setInterval(checkForNotifications, 30000);
    
    // Verificar imediatamente ao carregar
    checkForNotifications();
}

// Check for notifications
async function checkForNotifications() {
    try {
        const response = await fetch('/Admin/GetNotifications');
        
        if (!response.ok) {
            console.warn('Endpoint de notificações retornou:', response.status);
            return;
        }
        
        const notifications = await response.json();
        
        if (notifications && notifications.length > 0) {
            notifications.forEach(notification => {
                showNotification(notification.type, notification.message);
            });
        }
        
    } catch (error) {
        console.error('Error checking notifications:', error);
    }
}

// Show notification
function showNotification(type, message) {
    const alertClass = type === 'error' ? 'alert-danger' : 
                     type === 'warning' ? 'alert-warning' : 
                     type === 'success' ? 'alert-success' : 'alert-info';
    
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show admin-notification" role="alert">
            <i class="fas fa-${type === 'error' ? 'exclamation-circle' : 
                              type === 'warning' ? 'exclamation-triangle' : 
                              type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    const container = document.querySelector('.admin-notifications') || document.body;
    container.insertAdjacentHTML('afterbegin', alertHtml);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        const alert = container.querySelector('.admin-notification');
        if (alert) {
            alert.remove();
        }
    }, 5000);
}

// Export functions
window.AdminJS = {
    showConfirmationModal,
    showNotification,
    refreshContent,
    searchTable,
    sortTable,
    validateForm
};

// Utility functions
function updateNoResultsMessage(table, show) {
    let noResultsRow = table.querySelector('.no-results-row');
    
    if (show && !noResultsRow) {
        const colspan = table.querySelectorAll('thead th').length;
        const row = document.createElement('tr');
        row.className = 'no-results-row';
        row.innerHTML = `
            <td colspan="${colspan}" class="text-center py-4 text-muted">
                <i class="fas fa-search mb-2" style="font-size: 2rem;"></i>
                <br>Nenhum resultado encontrado
            </td>
        `;
        table.querySelector('tbody').appendChild(row);
    } else if (!show && noResultsRow) {
        noResultsRow.remove();
    }
}

// Format numbers for display
function formatNumber(num, decimals = 0) {
    return new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(num);
}

// Format currency for display
function formatCurrency(num) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(num);
}

// Format dates for display
function formatDate(date, format = 'short') {
    const options = format === 'short' ? 
        { day: '2-digit', month: '2-digit', year: 'numeric' } :
        { day: '2-digit', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' };
    
    return new Intl.DateTimeFormat('pt-BR', options).format(new Date(date));
}

// Export utility functions
window.AdminUtils = {
    formatNumber,
    formatCurrency,
    formatDate
};