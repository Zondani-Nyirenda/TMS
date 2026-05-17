/* ================================================
   TMS Service Worker - FINAL FIXED VERSION
   Cache version: tms-cache-v3
   ================================================ */

const CACHE_NAME = 'tms-cache-v3';
const OFFLINE_URL = '/offline.html';

const PRECACHE_URLS = [
    '/',
    '/index.html',
    '/offline.html',
    '/manifest.json',
    '/css/app.css',
];

// ── Install ────────────────────────────────────────────────────────────────
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(PRECACHE_URLS))
    );
    self.skipWaiting();
});

// ── Activate ───────────────────────────────────────────────────────────────
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        )
    );
    self.clients.claim();
});

// ── Fetch Handler ──────────────────────────────────────────────────────────
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    if (event.request.method !== 'GET') return;

    // ==================== API CALLS ====================
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(event.request)
                .catch(() => {
                    return new Response(
                        JSON.stringify({ success: false, message: 'You are offline.' }),
                        {
                            status: 503,
                            headers: { 'Content-Type': 'application/json' }
                        }
                    );
                })
        );
        return;
    }

    // ==================== BLAZOR WASM FILES ====================
    if (url.pathname.startsWith('/_framework/') ||
        url.pathname.startsWith('/_content/')) {

        event.respondWith(
            caches.match(event.request).then(cached => {
                if (cached) return cached;

                return fetch(event.request).then(response => {
                    // Safe clone only if response is valid
                    if (!response || response.status !== 200) return response;

                    const responseClone = response.clone();
                    caches.open(CACHE_NAME).then(cache => {
                        cache.put(event.request, responseClone);
                    });
                    return response;
                });
            })
        );
        return;
    }

    // ==================== NAVIGATION (SPA) ====================
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() =>
                caches.match(OFFLINE_URL).then(r => r || caches.match('/index.html'))
            )
        );
        return;
    }

    // ==================== STATIC ASSETS (Stale-While-Revalidate) ====================
    event.respondWith(
        caches.match(event.request).then(cachedResponse => {
            // Return cached immediately if available
            if (cachedResponse) {
                // Update cache in background
                fetch(event.request).then(networkResponse => {
                    if (networkResponse && networkResponse.status === 200) {
                        const clone = networkResponse.clone();
                        caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                    }
                }).catch(() => { }); // silent fail
                return cachedResponse;
            }

            // No cache → fetch and cache
            return fetch(event.request).then(networkResponse => {
                if (networkResponse && networkResponse.status === 200) {
                    const clone = networkResponse.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                }
                return networkResponse;
            });
        })
    );
});

// ── Background Sync ───────────────────────────────────────────────────────
self.addEventListener('sync', event => {
    if (event.tag === 'sync-attendance') {
        event.waitUntil(syncOfflineAttendance());
    }
});

async function syncOfflineAttendance() {
    try {
        const db = await openOfflineDb();
        const records = await getAllOfflineRecords(db);

        if (!records?.length) return;

        const response = await fetch('/api/attendance/sync', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(records)
        });

        if (response.ok) {
            await clearOfflineRecords(db);
            self.clients.matchAll().then(clients => {
                clients.forEach(c => c.postMessage({
                    type: 'ATTENDANCE_SYNCED',
                    count: records.length
                }));
            });
        }
    } catch (err) {
        console.error('[SW] Sync failed:', err);
    }
}

// ── Push Notifications ─────────────────────────────────────────────────────
self.addEventListener('push', event => {
    const data = event.data?.json() ?? {};
    event.waitUntil(
        self.registration.showNotification(data.title ?? 'TMS', {
            body: data.body ?? '',
            icon: '/icon/icon-192.png',
            badge: '/icon/badge-72.png',
            data: { url: data.actionUrl ?? '/' }
        })
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const url = event.notification.data?.url ?? '/';
    event.waitUntil(clients.openWindow(url));
});

// ── IndexedDB Helpers ──────────────────────────────────────────────────────
function openOfflineDb() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open('tms-offline', 1);
        req.onupgradeneeded = e => e.target.result.createObjectStore('attendance', { keyPath: 'localId' });
        req.onsuccess = e => resolve(e.target.result);
        req.onerror = e => reject(e.target.error);
    });
}

function getAllOfflineRecords(db) {
    return new Promise((resolve, reject) => {
        const tx = db.transaction('attendance', 'readonly');
        tx.objectStore('attendance').getAll().onsuccess = e => resolve(e.target.result);
        tx.onerror = e => reject(e.target.error);
    });
}

function clearOfflineRecords(db) {
    return new Promise((resolve, reject) => {
        const tx = db.transaction('attendance', 'readwrite');
        tx.objectStore('attendance').clear().onsuccess = () => resolve();
        tx.onerror = e => reject(e.target.error);
    });
}