/**
 * Database Connectivity Helper Functions
 * 
 * This file contains utility functions for testing and managing database connections
 * in the Microfinance App.
 */

/**
 * Checks the database connection status by calling the connection test endpoint
 * @param {function} callback - Function to call with the results
 */
function checkDatabaseStatus(callback) {
    // Try the new connection-test endpoint first
    fetch('/connection-test')
        .then(response => {
            if (!response.ok) {
                throw new Error('Connection test failed - trying fallback');
            }
            return response.json();
        })
        .then(data => {
            callback(null, {
                DatabaseConnection: data.CanConnect ? 'Success' : 'Failed',
                DatabaseProvider: data.ProviderName,
                Error: data.ErrorMessage,
                InnerError: data.InnerErrorMessage,
                PendingMigrations: data.PendingMigrations
            });
        })
        .catch(error => {
            // Fall back to the diagnostics endpoint
            console.log('Falling back to diagnostics endpoint:', error);
            fetch('/Home/Diagnostics')
                .then(response => response.json())
                .then(data => {
                    callback(null, data);
                })
                .catch(error => {
                    callback(error, null);
                });
        });
}

/**
 * Display database connection information in the specified element
 * @param {string} elementId - The ID of the element to update
 */
function displayDatabaseInfo(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    element.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';
    
    checkDatabaseStatus((error, data) => {
        if (error) {
            element.innerHTML = `
                <div class="alert alert-danger">
                    <h5 class="alert-heading">Connection Error</h5>
                    <p>${error.message}</p>
                </div>
            `;
            return;
        }
        
        // Create status card based on connection result
        const isConnected = data.DatabaseConnection === 'Success';
        const statusHtml = `
            <div class="card border-0 shadow-sm mb-4">
                <div class="card-header bg-${isConnected ? 'success' : 'danger'} text-white">
                    <h5 class="mb-0">
                        <i class="fas fa-${isConnected ? 'check-circle' : 'times-circle'} me-2"></i>
                        Database Connection: ${isConnected ? 'Successful' : 'Failed'}
                    </h5>
                </div>                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tr>
                                <th>Provider</th>
                                <td>${data.DatabaseProvider || 'Unknown'}</td>
                            </tr>
                            ${data.DatabasePath ? `
                            <tr>
                                <th>Database Path</th>
                                <td>${data.DatabasePath}</td>
                            </tr>
                            ` : ''}
                            ${data.FileExists ? `
                            <tr>
                                <th>File Status</th>
                                <td>${data.FileExists} ${data.FileSize ? `(${data.FileSize})` : ''}</td>
                            </tr>
                            ` : ''}
                            ${data.LastModified ? `
                            <tr>
                                <th>Last Modified</th>
                                <td>${data.LastModified}</td>
                            </tr>
                            ` : ''}
                            ${data.PendingMigrations ? `
                            <tr>
                                <th>Pending Migrations</th>
                                <td>${data.PendingMigrations}</td>
                            </tr>
                            ` : ''}
                            ${data.TableCount ? `
                            <tr>
                                <th>Tables</th>
                                <td>${data.TableCount}</td>
                            </tr>
                            ` : ''}
                            ${data.ClientCount !== undefined ? `
                            <tr>
                                <th>Client Records</th>
                                <td>${data.ClientCount}</td>
                            </tr>
                            ` : ''}
                        </table>
                    </div>
                    
                    ${data.Error ? `
                    <div class="alert alert-warning mt-3">
                        <h6>Error Details</h6>
                        <p class="mb-0">${data.Error}</p>
                        ${data.InnerError ? `<p class="mb-0 mt-1"><small>${data.InnerError}</small></p>` : ''}
                    </div>
                    ` : ''}
                </div>
            </div>
        `;
        
        element.innerHTML = statusHtml;
    });
}

/**
 * Suggest database fixes based on detected issues
 * @param {string} elementId - The ID of the element to update
 */
function suggestDatabaseFixes(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    checkDatabaseStatus((error, data) => {
        if (error || data.DatabaseConnection !== 'Success') {
            // Suggest fixes for connection issues
            element.innerHTML = `
                <div class="card border-0 shadow-sm">
                    <div class="card-header bg-warning">
                        <h5 class="mb-0"><i class="fas fa-tools me-2"></i>Suggested Fixes</h5>
                    </div>
                    <div class="card-body">
                        <ol class="mb-0">
                            <li class="mb-2">Check if SQL Server is running on your machine</li>
                            <li class="mb-2">Verify the connection string in appsettings.json</li>
                            <li class="mb-2">Try switching to LocalDB using the SetupLocalDB.bat script</li>
                            <li>If all else fails, switch to SQLite using SetupSQLiteBackup.bat</li>
                        </ol>
                    </div>
                </div>
            `;
        } else if (data.PendingMigrations && data.PendingMigrations !== 'None') {
            // Suggest applying migrations
            element.innerHTML = `
                <div class="card border-0 shadow-sm">
                    <div class="card-header bg-info text-white">
                        <h5 class="mb-0"><i class="fas fa-info-circle me-2"></i>Migration Required</h5>
                    </div>
                    <div class="card-body">
                        <p>Your database has pending migrations that need to be applied.</p>
                        <p>Run the following command in your project directory:</p>
                        <pre class="bg-light p-3 rounded">dotnet ef database update</pre>
                    </div>
                </div>
            `;
        } else {
            // Everything looks good
            element.innerHTML = `
                <div class="card border-0 shadow-sm">
                    <div class="card-header bg-success text-white">
                        <h5 class="mb-0"><i class="fas fa-check-circle me-2"></i>All Systems Go</h5>
                    </div>
                    <div class="card-body">
                        <p class="mb-0">Your database connection is working properly with no issues detected.</p>
                    </div>
                </div>
            `;
        }
    });
}
