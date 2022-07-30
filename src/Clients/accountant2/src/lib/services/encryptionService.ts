export class EncryptionService {
	private readonly encoder = new TextEncoder();
	private readonly decoder = new TextDecoder();

	async encrypt(data: string, password: string): Promise<EncryptionResult> {
		const salt = window.crypto.getRandomValues(new Uint8Array(16));
		const nonce = window.crypto.getRandomValues(new Uint8Array(16));
		const key = await this.getKey(password, salt);
		const encodedData = this.encoder.encode(data);

		const encryptedData = await window.crypto.subtle.encrypt(
			{
				name: 'AES-GCM',
				iv: nonce
			},
			key,
			encodedData
		);

		return new EncryptionResult(
			this.arrayBufferToBase64(encryptedData),
			this.arrayBufferToBase64(salt),
			this.arrayBufferToBase64(nonce)
		);
	}

	async decrypt(data: string, salt: string, nonce: string, password: string): Promise<string> {
		const dataArray = this.stringToUInt8Array(data);
		const saltArray = this.stringToUInt8Array(salt);
		const nonceArray = this.stringToUInt8Array(nonce);

		const key = await this.getKey(password, saltArray);

		const decrypted = await window.crypto.subtle.decrypt(
			{
				name: 'AES-GCM',
				iv: nonceArray
			},
			key,
			dataArray
		);

		return this.decoder.decode(decrypted);
	}

	private async getKey(password: string, salt: Uint8Array) {
		const cryptoKey = await window.crypto.subtle.importKey('raw', this.encoder.encode(password), 'PBKDF2', false, [
			'deriveBits',
			'deriveKey'
		]);

		return window.crypto.subtle.deriveKey(
			{
				name: 'PBKDF2',
				salt: salt,
				iterations: 1000,
				hash: 'SHA-256'
			},
			cryptoKey,
			{
				name: 'AES-GCM',
				length: 256
			},
			true,
			['encrypt', 'decrypt']
		);
	}

	private arrayBufferToBase64(buffer: any): string {
		let binary = '';
		const bytes = new Uint8Array(buffer);
		const len = bytes.byteLength;
		for (let i = 0; i < len; i++) {
			binary += String.fromCharCode(bytes[i]);
		}
		return window.btoa(binary);
	}

	private stringToUInt8Array(base64String: string): Uint8Array {
		const rawString = window.atob(base64String);
		const uint8Array = new Uint8Array(rawString.length);
		for (let i = 0; i < rawString.length; i++) {
			uint8Array[i] = rawString.charCodeAt(i);
		}
		return uint8Array;
	}
}

export class EncryptionResult {
	constructor(public encryptedData: string, public salt: string, public nonce: string) {}
}
