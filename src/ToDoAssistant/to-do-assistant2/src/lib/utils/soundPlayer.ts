export class SoundPlayer {
	private context: AudioContext | undefined;
	private bleep: AudioBuffer | null = null;
	private blop: AudioBuffer | null = null;

	async playBleep() {
		if (!this.bleep) {
			await this.initialize();
		}

		this.play(<AudioBuffer>this.bleep);
	}

	async playBlop() {
		if (!this.blop) {
			await this.initialize();
		}

		this.play(<AudioBuffer>this.blop);
	}

	private async initialize() {
		this.context = new AudioContext();

		const bleepPromise = window.fetch('/audio/bleep.mp3').then(async (response) => {
			const arrayBuffer = await response.arrayBuffer();
			this.bleep = await (<AudioContext>this.context).decodeAudioData(arrayBuffer);
		});
		const blopPromise = window.fetch('/audio/blop.mp3').then(async (response) => {
			const arrayBuffer = await response.arrayBuffer();
			this.blop = await (<AudioContext>this.context).decodeAudioData(arrayBuffer);
		});

		return Promise.all([bleepPromise, blopPromise]);
	}

	private play(audioBuffer: AudioBuffer) {
		if (!this.context) {
			throw 'AudioContext not initialized. Cannot play sound.';
		}

		const source = this.context.createBufferSource();
		source.buffer = audioBuffer;
		source.connect(this.context.destination);
		source.start();
	}
}
