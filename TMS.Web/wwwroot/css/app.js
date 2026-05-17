// ── Theme persistence ─────────────────────────────────────────────
window.tmsApp = {
    getTheme: () => localStorage.getItem('tms-theme') || 'light',
    setTheme: (t) => localStorage.setItem('tms-theme', t),

    // ── Chart helpers (called from Blazor) ───────────────────────
    printElement: (id) => {
        const el = document.getElementById(id);
        if (!el) return;
        const w = window.open('', '_blank');
        w.document.write('<html><head><title>Print</title></head><body>');
        w.document.write(el.innerHTML);
        w.document.write('</body></html>');
        w.document.close();
        w.print();
    },

    // ── Clipboard ────────────────────────────────────────────────
    copyToClipboard: (text) => navigator.clipboard.writeText(text),

    // ── Scroll ───────────────────────────────────────────────────
    scrollToTop: () => window.scrollTo({ top: 0, behavior: 'smooth' }),

    // ── PWA install prompt ───────────────────────────────────────
    installPrompt: null
};

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    window.tmsApp.installPrompt = e;
});