// pwaInstall.js
// Registers the service worker from wwwroot/js/service-worker.js
// with scope '/' so it controls the whole app.
// Using { scope: '/' } requires the server to send the header:
//   Service-Worker-Allowed: /
// on the service-worker.js response.
// In development Blazor's dev server does this automatically.
// In production add it to your web.config / nginx.conf (see README).

(function () {
    if (!('serviceWorker' in navigator)) {
        console.info('[PWA] Service workers not supported in this browser.');
        return;
    }

    window.addEventListener('load', () => {
        navigator.serviceWorker
            .register('/js/service-worker.js', { scope: '/' })
            .then(reg => {
                console.info('[PWA] Service worker registered. Scope:', reg.scope);

                // Detect when a new SW is waiting and prompt user to refresh
                reg.addEventListener('updatefound', () => {
                    const newWorker = reg.installing;
                    if (!newWorker) return;

                    newWorker.addEventListener('statechange', () => {
                        if (newWorker.state === 'installed' &&
                            navigator.serviceWorker.controller) {
                            // New version available — tell Blazor if needed
                            console.info('[PWA] New version available. Refresh to update.');
                            // Optional: dispatch a custom event for a Blazor snackbar
                            window.dispatchEvent(new CustomEvent('pwa-update-available'));
                        }
                    });
                });
            })
            .catch(err => {
                console.warn('[PWA] Service worker registration failed:', err);
            });

        // Listen for messages from the SW (e.g. ATTENDANCE_SYNCED)
        navigator.serviceWorker.addEventListener('message', event => {
            const { type, count } = event.data ?? {};
            if (type === 'ATTENDANCE_SYNCED') {
                console.info(`[PWA] ${count} offline attendance record(s) synced.`);
                window.dispatchEvent(
                    new CustomEvent('attendance-synced', { detail: { count } })
                );
            }
        });
    });
})();