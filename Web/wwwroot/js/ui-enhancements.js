// ========================================
// TOAST NOTIFICATION SYSTEM
// ========================================
window.showToast = function(message, type = 'info', duration = 5000) {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();
    const toast = createToast(message, type);
    toastContainer.appendChild(toast);
    
    const bsToast = new bootstrap.Toast(toast, { delay: duration });
    bsToast.show();
    
    toast.addEventListener('hidden.bs.toast', () => toast.remove());
};

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
    return container;
}

function createToast(message, type) {
    const icons = {
        success: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-check-circle-fill me-2" viewBox="0 0 16 16">
            <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
        </svg>`,
        error: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-x-circle-fill me-2" viewBox="0 0 16 16">
            <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0M5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293z"/>
        </svg>`,
        warning: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-exclamation-triangle-fill me-2" viewBox="0 0 16 16">
            <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.437-.99.98-1.767zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5m.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2"/>
        </svg>`,
        info: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-info-circle-fill me-2" viewBox="0 0 16 16">
            <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16m.93-9.412-1 4.705c-.07.34.029.533.304.533.194 0 .487-.07.686-.246l-.088.416c-.287.346-.92.598-1.465.598-.703 0-1.002-.422-.808-1.319l.738-3.468c.064-.293.006-.399-.287-.47l-.451-.081.082-.381 2.29-.287zM8 5.5a1 1 0 1 1 0-2 1 1 0 0 1 0 2"/>
        </svg>`
    };

    const bgColors = {
        success: 'bg-success',
        error: 'bg-danger',
        warning: 'bg-warning',
        info: 'bg-info'
    };

    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="toast-header ${bgColors[type]} text-white">
            ${icons[type]}
            <strong class="me-auto">${type.charAt(0).toUpperCase() + type.slice(1)}</strong>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">
            ${message}
        </div>
    `;
    return toast;
}

// ========================================
// CONFIRMATION MODAL
// ========================================
window.confirmAction = function(message, onConfirm, title = 'Confirm Action') {
    const modal = createConfirmModal(message, title, onConfirm);
    document.body.appendChild(modal);
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
    modal.addEventListener('hidden.bs.modal', () => modal.remove());
};

function createConfirmModal(message, title, onConfirm) {
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.innerHTML = `
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p>${message}</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-danger" id="confirmBtn">Confirm</button>
                </div>
            </div>
        </div>
    `;
    
    modal.querySelector('#confirmBtn').addEventListener('click', () => {
        onConfirm();
        bootstrap.Modal.getInstance(modal).hide();
    });
    
    return modal;
}

// ========================================
// LOADING STATE
// ========================================
window.showLoading = function(message = 'Loading...') {
    const overlay = document.createElement('div');
    overlay.id = 'loading-overlay';
    overlay.className = 'loading-overlay';
    overlay.innerHTML = `
        <div class="text-center">
            <div class="loading-spinner mb-3"></div>
            <p class="text-white">${message}</p>
        </div>
    `;
    document.body.appendChild(overlay);
};

window.hideLoading = function() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) overlay.remove();
};

// ========================================
// FORM LOADING BUTTON
// ========================================
window.setButtonLoading = function(button, loading = true) {
    if (loading) {
        button.classList.add('btn-loading');
        button.disabled = true;
    } else {
        button.classList.remove('btn-loading');
        button.disabled = false;
    }
};

// ========================================
// AUTO-SHOW TEMPDATA MESSAGES
// ========================================
document.addEventListener('DOMContentLoaded', () => {
    const successMessage = document.querySelector('[data-success-message]');
    const errorMessage = document.querySelector('[data-error-message]');
    const infoMessage = document.querySelector('[data-info-message]');
    const warningMessage = document.querySelector('[data-warning-message]');

    if (successMessage) {
        showToast(successMessage.getAttribute('data-success-message'), 'success');
    }
    if (errorMessage) {
        showToast(errorMessage.getAttribute('data-error-message'), 'error');
    }
    if (infoMessage) {
        showToast(infoMessage.getAttribute('data-info-message'), 'info');
    }
    if (warningMessage) {
        showToast(warningMessage.getAttribute('data-warning-message'), 'warning');
    }
});

// ========================================
// CONFIRM DELETE LINKS
// ========================================
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-confirm-delete]').forEach(element => {
        element.addEventListener('click', (e) => {
            e.preventDefault();
            const message = element.getAttribute('data-confirm-delete') || 'Are you sure you want to delete this item?';
            confirmAction(message, () => {
                if (element.tagName === 'FORM') {
                    element.submit();
                } else {
                    window.location.href = element.href;
                }
            }, 'Confirm Delete');
        });
    });
});
