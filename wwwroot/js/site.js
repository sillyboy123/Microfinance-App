// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Generic utility functions
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize elements with animation classes
    const animatedElements = document.querySelectorAll('.fade-in, .slide-in-left, .slide-in-right, .slide-in-up');
    animatedElements.forEach(el => {
        el.classList.add('show');
    });
});

// Transaction page functionality
document.addEventListener('DOMContentLoaded', function() {
    const transactionTable = document.getElementById('transactionsTable');
    if (!transactionTable) return;

    const searchInput = document.getElementById('transactionSearch');
    const sortableHeaders = document.querySelectorAll('th.sortable');
    const filterDropdownItems = document.querySelectorAll('[data-filter]');
    const paginationLinks = document.querySelectorAll('.pagination .page-link');
    const prevPageBtn = document.getElementById('prev-page');
    const nextPageBtn = document.getElementById('next-page');

    let currentPage = 1;
    const itemsPerPage = 10;
    let currentSort = { column: '', direction: 'asc' };
    let currentFilter = 'all';
    
    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('keyup', function() {
            const searchTerm = this.value.toLowerCase();
            filterTable(searchTerm, currentFilter);
        });
    }
    
    // Sorting functionality
    sortableHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const column = this.dataset.sort;
            
            // Toggle sort direction
            if (currentSort.column === column) {
                currentSort.direction = currentSort.direction === 'asc' ? 'desc' : 'asc';
            } else {
                currentSort.column = column;
                currentSort.direction = 'asc';
            }
            
            // Update sort indicators
            sortableHeaders.forEach(h => {
                const icon = h.querySelector('i');
                icon.className = 'fas fa-sort ms-1';
            });
            
            const icon = this.querySelector('i');
            icon.className = `fas fa-sort-${currentSort.direction === 'asc' ? 'up' : 'down'} ms-1`;
            
            sortTable(column, currentSort.direction);
        });
    });
    
    // Filter functionality
    filterDropdownItems.forEach(item => {
        item.addEventListener('click', function(e) {
            e.preventDefault();
            currentFilter = this.dataset.filter;
            
            // Update dropdown button text
            const filterDropdown = document.getElementById('filterDropdown');
            filterDropdown.innerHTML = `<i class="fas fa-filter me-2"></i>${this.textContent}`;
            
            filterTable(searchInput?.value.toLowerCase() || '', currentFilter);
        });
    });
    
    // Pagination functionality
    paginationLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const page = this.textContent;
            
            if (page === '1' || page === '2' || page === '3' || page === '4') {
                currentPage = parseInt(page);
                updatePagination();
            }
        });
    });
    
    if (prevPageBtn) {
        prevPageBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (!this.classList.contains('disabled') && currentPage > 1) {
                currentPage--;
                updatePagination();
            }
        });
    }
    
    if (nextPageBtn) {
        nextPageBtn.addEventListener('click', function(e) {
            e.preventDefault();
            const maxPage = Math.ceil(getVisibleRows().length / itemsPerPage);
            if (!this.classList.contains('disabled') && currentPage < maxPage) {
                currentPage++;
                updatePagination();
            }
        });
    }
    
    // Table filter function
    function filterTable(searchTerm, filter) {
        const rows = transactionTable.querySelectorAll('tbody tr');
        let visibleCount = 0;
        
        rows.forEach(row => {
            const reference = row.querySelector('td:nth-child(1)').textContent.toLowerCase();
            const client = row.querySelector('td:nth-child(2)').textContent.toLowerCase();
            const type = row.querySelector('td:nth-child(3)').textContent.toLowerCase();
            const amount = row.querySelector('td:nth-child(4)').textContent.toLowerCase();
            const date = row.querySelector('td:nth-child(5)').textContent.toLowerCase();
            const status = row.querySelector('td:nth-child(6)').textContent.toLowerCase();
            
            const matchesSearch = reference.includes(searchTerm) || 
                               client.includes(searchTerm) ||
                               type.includes(searchTerm) ||
                               amount.includes(searchTerm) ||
                               date.includes(searchTerm) ||
                               status.includes(searchTerm);
            
            // Apply filters
            let matchesFilter = true;
            const today = new Date();
            const txDate = new Date(date);
            
            switch (filter) {
                case 'completed':
                    matchesFilter = status.includes('success');
                    break;
                case 'pending':
                    matchesFilter = status.includes('pending');
                    break;
                case 'today':
                    matchesFilter = txDate.setHours(0,0,0,0) === today.setHours(0,0,0,0);
                    break;
                case 'thisWeek':
                    const weekStart = new Date(today);
                    weekStart.setDate(today.getDate() - today.getDay());
                    matchesFilter = txDate >= weekStart;
                    break;
                case 'thisMonth':
                    matchesFilter = txDate.getMonth() === today.getMonth() && 
                                 txDate.getFullYear() === today.getFullYear();
                    break;
                default:
                    matchesFilter = true;
            }
            
            const visible = matchesSearch && matchesFilter;
            row.style.display = visible ? '' : 'none';
            if (visible) visibleCount++;
        });
        
        // Reset pagination
        currentPage = 1;
        updatePagination();
        
        // Update showing text
        document.getElementById('total-items').textContent = visibleCount;
    }
    
    // Table sorting function
    function sortTable(column, direction) {
        const rows = Array.from(transactionTable.querySelectorAll('tbody tr'));
        
        rows.sort((rowA, rowB) => {
            let valueA, valueB;
            
            switch (column) {
                case 'reference':
                    valueA = rowA.querySelector('td:nth-child(1)').textContent.trim();
                    valueB = rowB.querySelector('td:nth-child(1)').textContent.trim();
                    break;
                case 'type':
                    valueA = rowA.querySelector('td:nth-child(3)').textContent.trim();
                    valueB = rowB.querySelector('td:nth-child(3)').textContent.trim();
                    break;
                case 'amount':
                    valueA = parseFloat(rowA.querySelector('td:nth-child(4)').textContent.replace(/[^0-9.-]+/g, ''));
                    valueB = parseFloat(rowB.querySelector('td:nth-child(4)').textContent.replace(/[^0-9.-]+/g, ''));
                    break;
                case 'date':
                    valueA = new Date(rowA.querySelector('td:nth-child(5)').textContent.trim());
                    valueB = new Date(rowB.querySelector('td:nth-child(5)').textContent.trim());
                    break;
                default:
                    return 0;
            }
            
            if (valueA < valueB) {
                return direction === 'asc' ? -1 : 1;
            }
            if (valueA > valueB) {
                return direction === 'asc' ? 1 : -1;
            }
            return 0;
        });
        
        // Reattach rows in new order
        const tbody = transactionTable.querySelector('tbody');
        rows.forEach(row => tbody.appendChild(row));
        
        // Reset pagination
        currentPage = 1;
        updatePagination();
    }
    
    // Get visible rows
    function getVisibleRows() {
        return Array.from(transactionTable.querySelectorAll('tbody tr')).filter(row => 
            row.style.display !== 'none'
        );
    }
    
    // Update pagination display
    function updatePagination() {
        const visibleRows = getVisibleRows();
        const totalItems = visibleRows.length;
        const maxPage = Math.ceil(totalItems / itemsPerPage);
        
        // Update page links
        const pageLinks = document.querySelectorAll('.pagination .page-item:not(.disabled):not(#prev-page):not(#next-page)');
        pageLinks.forEach((link, index) => {
            if (index + 1 <= maxPage) {
                link.style.display = '';
                if (index + 1 === currentPage) {
                    link.classList.add('active');
                } else {
                    link.classList.remove('active');
                }
            } else {
                link.style.display = 'none';
            }
        });
        
        // Update prev/next buttons
        prevPageBtn.classList.toggle('disabled', currentPage === 1);
        nextPageBtn.classList.toggle('disabled', currentPage === maxPage || maxPage === 0);
        
        // Update showing text
        const start = Math.min((currentPage - 1) * itemsPerPage + 1, totalItems);
        const end = Math.min(currentPage * itemsPerPage, totalItems);
        document.getElementById('showing-start').textContent = totalItems > 0 ? start : 0;
        document.getElementById('showing-end').textContent = end;
        
        // Show/hide rows based on pagination
        visibleRows.forEach((row, index) => {
            const rowNum = index + 1;
            const showOnThisPage = rowNum > (currentPage - 1) * itemsPerPage && rowNum <= currentPage * itemsPerPage;
            row.style.display = showOnThisPage ? '' : 'none';
        });
    }
    
    // Export functionality
    const exportBtn = document.getElementById('exportTransactionBtn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function() {
            exportTableToCSV('transactions.csv');
        });
    }
    
    // Function to export table data to CSV
    function exportTableToCSV(filename) {
        const rows = getVisibleRows();
        let csv = [];
        
        // Add header row
        const headers = [];
        transactionTable.querySelectorAll('thead th').forEach(th => {
            headers.push(th.textContent.trim().replace(/\s*\<i.*\<\/i\>/g, ''));
        });
        csv.push(headers.join(','));
        
        // Add data rows
        rows.forEach(row => {
            const rowData = [];
            row.querySelectorAll('td').forEach((td, index) => {
                // Skip the actions column
                if (index < 7) { // Assuming actions is the last column
                    let text = td.textContent.trim().replace(/,/g, ';');
                    rowData.push(`"${text}"`);
                }
            });
            csv.push(rowData.join(','));
        });
        
        // Create download link
        const csvContent = csv.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
    
    // Print receipt functionality
    const printButtons = document.querySelectorAll('.js-print-receipt');
    printButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            const transactionId = this.dataset.id;
            printReceipt(transactionId);
        });
    });
    
    function printReceipt(transactionId) {
        // In a real implementation, you would fetch the transaction details from the server
        // For now, we'll create a simple receipt window
        const receiptWindow = window.open('', '_blank', 'width=400,height=600');
        receiptWindow.document.write(`
            <html>
                <head>
                    <title>Transaction Receipt</title>
                    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css" rel="stylesheet">
                    <style>
                        body { font-family: Arial, sans-serif; padding: 20px; }
                        .receipt { max-width: 400px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; }
                        .receipt-header { text-align: center; margin-bottom: 20px; }
                        .receipt-logo { max-width: 150px; margin-bottom: 10px; }
                        .receipt-details { margin-bottom: 20px; }
                        .receipt-table { width: 100%; margin-bottom: 20px; }
                        .receipt-footer { text-align: center; font-size: 12px; color: #777; }
                    </style>
                </head>
                <body>
                    <div class="receipt">
                        <div class="receipt-header">
                            <h4>Microfinance App</h4>
                            <p>Transaction Receipt</p>
                        </div>
                        <div class="receipt-details">
                            <p><strong>Transaction ID:</strong> ${transactionId}</p>
                            <p><strong>Date:</strong> ${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}</p>
                        </div>
                        <hr>
                        <p class="text-center">Thank you for your business!</p>
                        <div class="receipt-footer mt-4">
                            <p>For questions regarding this receipt, please contact support.</p>
                            <button class="btn btn-primary" onclick="window.print()">Print</button>
                            <button class="btn btn-secondary ms-2" onclick="window.close()">Close</button>
                        </div>
                    </div>
                </body>
            </html>
        `);
    }
    
    // Initialize the table with default settings
    updatePagination();
});

