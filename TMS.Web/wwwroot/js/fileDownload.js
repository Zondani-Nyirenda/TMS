window.downloadFileFromStream = async (url, token, fileName) => {
    const response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) {
        console.error('Download failed:', response.status);
        return;
    }

    // Prefer the server-supplied filename from Content-Disposition
    const disposition = response.headers.get('Content-Disposition');
    if (disposition) {
        const match = disposition.match(/filename\*?=(?:UTF-8'')?["']?([^;"'\n]+)["']?/i);
        if (match?.[1]) fileName = decodeURIComponent(match[1].trim());
    }

    const blob = await response.blob();
    const blobUrl = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = fileName || 'download';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(blobUrl);
};