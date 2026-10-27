window.vaultCrypto = {
    async encrypt(keyBase64, plaintext) {
        const keyBytes = Uint8Array.from(atob(keyBase64), c => c.charCodeAt(0));
        const key = await crypto.subtle.importKey(
            'raw', keyBytes, { name: 'AES-GCM', length: 256 }, false, ['encrypt']);
        const iv = crypto.getRandomValues(new Uint8Array(12));
        const ciphertext = await crypto.subtle.encrypt(
            { name: 'AES-GCM', iv }, key, new TextEncoder().encode(plaintext));
        const result = new Uint8Array(12 + ciphertext.byteLength);
        result.set(iv);
        result.set(new Uint8Array(ciphertext), 12);
        return btoa(String.fromCharCode(...result));
    },

    async decrypt(keyBase64, encryptedBase64) {
        const data = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));
        const keyBytes = Uint8Array.from(atob(keyBase64), c => c.charCodeAt(0));
        const key = await crypto.subtle.importKey(
            'raw', keyBytes, { name: 'AES-GCM', length: 256 }, false, ['decrypt']);
        const plaintext = await crypto.subtle.decrypt(
            { name: 'AES-GCM', iv: data.slice(0, 12) }, key, data.slice(12));
        return new TextDecoder().decode(plaintext);
    }
};