// Add animation to transaction table rows
document.addEventListener('DOMContentLoaded', function() {
    // Add entrance animations to table rows
    const tableRows = document.querySelectorAll('.table tbody tr');
    
    tableRows.forEach((row, index) => {
        // Add animation delay based on index
        row.style.animationDelay = `${index * 0.05}s`;
        row.classList.add('fade-in');
        
        // After a delay, add the show class to trigger the animation
        setTimeout(() => {
            row.classList.add('show');
        }, 100);
        
        // Add hover class for enhanced interactions
        row.addEventListener('mouseenter', function() {
            this.classList.add('row-hover');
        });
        
        row.addEventListener('mouseleave', function() {
            this.classList.remove('row-hover');
        });
    });
    
    // Initialize tooltips in the application
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0) {
        tooltipTriggerList.forEach(function(tooltipTriggerEl) {
            new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Add directional awareness to buttons and cards
    const interactiveElements = document.querySelectorAll('.btn, .card.hover-shadow-lg');
    
    interactiveElements.forEach(element => {
        element.addEventListener('mousemove', function(e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            
            const angleX = (y - centerY) / 10;
            const angleY = (centerX - x) / 10;
            
            this.style.transform = `perspective(1000px) rotateX(${angleX}deg) rotateY(${angleY}deg) scale3d(1.02, 1.02, 1.02)`;
        });
        
        element.addEventListener('mouseleave', function() {
            this.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) scale3d(1, 1, 1)';
        });
    });
});

// Client page functionality
document.addEventListener('DOMContentLoaded', function() {
    const clientsTable = document.getElementById('clientsTable');
    const clientSearch = document.getElementById('clientSearch');
    if (!clientsTable) return;
    
    // Variables for client pagination
    let currentClientPage = 1;
    const clientsPerPage = 10;
    let currentClientSort = { column: '', direction: 'asc' };
    let currentClientFilter = 'all';
    
    // Client search functionality
    if (clientSearch) {
        clientSearch.addEventListener('keyup', function() {
            const searchTerm = this.value.toLowerCase();
            filterClients(searchTerm, currentClientFilter);
        });
    }
    
    // Client sorting functionality
    const sortableClientHeaders = document.querySelectorAll('#clientsTable th.sortable');
    sortableClientHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const column = this.dataset.sort;
            
            // Toggle sort direction
            if (currentClientSort.column === column) {
                currentClientSort.direction = currentClientSort.direction === 'asc' ? 'desc' : 'asc';
            } else {
                currentClientSort.column = column;
                currentClientSort.direction = 'asc';
            }
            
            // Update sort indicators
            sortableClientHeaders.forEach(h => {
                const icon = h.querySelector('i');
                icon.className = 'fas fa-sort ms-1';
            });
            
            const icon = this.querySelector('i');
            icon.className = `fas fa-sort-${currentClientSort.direction === 'asc' ? 'up' : 'down'} ms-1`;
            
            sortClients(column, currentClientSort.direction);
        });
    });
    
    // Client filter functionality
    const filterItems = document.querySelectorAll('[data-filter]');
    filterItems.forEach(item => {
        item.addEventListener('click', function(e) {
            e.preventDefault();
            currentClientFilter = this.dataset.filter;
            
            // Update dropdown button text
            const filterDropdown = document.getElementById('filterDropdown');
            if (filterDropdown) {
                filterDropdown.innerHTML = `<i class="fas fa-filter me-2"></i>${this.textContent}`;
            }
            
            filterClients(clientSearch?.value.toLowerCase() || '', currentClientFilter);
        });
    });
    
    // Client pagination functionality
    const clientPaginationLinks = document.querySelectorAll('.pagination .page-link');
    clientPaginationLinks.forEach(link => {
        if (!link.textContent.trim().match(/^\d+$/)) return;
        
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const page = parseInt(this.textContent.trim());
            if (!isNaN(page)) {
                currentClientPage = page;
                updateClientPagination();
            }
        });
    });
    
    const prevClientPageBtn = document.getElementById('client-prev-page');
    if (prevClientPageBtn) {
        prevClientPageBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (!this.parentElement.classList.contains('disabled') && currentClientPage > 1) {
                currentClientPage--;
                updateClientPagination();
            }
        });
    }
    
    const nextClientPageBtn = document.getElementById('client-next-page');
    if (nextClientPageBtn) {
        nextClientPageBtn.addEventListener('click', function(e) {
            e.preventDefault();
            const maxPage = Math.ceil(getVisibleClientRows().length / clientsPerPage);
            if (!this.parentElement.classList.contains('disabled') && currentClientPage < maxPage) {
                currentClientPage++;
                updateClientPagination();
            }
        });
    }
    
    // Client details modal functionality
    const clientRows = document.querySelectorAll('#clientsTable tbody tr');
    const clientDetailsModal = new bootstrap.Modal(document.getElementById('clientDetailsModal'), {
        keyboard: true,
        backdrop: true
    });
    
    clientRows.forEach(row => {
        row.addEventListener('click', function(e) {
            const isActionButton = e.target.closest('a') || e.target.closest('button');
            if (!isActionButton) {
                const clientId = this.querySelector('td:nth-child(1)').textContent.match(/ID: (\d+)/)[1];
                loadClientDetails(clientId);
                clientDetailsModal.show();
            }
        });
    });
    
    function loadClientDetails(clientId) {
        // This would normally be an AJAX call to the server
        // For demo purposes, we'll simulate loading client details
        const clientDetailsContent = document.getElementById('clientDetailsContent');
        
        // Simulate loading state
        clientDetailsContent.innerHTML = `
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2">Loading client details...</p>
            </div>
        `;
        
        // Simulate delay and then show dummy data
        setTimeout(() => {
            // Find the client row for this ID
            const clientRow = document.querySelector(`tbody tr td:contains("ID: ${clientId}")`).closest('tr');
            
            if (clientRow) {
                const clientName = clientRow.querySelector('td:nth-child(1) .fw-bold').textContent;
                const clientEmail = clientRow.querySelector('td:nth-child(2)').textContent.match(/([a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6})/)[0];
                const clientPhone = clientRow.querySelector('td:nth-child(2)').textContent.match(/(\+\d{1,3})?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}/)[0];
                const regDate = clientRow.querySelector('td:nth-child(3) .fw-medium').textContent;
                
                // Update edit client button
                document.getElementById('editClientBtn').href = `/Clients/Edit/${clientId}`;
                
                // Update modal content with client information
                clientDetailsContent.innerHTML = `
                    <div class="row">
                        <div class="col-md-4 mb-4 mb-md-0">
                            <div class="text-center mb-4">
                                <div class="avatar-initial rounded-circle d-inline-flex align-items-center justify-content-center bg-gradient mb-3"
                                    style="width: 100px; height: 100px; background: linear-gradient(45deg, #4158D0, #C850C0);">
                                    <span class="display-4 text-white fw-bold">
                                        ${clientName.split(' ')[0][0]}${clientName.split(' ')[1][0]}
                                    </span>
                                </div>
                                <h4 class="fw-bold">${clientName}</h4>
                                <p class="text-muted mb-0">
                                    <span class="badge bg-success">Active</span>
                                </p>
                            </div>
                            <hr>
                            <div class="mb-3">
                                <h5 class="fw-bold">Quick Actions</h5>
                                <div class="d-grid gap-2">
                                    <a href="/Loans/Create?clientId=${clientId}" class="btn btn-primary">
                                        <i class="fas fa-plus-circle me-2"></i>New Loan
                                    </a>
                                    <a href="/Transactions/Create?clientId=${clientId}" class="btn btn-outline-primary">
                                        <i class="fas fa-exchange-alt me-2"></i>New Transaction
                                    </a>
                                    <a href="/Clients/Documents/${clientId}" class="btn btn-outline-primary">
                                        <i class="fas fa-file-alt me-2"></i>Documents
                                    </a>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-8">
                            <h5 class="fw-bold mb-3">Contact Information</h5>
                            <div class="mb-4">
                                <div class="row mb-2">
                                    <div class="col-md-4 text-md-end text-muted">Email:</div>
                                    <div class="col-md-8 fw-medium">${clientEmail}</div>
                                </div>
                                <div class="row mb-2">
                                    <div class="col-md-4 text-md-end text-muted">Phone:</div>
                                    <div class="col-md-8 fw-medium">${clientPhone}</div>
                                </div>
                                <div class="row mb-2">
                                    <div class="col-md-4 text-md-end text-muted">Registered On:</div>
                                    <div class="col-md-8 fw-medium">${regDate}</div>
                                </div>
                                <div class="row mb-2">
                                    <div class="col-md-4 text-md-end text-muted">Address:</div>
                                    <div class="col-md-8 fw-medium">123 Sample Street, City, Country</div>
                                </div>
                            </div>
                            
                            <h5 class="fw-bold mb-3">Financial Summary</h5>
                            <div class="row g-3 mb-4">
                                <div class="col-md-6">
                                    <div class="card border-0 shadow-sm rounded-4">
                                        <div class="card-body">
                                            <h6 class="text-muted mb-1">Active Loans</h6>
                                            <h4 class="fw-bold mb-0">$3,500.00</h4>
                                            <div class="progress mt-2" style="height: 6px;">
                                                <div class="progress-bar" role="progressbar" style="width: 40%;" aria-valuenow="40" aria-valuemin="0" aria-valuemax="100"></div>
                                            </div>
                                            <small class="text-muted">40% repaid</small>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="card border-0 shadow-sm rounded-4">
                                        <div class="card-body">
                                            <h6 class="text-muted mb-1">Savings Balance</h6>
                                            <h4 class="fw-bold mb-0">$1,250.00</h4>
                                            <div class="progress mt-2" style="height: 6px;">
                                                <div class="progress-bar bg-success" role="progressbar" style="width: 75%;" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100"></div>
                                            </div>
                                            <small class="text-muted">75% of target</small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            
                            <h5 class="fw-bold mb-3">Recent Activity</h5>
                            <div class="list-group list-group-flush border-top">
                                <div class="list-group-item d-flex align-items-center px-0 py-3">
                                    <div class="avatar-initial rounded-circle bg-light d-flex align-items-center justify-content-center me-3" style="width: 45px; height: 45px;">
                                        <i class="fas fa-arrow-down text-success"></i>
                                    </div>
                                    <div class="flex-grow-1">
                                        <div class="d-flex align-items-center justify-content-between mb-1">
                                            <h6 class="mb-0">Loan Repayment</h6>
                                            <span class="text-success fw-bold">$250.00</span>
                                        </div>
                                        <div class="text-muted small">Yesterday at 3:45 PM</div>
                                    </div>
                                </div>
                                <div class="list-group-item d-flex align-items-center px-0 py-3">
                                    <div class="avatar-initial rounded-circle bg-light d-flex align-items-center justify-content-center me-3" style="width: 45px; height: 45px;">
                                        <i class="fas fa-arrow-up text-primary"></i>
                                    </div>
                                    <div class="flex-grow-1">
                                        <div class="d-flex align-items-center justify-content-between mb-1">
                                            <h6 class="mb-0">Savings Deposit</h6>
                                            <span class="text-primary fw-bold">$100.00</span>
                                        </div>
                                        <div class="text-muted small">May 23, 2025 at 10:30 AM</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            } else {
                clientDetailsContent.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Client information not found.
                    </div>
                `;
            }
        }, 800);
    }
    
    // Filter clients
    function filterClients(searchTerm, filter) {
        const rows = document.querySelectorAll('#clientsTable tbody tr');
        let visibleCount = 0;
        
        rows.forEach(row => {
            const name = row.querySelector('td:nth-child(1)').textContent.toLowerCase();
            const contact = row.querySelector('td:nth-child(2)').textContent.toLowerCase();
            const regDate = row.querySelector('td:nth-child(3)').textContent.toLowerCase();
            const status = row.querySelector('td:nth-child(4)').textContent.toLowerCase();
            
            const matchesSearch = name.includes(searchTerm) || 
                               contact.includes(searchTerm) ||
                               regDate.includes(searchTerm) ||
                               status.includes(searchTerm);
            
            // Apply filters
            let matchesFilter = true;
            const today = new Date();
            const hasLoans = row.dataset.hasLoans === "True";
            const regDateValue = row.dataset.registrationDate ? new Date(row.dataset.registrationDate) : null;
            
            switch (filter) {
                case 'hasLoans':
                    matchesFilter = hasLoans;
                    break;
                case 'noLoans':
                    matchesFilter = !hasLoans;
                    break;
                case 'thisMonth':
                    matchesFilter = regDateValue && 
                                 regDateValue.getMonth() === today.getMonth() && 
                                 regDateValue.getFullYear() === today.getFullYear();
                    break;
                case 'thisYear':
                    matchesFilter = regDateValue && regDateValue.getFullYear() === today.getFullYear();
                    break;
                default:
                    matchesFilter = true;
            }
            
            const visible = matchesSearch && matchesFilter;
            row.style.display = visible ? '' : 'none';
            if (visible) visibleCount++;
        });
        
        // Reset pagination
        currentClientPage = 1;
        updateClientPagination();
        
        // Update showing text
        if (document.getElementById('client-total-items')) {
            document.getElementById('client-total-items').textContent = visibleCount;
        }
    }
    
    // Sort clients
    function sortClients(column, direction) {
        const rows = Array.from(document.querySelectorAll('#clientsTable tbody tr'));
        
        rows.sort((rowA, rowB) => {
            let valueA, valueB;
            
            switch (column) {
                case 'name':
                    valueA = rowA.querySelector('td:nth-child(1) .fw-bold').textContent.trim();
                    valueB = rowB.querySelector('td:nth-child(1) .fw-bold').textContent.trim();
                    break;
                case 'date':
                    valueA = new Date(rowA.dataset.registrationDate || '');
                    valueB = new Date(rowB.dataset.registrationDate || '');
                    break;
                case 'status':
                    valueA = rowA.querySelector('td:nth-child(4)').textContent.trim();
                    valueB = rowB.querySelector('td:nth-child(4)').textContent.trim();
                    break;
                default:
                    return 0;
            }
            
            if (valueA < valueB) {
                return direction === 'asc' ? -1 : 1;
            }
            if (valueA > valueB) {
                return direction === 'asc' ? 1 : -1;
            }
            return 0;
        });
        
        // Reattach rows in new order
        const tbody = document.querySelector('#clientsTable tbody');
        rows.forEach(row => tbody.appendChild(row));
        
        // Reset pagination
        currentClientPage = 1;
        updateClientPagination();
    }
    
    // Get visible client rows
    function getVisibleClientRows() {
        return Array.from(document.querySelectorAll('#clientsTable tbody tr')).filter(row => 
            row.style.display !== 'none'
        );
    }
    
    // Update client pagination
    function updateClientPagination() {
        const visibleRows = getVisibleClientRows();
        const totalItems = visibleRows.length;
        const maxPage = Math.ceil(totalItems / clientsPerPage);
        
        // Update page links
        const pageLinks = document.querySelectorAll('.pagination .page-item:not(.disabled):not(#client-prev-page):not(#client-next-page)');
        pageLinks.forEach((link, index) => {
            if (index + 1 <= maxPage) {
                link.style.display = '';
                if (index + 1 === currentClientPage) {
                    link.classList.add('active');
                } else {
                    link.classList.remove('active');
                }
            } else {
                link.style.display = 'none';
            }
        });
        
        // Update prev/next buttons
        if (prevClientPageBtn) {
            prevClientPageBtn.parentElement.classList.toggle('disabled', currentClientPage === 1);
        }
        if (nextClientPageBtn) {
            nextClientPageBtn.parentElement.classList.toggle('disabled', currentClientPage === maxPage || maxPage === 0);
        }
        
        // Update showing text
        if (document.getElementById('client-showing-start') && document.getElementById('client-showing-end')) {
            const start = Math.min((currentClientPage - 1) * clientsPerPage + 1, totalItems);
            const end = Math.min(currentClientPage * clientsPerPage, totalItems);
            document.getElementById('client-showing-start').textContent = totalItems > 0 ? start : 0;
            document.getElementById('client-showing-end').textContent = end;
        }
        
        // Show/hide rows based on pagination
        visibleRows.forEach((row, index) => {
            const rowNum = index + 1;
            const showOnThisPage = rowNum > (currentClientPage - 1) * clientsPerPage && rowNum <= currentClientPage * clientsPerPage;
            row.style.display = showOnThisPage ? '' : 'none';
        });
    }
    
    // Client export functionality
    const exportBtn = document.getElementById('exportBtn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function() {
            exportClientsToCSV('clients.csv');
        });
    }
    
    // Function to export clients to CSV
    function exportClientsToCSV(filename) {
        const rows = getVisibleClientRows();
        let csv = [];
        
        // Add header row
        const headers = ['Client Name', 'Email', 'Phone', 'Registration Date', 'Status'];
        csv.push(headers.join(','));
        
        // Add data rows
        rows.forEach(row => {
            const name = row.querySelector('td:nth-child(1) .fw-bold').textContent.trim();
            const email = row.querySelector('td:nth-child(2)').textContent.match(/([a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6})/)?.[0] || '';
            const phone = row.querySelector('td:nth-child(2)').textContent.match(/(\+\d{1,3})?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}/)?.[0] || '';
            const regDate = row.querySelector('td:nth-child(3) .fw-medium').textContent.trim();
            const status = row.querySelector('td:nth-child(4) span').textContent.trim();
            
            csv.push([`"${name}"`, `"${email}"`, `"${phone}"`, `"${regDate}"`, `"${status}"`].join(','));
        });
        
        // Create download link
        const csvContent = csv.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
    
    // Initialize pagination
    updateClientPagination();
    
    // Add JQuery extension for contains case insensitive
    if (typeof jQuery !== 'undefined') {
        jQuery.expr[':'].contains = function(a, i, m) {
            return jQuery(a).text().toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
        };
    }
});

// Counter animation for statistics
document.addEventListener('DOMContentLoaded', function() {
    const counters = document.querySelectorAll('.stats-counter');
    
    const startCounting = (entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counter = entry.target;
                const target = parseInt(counter.getAttribute('data-target'));
                const duration = 2000; // 2 seconds
                const step = target / (duration / 20); // 20ms per step
                
                let current = 0;
                const timer = setInterval(() => {
                    current += step;
                    if (current >= target) {
                        counter.textContent = formatNumber(target);
                        clearInterval(timer);
                    } else {
                        counter.textContent = formatNumber(Math.floor(current));
                    }
                }, 20);
                
                counter.classList.add('counter-animated');
                observer.unobserve(counter);
            }
        });
    };
    
    function formatNumber(num) {
        return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
    }
    
    if ('IntersectionObserver' in window && counters.length > 0) {
        const options = {
            threshold: 0.5
        };
        
        const observer = new IntersectionObserver(startCounting, options);
        counters.forEach(counter => observer.observe(counter));
    }
});
